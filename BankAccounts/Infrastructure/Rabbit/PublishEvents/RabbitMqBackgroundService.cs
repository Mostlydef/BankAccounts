namespace BankAccounts.Infrastructure.Rabbit.PublishEvents
{
    /// <summary>
    /// Фоновая служба, которая запускает RabbitMQ-публикатор при старте приложения.
    /// </summary>
    public class RabbitMqBackgroundService : IHostedService
    {
        private readonly IRabbitMqPublisher _publisher;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="RabbitMqBackgroundService"/>.
        /// </summary>
        /// <param name="publisher">Интерфейс для публикации сообщений в RabbitMQ.</param>
        public RabbitMqBackgroundService(IRabbitMqPublisher publisher)
        {
            _publisher = publisher;
        }

        /// <summary>
        /// Запускает фоновую службу и инициализирует подключение к RabbitMQ.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Асинхронная задача запуска службы.</returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _publisher.StartAsync(cancellationToken);
        }

        /// <summary>
        /// Останавливает фоновую службу и освобождает ресурсы.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Завершённая задача.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _publisher.Dispose();
            return Task.CompletedTask;
        }
    }

}
