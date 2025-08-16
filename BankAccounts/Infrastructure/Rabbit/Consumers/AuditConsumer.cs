using BankAccounts.Database;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace BankAccounts.Infrastructure.Rabbit.Consumers;

public class AuditConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnectionFactory _factory;
    private IConnection? _connection;
    private IChannel? _channel;

    private const string QueueName = "account.audit";

    public AuditConsumer(IServiceScopeFactory scopeFactory, IConnectionFactory factory)
    {
        _scopeFactory = scopeFactory;
        _factory = factory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection = await _factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.BasicQosAsync(0, 1, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            var messageId = Guid.Parse(eventArgs.BasicProperties.MessageId
                                       ?? throw new InvalidOperationException("MessageId missing"));
            var payload = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await HandleMessage(context, payload, messageId, stoppingToken);

                // ACK после успешного коммита
                await _channel.BasicAckAsync(eventArgs.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AuditConsumer error: {ex}");
            }
        };

        await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, stoppingToken);
    }

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

        // Записываем в InboxConsumed
        context.InboxConsumed.Add(new InboxConsumed
        {
            MessageId = messageId,
            Handler = nameof(AuditConsumer),
            ProcessedAt = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(ct);
    }

    public override void Dispose()
    {
        _channel?.CloseAsync();
        _connection?.CloseAsync();
        base.Dispose();
    }
}