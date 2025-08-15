namespace BankAccounts.Infrastructure.Messaging
{
    public class InboxConsumed
    {
        public Guid MessageId { get; set; }
        public DateTimeOffset ProcessedAt { get; set; } = DateTimeOffset.UtcNow;
        public string Handler { get; set; } = default!;
    }
}
