using JetBrains.Annotations;

namespace BankAccounts.Infrastructure.Rabbit.Consumers
{
    /// <summary>
    /// Представляет запись о потреблённом сообщении для реализации идемпотентности.
    /// </summary>
    public class InboxConsumed
    {
        /// <summary>
        /// Уникальный идентификатор сообщения, который приходит из очереди.
        /// Свойство MessageId используется через ORM (EF Core)
        /// </summary>
        [UsedImplicitly]
        public Guid MessageId { get; set; }
        /// <summary>
        /// Дата и время, когда сообщение было успешно обработано.
        /// По умолчанию устанавливается в <see cref="DateTimeOffset.UtcNow"/>.
        /// </summary>
        public DateTimeOffset ProcessedAt { get; set; } = DateTimeOffset.UtcNow;
        /// <summary>
        /// Имя обработчика, который обработал сообщение.
        /// Используется для поддержки нескольких обработчиков одной очереди.
        /// Свойство Handler используется через ORM (EF Core)
        /// </summary>
        [UsedImplicitly]
        public required string Handler { get; set; }
        /// <summary>
        /// Количество попыток обработки сообщения.
        /// Увеличивается при каждой неудачной попытке.
        /// </summary>
        public int RetryCount { get; set; }
    }
}
