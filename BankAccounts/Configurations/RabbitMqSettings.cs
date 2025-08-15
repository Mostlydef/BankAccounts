namespace BankAccounts.Configurations
{
    public class RabbitMqSettings
    {
        public string HostName { get; set; } = "localhost";
        public string UserName { get; set; } = "admin";
        public string Password { get; set; } = "admin";
        public string ExchangeName { get; set; } = "account.events";
    }
}
