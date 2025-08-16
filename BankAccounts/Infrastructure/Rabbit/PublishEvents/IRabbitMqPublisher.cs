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
        /// <param name="payloadJson">Содержимое сообщения в формате JSON.</param>
        /// <param name="correlationId">Идентификатор корреляции для отслеживания цепочки событий.</param>
        /// <param name="causationId">Идентификатор причинного события.</param>
        /// <param name="messageId">Уникальный идентификатор сообщения, используется для idempotency или Inbox.</param>
        /// <returns>Задача <see cref="Task"/>, представляющая асинхронную операцию публикации.</returns>
        public Task PublishRaw(string routingKey, string payloadJson, string? correlationId, string? causationId, string? messageId);

        /// <summary>
        /// Асинхронно запускает и инициализирует соединение с RabbitMQ.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены для завершения операции.</param>
        /// <returns>Задача <see cref="Task"/> для отслеживания завершения инициализации.</returns>
        public Task StartAsync(CancellationToken cancellationToken);
    }
}