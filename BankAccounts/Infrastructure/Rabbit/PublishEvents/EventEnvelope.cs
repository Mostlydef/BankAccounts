namespace BankAccounts.Infrastructure.Rabbit.PublishEvents
{
    public class EventEnvelope<T>
    {
        public Guid EventId { get; set; }
        public string OccurredAt { get; set; } // ISO-8601 UTC
        public EventMeta Meta { get; set; } = default!;
        public T Payload { get; set; } = default!;
    }
}
