namespace BankAccounts.Infrastructure.Rabbit.Consumers
{
    public class InboxDeadLetter
    {
        public Guid MessageId { get; set; }           
        public DateTimeOffset ReceivedAt { get; set; } 
        public string Handler { get; set; } 
        public string Payload { get; set; } 
        public string Error { get; set; } 
    }
}
