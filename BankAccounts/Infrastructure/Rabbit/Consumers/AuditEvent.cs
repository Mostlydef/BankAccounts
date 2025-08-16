namespace BankAccounts.Infrastructure.Rabbit.Consumers
{
    public class AuditEvent
    {
        public Guid Id { get; set; }
        public Guid MessageId { get; set; }
        public string Handler { get; set; } = null!;
        public string Payload { get; set; } = null!;
        public DateTimeOffset ReceivedAt { get; set; }
    }
}
