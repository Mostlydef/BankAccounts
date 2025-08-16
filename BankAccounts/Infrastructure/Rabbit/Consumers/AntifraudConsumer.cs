using BankAccounts.Database;
using BankAccounts.Features.Accounts.Events;
using BankAccounts.Infrastructure.Rabbit.Consumers;
using BankAccounts.Infrastructure.Rabbit.PublishEvents;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class AntifraudConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnectionFactory _factory;
    private IConnection _connection;
    private IChannel _channel;
    private const string QueueName = "account.antifraud";

    public AntifraudConsumer(IServiceScopeFactory scopeFactory, IConnectionFactory factory)
    {
        _scopeFactory = scopeFactory;
        _factory = factory;
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
            var messageId = eventArgs.BasicProperties.MessageId
                            ?? throw new InvalidOperationException("MessageId missing");

            var payload = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            try
            {
                // создаём scope, чтобы взять DbContext
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await HandleMessage(context, payload, Guid.Parse(messageId));

                await _channel.BasicAckAsync(eventArgs.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {messageId}: {ex}");
                // сообщение останется в очереди
            }
        };

        await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer, stoppingToken);
    }

    private static async Task HandleMessage(AppDbContext context, string payloadJson, Guid messageId)
    {
        var alreadyProcessed = await context.InboxConsumed
            .AnyAsync(x => x.MessageId == messageId && x.Handler == nameof(AntifraudConsumer));

        if (alreadyProcessed) return;

        var envelope = JsonSerializer.Deserialize<EventEnvelope<object>>(payloadJson);
        if (envelope?.Payload is ClientBlockedEvent blocked)
        {
            var accounts = await context.Accounts.Where(a => a.OwnerId == blocked.ClientId).ToListAsync();
            foreach (var acc in accounts) acc.Frozen = true;
        }
        else if (envelope?.Payload is ClientUnblockedEvent unblocked)
        {
            var accounts = await context.Accounts.Where(a => a.OwnerId == unblocked.ClientId).ToListAsync();
            foreach (var acc in accounts) acc.Frozen = false;
        }

        context.InboxConsumed.Add(new InboxConsumed
        {
            MessageId = messageId,
            Handler = nameof(AntifraudConsumer),
            ProcessedAt = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync();
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
