namespace BankAccounts.Infrastructure.Messaging
{
    public class RabbitMqBackgroundService : IHostedService
    {
        private readonly IRabbitMqPublisher _publisher;

        public RabbitMqBackgroundService(IRabbitMqPublisher publisher)
        {
            _publisher = publisher;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _publisher.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _publisher.Dispose();
            return Task.CompletedTask;
        }
    }

}
