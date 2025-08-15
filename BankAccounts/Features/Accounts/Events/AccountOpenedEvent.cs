namespace BankAccounts.Features.Accounts.Events
{
    public class AccountOpenedEvent
    {
        public Guid AccountId { get; set; }
        public Guid OwnerId { get; set; }
        public string Currency { get; set; }
        public string Type { get; set; }
    }
}
