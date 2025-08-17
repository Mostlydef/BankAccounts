using BankAccounts.Database;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace BankAccounts.Infrastructure.Rabbit.Consumers;

/// <summary>
/// BackgroundService, который слушает очередь RabbitMQ "account.audit" и записывает события в таблицу AuditEvents.
/// Реализует идемпотентность через таблицу InboxConsumed.
/// </summary>
public class AuditConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnectionFactory _factory;
    private IConnection? _connection;
    private IChannel? _channel;
    /// <summary>
    /// Название очереди RabbitMQ для аудита
    /// </summary>
    private const string QueueName = "account.audit";

    /// <summary>
    /// Конструктор, принимает factory для подключения к RabbitMQ и scopeFactory для создания DbContext
    /// </summary>
    public AuditConsumer(IServiceScopeFactory scopeFactory, IConnectionFactory factory)
    {
        _scopeFactory = scopeFactory;
        _factory = factory;
    }

    /// <summary>
    /// Основной метод сервиса, запускается при старте приложения
    /// </summary>
    /// <param name="stoppingToken">Токен для отмены</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Создаем подключение и канал к RabbitMQ
        _connection = await _factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // QoS: получаем по одному сообщению за раз
        await _channel.BasicQosAsync(0, 1, false, stoppingToken);
        var consumer = new AsyncEventingBasicConsumer(_channel);

        // Обработчик получения сообщений
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {

                // Получаем MessageId из свойств RabbitMQ
                var messageIdString = eventArgs.BasicProperties.MessageId;
                if (messageIdString == null)
                    throw new InvalidOperationException("MessageId missing");

                var messageId = Guid.Parse(messageIdString);

                // Декодируем тело сообщения из UTF8
                var payload = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                // Создаем scope для получения DbContext
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Обрабатываем сообщение (записываем в audit_events и InboxConsumed)
                await HandleMessage(context, payload, messageId, stoppingToken);

                // Подтверждаем успешное получение сообщения
                await _channel.BasicAckAsync(eventArgs.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AuditConsumer error: {ex}");
            }
        };

        // Запускаем прослушивание очереди
        await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, stoppingToken);
    }

    /// <summary>
    /// Обработка одного сообщения из очереди
    /// </summary>
    /// <param name="context">DbContext для доступа к базе</param>
    /// <param name="payload">Содержимое сообщения</param>
    /// <param name="messageId">Идентификатор сообщения</param>
    /// <param name="ct">Токен отмены</param>
    private async Task HandleMessage(AppDbContext context, string payload, Guid messageId, CancellationToken ct)
    {
        // Проверка идемпотентности
        var alreadyProcessed = await context.InboxConsumed.AnyAsync(x =>
            x.MessageId == messageId && x.Handler == nameof(AuditConsumer), ct);

        if (alreadyProcessed) return;

        // Записываем событие в audit_events
        var audit = new AuditEvent
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            Handler = nameof(AuditConsumer),
            Payload = payload,
            ReceivedAt = DateTimeOffset.UtcNow
        };
        context.AuditEvents.Add(audit);

        // Записываем запись о том, что сообщение обработано
        context.InboxConsumed.Add(new InboxConsumed
        {
            MessageId = messageId,
            Handler = nameof(AuditConsumer),
            ProcessedAt = DateTimeOffset.UtcNow
        });

        // Сохраняем изменения в базе
        await context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Освобождает ресурсы RabbitMQ при остановке сервиса
    /// </summary>
    public override void Dispose()
    {
        _channel?.CloseAsync();
        _connection?.CloseAsync();
        base.Dispose();
    }
}