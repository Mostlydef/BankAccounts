namespace BankAccounts.Infrastructure.Messaging
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }                  // = eventId
        public DateTimeOffset OccurredAt { get; set; }
        public string Type { get; set; } = default!;
        public string RoutingKey { get; set; } = default!;
        public string Payload { get; set; } = default!;   // JSON string
        public string Headers { get; set; } = "{}";       // JSON string
        public string Status { get; set; } = "Pending";
        public int Attempts { get; set; }
        public DateTimeOffset? NextAttemptAt { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
    }
}
