namespace BankAccounts.Features.Accounts.Events
{
    public class ClientUnblockedEvent
    {
        public Guid EventId { get; set; }
        public DateTimeOffset OccuredAt { get; set; }
        public Guid ClientId { get; set; }
    }
}
