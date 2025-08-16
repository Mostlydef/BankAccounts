using BankAccounts.Database;
using BankAccounts.Features.Accounts.Events;
using BankAccounts.Infrastructure.Rabbit.Consumers;
using BankAccounts.Infrastructure.Rabbit.PublishEvents;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

public class AntifraudConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnectionFactory _factory;
    private IConnection _connection;
    private IChannel _channel;
    private readonly ILogger<AntifraudConsumer> _logger;
    private const string QueueName = "account.antifraud";
    private const int MaxRetryCount = 3;

    public AntifraudConsumer(IServiceScopeFactory scopeFactory, IConnectionFactory factory, ILogger<AntifraudConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _factory = factory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection = await _factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // QoS
        await _channel.BasicQosAsync(0, 1, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            try
            {
                // создаём scope, чтобы взять DbContext
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await HandleMessage(context, eventArgs);

                await _channel.BasicAckAsync(eventArgs.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                
            }
        };

        await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer, stoppingToken);
    }

    private async Task HandleMessage(AppDbContext context, BasicDeliverEventArgs eventArgs)
    {
        var stopwatch = Stopwatch.StartNew();
        var messageIdString = eventArgs.BasicProperties.MessageId;

        if (messageIdString == null)
            throw new InvalidOperationException("MessageId missing");

        var messageId = Guid.Parse(messageIdString);

        var payloadJson = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

        // ищем запись о предыдущих попытках
        var inbox = await context.InboxConsumed
            .FirstOrDefaultAsync(x => x.MessageId == messageId && x.Handler == nameof(AntifraudConsumer));

        if (inbox == null)
        {
            inbox = new InboxConsumed
            {
                MessageId = messageId,
                Handler = nameof(AntifraudConsumer),
                RetryCount = 0
            };
            context.InboxConsumed.Add(inbox);
            await context.SaveChangesAsync();
        }

        // реализация идемпотентности 
        var alreadyProcessed = await context.InboxConsumed
            .AnyAsync(x => x.MessageId == messageId && x.Handler == nameof(AntifraudConsumer));

        if (alreadyProcessed) 
            return;

        try
        {
            using var doc = JsonDocument.Parse(payloadJson);
            var root = doc.RootElement;

            var version = root.GetProperty("meta").GetProperty("version").GetString();
            if (version != "v1")
                throw new InvalidOperationException($"Unsupported event version: {version}");

            var type = root.GetProperty("meta").GetProperty("type").GetString();
            if (string.IsNullOrWhiteSpace(type))
                throw new InvalidOperationException("Missing event type in envelope");

            switch (type)
            {
                case nameof(ClientBlockedEvent):
                {
                    var payload = root.GetProperty("payload").Deserialize<ClientBlockedEvent>();
                    if (payload == null)
                        throw new InvalidOperationException("Invalid ClientBlockedEvent payload");

                    var accounts = await context.Accounts.Where(a => a.OwnerId == payload.ClientId).ToListAsync();
                    foreach (var acc in accounts) acc.Frozen = true;
                    break;
                }

                case nameof(ClientUnblockedEvent):
                {
                    var payload = root.GetProperty("payload").Deserialize<ClientUnblockedEvent>();
                    if (payload == null)
                        throw new InvalidOperationException("Invalid ClientUnblockedEvent payload");

                    var accounts = await context.Accounts.Where(a => a.OwnerId == payload.ClientId).ToListAsync();
                    foreach (var acc in accounts) acc.Frozen = false;
                    break;
                }

                default:
                    throw new InvalidOperationException($"Unknown event type: {type}");
            }

            inbox.ProcessedAt = DateTimeOffset.UtcNow;
            context.InboxConsumed.Update(inbox);
            await context.SaveChangesAsync();

            stopwatch.Stop();
            _logger.LogInformation("Consumed event {@EventLog}", new
            {
                EventId = messageId,
                Type = type,
                CorrelationId = root.GetProperty("meta").GetProperty("correlationId").GetString(),
                Retry = 0,
                LatencyMs = stopwatch.ElapsedMilliseconds
            });
        }
        catch (Exception e)
        {
            inbox.RetryCount++;
            await context.SaveChangesAsync();

            if (inbox.RetryCount >= MaxRetryCount)
            {
                // помещаем в DLQ и ACK
                context.InboxDeadLetters.Add(new InboxDeadLetter
                {
                    MessageId = messageId,
                    ReceivedAt = DateTimeOffset.UtcNow,
                    Handler = nameof(AntifraudConsumer),
                    Payload = payloadJson,
                    Error = e.Message
                });
                await context.SaveChangesAsync();

                await _channel.BasicAckAsync(eventArgs.DeliveryTag, false);
                _logger.LogWarning(e, "Message {MessageId} exceeded max retries, moved to DLQ", messageId);
            }
            else
            {
                // повторная попытка: возвращаем в очередь
                await _channel.BasicNackAsync(eventArgs.DeliveryTag, false, true);
                _logger.LogWarning(e, "Retry {RetryCount} for message {MessageId}", inbox.RetryCount, messageId);
            }
        }
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
