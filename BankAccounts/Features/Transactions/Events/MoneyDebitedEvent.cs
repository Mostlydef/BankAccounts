namespace BankAccounts.Features.Transactions.Events
{
    public class MoneyDebitedEvent
    {
        public Guid EventId { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = default!;
        public Guid OperationId { get; set; }
        public string Reason { get; set; } = default!;
    }
}
