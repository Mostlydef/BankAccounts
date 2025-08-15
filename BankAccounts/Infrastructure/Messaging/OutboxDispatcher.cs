using BankAccounts.Database;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BankAccounts.Infrastructure.Messaging
{
    public class OutboxDispatcher : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<OutboxDispatcher> _log;
        private readonly IRabbitMqPublisher _publisher;
        private const int BatchSize = 100;
        private static readonly TimeSpan BaseDelay = TimeSpan.FromSeconds(5);
        private const int MaxAttempts = 10;

        public OutboxDispatcher(IServiceProvider sp, ILogger<OutboxDispatcher> log, IRabbitMqPublisher publisher)
        {
            _sp = sp; _log = log; _publisher = publisher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int processed;
            while (!stoppingToken.IsCancellationRequested)
            {
                processed = 0;

                try
                {
                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // взять пачку сообщений на отправку
                    var now = DateTimeOffset.UtcNow;
                    var candidates = await db.OutboxMessages
                        .Where(x => x.Status == "Pending" || (x.Status == "Failed" && x.NextAttemptAt <= now))
                        .OrderBy(x => x.OccurredAt)
                        .Take(BatchSize)
                        .ToListAsync(stoppingToken);

                    foreach (var msg in candidates)
                    {
                        try
                        {
                            msg.Status = "Publishing";
                            await db.SaveChangesAsync(stoppingToken);

                            // Достаём заголовки
                            var headers = string.IsNullOrWhiteSpace(msg.Headers)
                                ? new Dictionary<string, string>()
                                : JsonSerializer.Deserialize<Dictionary<string, string>>(msg.Headers)!;

                            // Публикация
                            await _publisher.PublishRaw(
                                routingKey: msg.RoutingKey,
                                payloadJson: msg.Payload,
                                correlationId: headers.GetValueOrDefault("X-Correlation-Id"),
                                causationId: headers.GetValueOrDefault("X-Causation-Id"),
                                messageId: msg.Id.ToString() // важно для Inbox
                            );

                            // Успешно
                            msg.Status = "Published";
                            msg.PublishedAt = DateTimeOffset.UtcNow;
                            await db.SaveChangesAsync(stoppingToken);
                            processed++;
                        }
                        catch (Exception ex)
                        {
                            _log.LogError(ex, "Failed to publish outbox message {MessageId}", msg.Id);
                            msg.Status = "Failed";
                            msg.Attempts += 1;

                            var delay = TimeSpan.FromMilliseconds(BaseDelay.TotalMilliseconds * Math.Pow(2, Math.Min(msg.Attempts, 8)));
                            msg.NextAttemptAt = DateTimeOffset.UtcNow + delay;

                            if (msg.Attempts >= MaxAttempts)
                            {
                                // можно отправить в dead-letter табличку/алертнуть
                                _log.LogWarning("Outbox message {MessageId} exceeded max attempts", msg.Id);
                            }

                            await db.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Outbox loop failed");
                }

                // если ничего не обработали — подождём чуть-чуть
                await Task.Delay(processed == 0 ? 1000 : 50, stoppingToken);
            }
        }
    }

}
