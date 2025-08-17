namespace BankAccounts.Infrastructure.Rabbit.Outbox
{
    /// <summary>
    /// Статус сообщения в Outbox.
    /// Используется для отслеживания состояния публикации сообщений в RabbitMQ.
    /// </summary>
    public enum MessageStatus
    {
        /// <summary>
        /// Сообщение создано и ожидает публикации.
        /// </summary>
        Pending,

        /// <summary>
        /// Сообщение находится в процессе публикации.
        /// </summary>
        Publishing,

        /// <summary>
        /// Сообщение успешно опубликовано.
        /// </summary>
        Published,

        /// <summary>
        /// Публикация сообщения завершилась неудачей.
        /// Может быть повторена в будущем в зависимости от retry-логики.
        /// </summary>
        Failed
    }
}