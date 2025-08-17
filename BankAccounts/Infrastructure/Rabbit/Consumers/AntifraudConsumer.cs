using BankAccounts.Database;
using BankAccounts.Features.Accounts.Events;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace BankAccounts.Infrastructure.Rabbit.Consumers
{
    /// <summary>
    /// Фоновый сервис для потребления сообщений из очереди Antifraud.
    /// Реализует идемпотентную обработку событий с повторными попытками и поддержкой DLQ (Dead Letter Queue).
    /// </summary>
    /// <remarks>
    /// Конструктор.
    /// </remarks>
    /// <param name="scopeFactory">Фабрика сервисных областей для получения DbContext.</param>
    /// <param name="factory">Фабрика соединений RabbitMQ.</param>
    /// <param name="logger">Логгер.</param>
    public class AntifraudConsumer(
        IServiceScopeFactory scopeFactory,
        IConnectionFactory factory,
        ILogger<AntifraudConsumer> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly IConnectionFactory _factory = factory;
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly ILogger<AntifraudConsumer> _logger = logger;

        /// <summary>
        /// Имя очереди, из которой потребляются события Antifraud.
        /// </summary>
        private const string QueueName = "account.antifraud";

        /// <summary>
        /// Максимальное количество попыток обработки одного сообщения.
        /// </summary>
        private const int MaxRetryCount = 3;

        /// <summary>
        /// Основной метод фонового сервиса. Подключается к очереди, настраивает потребителя и обрабатывает входящие сообщения.
        /// </summary>
        /// <param name="stoppingToken">Токен отмены.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Создаем соединение и канал RabbitMQ
            _connection = await _factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            if (_channel == null)
                return;

            // QoS: обрабатываем по одному сообщению за раз
            await _channel.BasicQosAsync(0, 1, false, stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                _logger.LogInformation("Получено сообщение {DeliveryTag} из очереди {Queue}", eventArgs.DeliveryTag, QueueName);
                try
                {
                    // создаём scope, чтобы взять DbContext
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // Обрабатываем сообщение
                    await HandleMessage(context, eventArgs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке сообщения с DeliveryTag {DeliveryTag}", eventArgs.DeliveryTag);
                    await _channel.BasicAckAsync(eventArgs.DeliveryTag, false, stoppingToken);
                }
            };
            // Подписываемся на очередь
            await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer, stoppingToken);
        }

        /// <summary>
        /// Обработка конкретного сообщения из очереди.
        /// Поддерживает идемпотентность, повторные попытки и DLQ.
        /// </summary>
        /// <param name="context">Контекст базы данных.</param>
        /// <param name="eventArgs">Аргументы события RabbitMQ.</param>
        private async Task HandleMessage(AppDbContext context, BasicDeliverEventArgs eventArgs)
        {
            if (_channel == null)
                throw new InvalidOperationException("RabbitMQ channel is not initialized");
            var stopwatch = Stopwatch.StartNew();
            // Получаем MessageId из свойств RabbitMQ
            var messageIdString = eventArgs.BasicProperties.MessageId;
            if (messageIdString == null)
                throw new InvalidOperationException("MessageId missing");

            var messageId = Guid.Parse(messageIdString);

            // Считываем тело сообщения
            var payloadJson = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            // Ищем запись о предыдущих попытках обработки
            var inbox = await context.InboxConsumed
                .FirstOrDefaultAsync(x => x.MessageId == messageId && x.Handler == nameof(AntifraudConsumer));

            if (inbox == null)
            {
                // Создаем новую запись для отслеживания
                inbox = new InboxConsumed
                {
                    MessageId = messageId,
                    Handler = nameof(AntifraudConsumer),
                    RetryCount = 0
                };
                context.InboxConsumed.Add(inbox);
                await context.SaveChangesAsync();
            }

            if (inbox.ProcessedAt != null)
            {
                stopwatch.Stop();
                _logger.LogInformation("Message already processed {@EventLog}", new
                {
                    MessageId = messageId,
                    inbox.Handler,
                    inbox.RetryCount,
                    LatencyMs = stopwatch.ElapsedMilliseconds
                });
                await _channel.BasicAckAsync(eventArgs.DeliveryTag, false);
                return;
            }

            try
            {
                // Парсим JSON события
                using var doc = JsonDocument.Parse(payloadJson);
                var root = doc.RootElement;

                // Проверяем версию события
                var version = root.GetProperty("Meta").GetProperty("Version").GetString();
                if (version != "v1")
                    throw new InvalidOperationException($"Unsupported event version: {version}");
                var headers = eventArgs.BasicProperties.Headers;
                if (headers == null)
                    throw new InvalidOperationException($"Unsupported event version: {version}");

                var bytesHeader = headers["X-Event-Type"];
                if (bytesHeader == null)
                    throw new InvalidOperationException($"Unsupported event version: {version}");

                var type = Encoding.UTF8.GetString((byte[])bytesHeader);

                // Обработка различных типов событий
                switch (type)
                {
                    case nameof(ClientBlockedEvent):
                        {
                            var payload = root.GetProperty("Payload").Deserialize<ClientBlockedEvent>();
                            if (payload == null)
                                throw new InvalidOperationException("Invalid ClientBlockedEvent payload");

                            // Блокируем счета клиента
                            var accounts = await context.Accounts.Where(a => a.OwnerId == payload.ClientId).ToListAsync();
                            foreach (var account in accounts)
                                account.Frozen = true;
                            break;
                        }

                    case nameof(ClientUnblockedEvent):
                        {
                            var payload = root.GetProperty("Payload").Deserialize<ClientUnblockedEvent>();
                            if (payload == null)
                                throw new InvalidOperationException("Invalid ClientUnblockedEvent payload");

                            // Разблокируем счета клиента
                            var accounts = await context.Accounts.Where(a => a.OwnerId == payload.ClientId).ToListAsync();
                            foreach (var account in accounts) 
                                account.Frozen = false;
                            break;
                        }

                    default:
                        throw new InvalidOperationException($"Unknown event type: {type}");
                }

                // Отмечаем сообщение как обработанное
                inbox.ProcessedAt = DateTimeOffset.UtcNow;
                context.InboxConsumed.Update(inbox);
                await context.SaveChangesAsync();

                await _channel.BasicAckAsync(eventArgs.DeliveryTag, false);

                stopwatch.Stop();
                _logger.LogInformation("Consumed event {@EventLog}", new
                {
                    EventId = messageId,
                    Type = type,
                    CorrelationId = root.GetProperty("Meta").GetProperty("CorrelationId").GetString(),
                    Retry = 0,
                    LatencyMs = stopwatch.ElapsedMilliseconds
                });
            }
            catch (Exception e)
            {
                // Обновляем счетчик попыток
                inbox.RetryCount++;
                await context.SaveChangesAsync();

                if (inbox.RetryCount >= MaxRetryCount)
                {
                    // Помещаем сообщение в DLQ после превышения числа попыток
                    context.InboxDeadLetters.Add(new InboxDeadLetter
                    {
                        MessageId = messageId,
                        ReceivedAt = DateTimeOffset.UtcNow,
                        Handler = nameof(AntifraudConsumer),
                        Payload = payloadJson,
                        Error = e.Message
                    });
                    context.InboxConsumed.Remove(inbox);
                    await context.SaveChangesAsync();

                    if (_channel == null)
                        throw new InvalidOperationException("RabbitMQ channel is not initialized");

                    await _channel.BasicAckAsync(eventArgs.DeliveryTag, false);
                    _logger.LogWarning(e, "Message {MessageId} exceeded max retries, moved to DLQ", messageId);
                }
                else
                {
                    // Повторная попытка: возвращаем в очередь
                    if (_channel == null)
                        throw new InvalidOperationException("RabbitMQ channel is not initialized");
                    await _channel.BasicNackAsync(eventArgs.DeliveryTag, false, true);
                    _logger.LogWarning(e, "Retry {RetryCount} for message {MessageId}", inbox.RetryCount, messageId);
                }
            }
        }

        /// <summary>
        /// Освобождение ресурсов канала и соединения RabbitMQ.
        /// </summary>
        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
