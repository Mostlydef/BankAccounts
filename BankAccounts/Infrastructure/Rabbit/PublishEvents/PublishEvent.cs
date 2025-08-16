using BankAccounts.Database;
using BankAccounts.Features.Accounts.Events;
using BankAccounts.Features.Transactions.Events;
using BankAccounts.Infrastructure.Rabbit.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;

namespace BankAccounts.Infrastructure.Rabbit.PublishEvents
{
    public class PublishEvent : IPublishEvent
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PublishEvent> _logger;

        public PublishEvent(AppDbContext context, ILogger<PublishEvent> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task PublishEventAsync<T>(T @event, Guid accountId)
        {
            var stopwatch = Stopwatch.StartNew();
            var lastEvent = await _context.OutboxMessages
                .Where(e => EF.Functions.JsonContains(e.Payload, $"{{\"accountId\": \"{accountId}\"}}"))
                .OrderByDescending(e => e.OccurredAt)
                .FirstOrDefaultAsync();

            Guid correlationId = Guid.NewGuid();
            Guid causationId = Guid.NewGuid();

            if (lastEvent != null)
            {
                correlationId = JsonSerializer.Deserialize<EventEnvelope<T>>(lastEvent.Payload)!.Meta.CorrelationId;
            }

            if (lastEvent != null)
            {
                causationId = JsonSerializer.Deserialize<EventEnvelope<T>>(lastEvent.Payload)!.EventId;
            }

            var envelope = new EventEnvelope<T>
            {
                EventId = Guid.NewGuid(),
                OccurredAt = DateTimeOffset.UtcNow.ToString("o"),
                Meta = new EventMeta
                {
                    Version = "v1",
                    Source = "account-service",
                    CorrelationId = correlationId,
                    CausationId = causationId
                },
                Payload = @event
            };

            var outboxMessage = new OutboxMessage
            {
                Id = envelope.EventId,
                OccurredAt = DateTimeOffset.UtcNow,
                Type = @event!.GetType().Name,
                RoutingKey = GetRoutingKeyForEvent(@event),
                Payload = JsonSerializer.Serialize(envelope),
                Headers = JsonSerializer.Serialize(envelope.Meta),
                Status = "Pending"
            };
            stopwatch.Stop();
            _logger.LogInformation("Published event {@EventLog}", new
            {
                envelope.EventId,
                Type = @event!.GetType().Name,
                CorrelationId = correlationId,
                Retry = 0,
                LatencyMs = stopwatch.ElapsedMilliseconds
            });

            await _context.OutboxMessages.AddAsync(outboxMessage);
        }

        private string GetRoutingKeyForEvent<TEvent>(TEvent @event)
        {
            return @event switch
            {
                AccountOpenedEvent => "account.opened",
                MoneyCreditedEvent => "money.credited",
                MoneyDebitedEvent => "money.debited",
                TransferCompletedEvent => "money.transfer.completed",
                InterestAccruedEvent => "money.interest.accrued",
                _ => throw new InvalidOperationException("Unknown event type")
            };
        }
    }
}
