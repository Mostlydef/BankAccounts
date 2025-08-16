using System.Diagnostics;
using System.Text.Json;
using BankAccounts.Database;
using BankAccounts.Infrastructure.Rabbit.PublishEvents;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Infrastructure.Rabbit.Outbox
{

    /// <summary>
    /// Фоновый сервис для публикации сообщений из таблицы Outbox в RabbitMQ.
    /// Реализует паттерн Outbox: сначала сообщение сохраняется в БД,
    /// а потом отдельный процесс его публикует.
    /// </summary>
    public class OutboxDispatcher : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxDispatcher> _logger;
        private readonly IRabbitMqPublisher _publisher;
        /// <summary>
        /// Фоновый сервис для публикации сообщений из таблицы Outbox в RabbitMQ.
        /// Реализует паттерн Outbox: сначала сообщение сохраняется в БД,
        /// а потом отдельный процесс его публикует.
        /// </summary>
        private const int BatchSize = 100;
        /// <summary>
        /// Базовая задержка перед повторной попыткой публикации (экспоненциальный backoff).
        /// </summary>
        private static readonly TimeSpan BaseDelay = TimeSpan.FromSeconds(5);
        /// <summary>
        /// Максимальное количество попыток публикации перед переводом в dead-letter.
        /// </summary>
        private const int MaxAttempts = 10;

        /// <summary>
        /// Конструктор сервиса публикации сообщений.
        /// </summary>
        public OutboxDispatcher(IServiceProvider serviceProvider, ILogger<OutboxDispatcher> logger, IRabbitMqPublisher publisher)
        {
            _serviceProvider = serviceProvider; 
            _logger = logger; 
            _publisher = publisher;
        }

        /// <summary>
        /// Основной цикл фонового сервиса.
        /// Выбирает из базы "подвисшие" сообщения и публикует их в RabbitMQ.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                int processed = 0;

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // взять пачку сообщений на отправку
                    var now = DateTimeOffset.UtcNow;
                    var messageList = await db.OutboxMessages
                        .Where(x => x.Status == nameof(MessageStatus.Pending) || x.Status == nameof(MessageStatus.Failed) && x.NextAttemptAt <= now)
                        .OrderBy(x => x.OccurredAt)
                        .Take(BatchSize)
                        .ToListAsync(stoppingToken);

                    foreach (var message in messageList)
                    {
                        var stopwatch = Stopwatch.StartNew();
                        try
                        {
                            // Помечаем сообщение как "в процессе публикации"
                            message.Status = nameof(MessageStatus.Publishing);
                            await db.SaveChangesAsync(stoppingToken);

                            // Достаём заголовки
                            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(message.Headers);

                            if (headers == null)
                                throw new InvalidOperationException($"Outbox message {message.Id} has invalid or missing headers.");

                            var correlationId = headers.GetValueOrDefault("X-Correlation-Id");
                            var causationId = headers.GetValueOrDefault("X-Causation-Id");
                            var eventType = headers.GetValueOrDefault("X-Event-Type");

                            // Логируем попытку публикации
                            _logger.LogInformation("Publishing outbox message {@EventContext}",
                                new
                                {
                                    EventId = message.Id,
                                    EventType = eventType,
                                    CorrelationId = correlationId,
                                    CausationId = causationId,
                                    Retry = message.Attempts,
                                    message.Status
                                });

                            // Публикация
                            await _publisher.PublishRaw(
                                routingKey: message.RoutingKey,
                                payloadJson: message.Payload,
                                correlationId: headers.GetValueOrDefault("X-Correlation-Id"),
                                causationId: headers.GetValueOrDefault("X-Causation-Id"),
                                messageId: message.Id.ToString() 
                            );
                            stopwatch.Stop();
                            var latency = stopwatch.ElapsedMilliseconds;

                            // Помечаем как успешно опубликованное
                            message.Status = nameof(MessageStatus.Published);
                            message.PublishedAt = DateTimeOffset.UtcNow;
                            await db.SaveChangesAsync(stoppingToken);
                            processed++;

                            // Логируем успешную публикацию
                            _logger.LogInformation("Published outbox message {@EventContext}",
                                new
                                {
                                    EventId = message.Id,
                                    EventType = eventType,
                                    CorrelationId = correlationId,
                                    Retry = message.Attempts,
                                    LatencyMs = latency
                                });
                        }
                        catch (Exception ex)
                        {
                            stopwatch.Stop();
                            var latency = stopwatch.ElapsedMilliseconds;

                            // Обновляем статус и увеличиваем счётчик попыток
                            message.Status = nameof(MessageStatus.Failed);
                            message.Attempts += 1;

                            // Вычисляем задержку перед следующей попыткой (экспоненциальный backoff)
                            var delay = TimeSpan.FromMilliseconds(
                                BaseDelay.TotalMilliseconds * Math.Pow(2, Math.Min(message.Attempts, 8))
                            );
                            message.NextAttemptAt = DateTimeOffset.UtcNow + delay;

                            // Логируем ошибку публикации
                            _logger.LogError(ex, "Failed to publish outbox message {@EventContext}",
                                new
                                {
                                    EventId = message.Id,
                                    Retry = message.Attempts,
                                    LatencyMs = latency,
                                    message.Status
                                });

                            // Если превышено количество попыток — предупреждение
                            if (message.Attempts >= MaxAttempts)
                            {
                                _logger.LogWarning("Outbox message exceeded max attempts {@EventContext}",
                                    new
                                    {
                                        EventId = message.Id,
                                        message.Attempts,
                                        message.Status
                                    });
                            }

                            await db.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Логируем сбой самого фонового цикла
                    _logger.LogError(ex, "Outbox loop failed");
                }

                // Если ничего не обработали — подождём подольше 
                await Task.Delay(processed == 0 ? 1000 : 50, stoppingToken);
            }
        }
    }

}
