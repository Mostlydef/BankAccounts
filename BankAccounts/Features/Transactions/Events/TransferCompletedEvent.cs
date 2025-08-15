namespace BankAccounts.Features.Transactions.Events
{
    public class TransferCompletedEvent
    {
        public Guid EventId { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
        public Guid SourceAccountId { get; set; }
        public Guid DestinationAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = default!;
        public Guid TransferId { get; set; }
    }
}
