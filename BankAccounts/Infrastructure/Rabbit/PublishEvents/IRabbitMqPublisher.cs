namespace BankAccounts.Infrastructure.Rabbit.PublishEvents
{
    public interface IRabbitMqPublisher
    {
        Task PublishRaw(string routingKey, string payloadJson, string? correlationId, string? causationId, string? messageId);
        public Task StartAsync(CancellationToken cancellationToken);
        public void Dispose();
    }
}
