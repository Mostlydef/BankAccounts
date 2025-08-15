namespace BankAccounts.Features.Accounts.Events
{
    public class InterestAccruedEvent
    {
        public Guid EventId { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
        public Guid AccountId { get; set; }
        public DateTimeOffset PeriodFrom { get; set; }
        public DateTimeOffset PeriodTo { get; set; }
        public decimal Amount { get; set; }
    }
}
