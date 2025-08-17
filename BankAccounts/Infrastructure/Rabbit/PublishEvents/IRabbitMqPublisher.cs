using BankAccounts.Infrastructure.Rabbit.Outbox;

namespace BankAccounts.Infrastructure.Rabbit.PublishEvents
{
    /// <summary>
    /// Определяет контракт для низкоуровневой публикации сообщений в RabbitMQ.
    /// </summary>
    public interface IRabbitMqPublisher : IDisposable
    {
        /// <summary>
        /// Публикует "сырое" сообщение в RabbitMQ.
        /// </summary>
        /// <param name="routingKey">Ключ маршрутизации для RabbitMQ (очередь или exchange).</param>
        /// <param name="message"></param>
        public Task PublishRaw(string routingKey, OutboxMessage message);

        /// <summary>
        /// Асинхронно запускает и инициализирует соединение с RabbitMQ.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены для завершения операции.</param>
        /// <returns>Задача <see cref="Task"/> для отслеживания завершения инициализации.</returns>
        public Task StartAsync(CancellationToken cancellationToken);
    }
}