namespace BankAccounts.Infrastructure.Messaging
{
    public class EventMeta
    {
        public string Version { get; set; } = "v1";
        public string Source { get; set; }
        public Guid CorrelationId { get; set; }
        public Guid CausationId { get; set; }
    }
}
