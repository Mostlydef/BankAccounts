using JetBrains.Annotations;

namespace BankAccounts.Infrastructure.Rabbit.Consumers
{
    /// <summary>
    /// Представляет запись аудита события, полученного из RabbitMQ.
    /// Используется для хранения содержимого сообщения и метаданных обработчика.
    /// </summary>
    public class AuditEvent
    {
        /// <summary>
        /// Уникальный идентификатор записи в таблице AuditEvents.
        /// </summary>
        [UsedImplicitly]
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор сообщения из RabbitMQ (MessageId).
        /// Позволяет реализовать идемпотентность обработки.
        /// </summary>
        [UsedImplicitly]
        public Guid MessageId { get; set; }

        /// <summary>
        /// Имя обработчика (consumer), который принял и обработал сообщение.
        /// Свойство Handler используется через ORM (EF Core)
        /// </summary>
        [UsedImplicitly]
        public required string Handler { get; set; }

        /// <summary>
        /// Содержимое сообщения (payload) в формате строки.
        /// Свойство Payload используется через ORM (EF Core)
        /// </summary>
        [UsedImplicitly]
        public required string Payload { get; set; }

        /// <summary>
        /// Дата и время получения сообщения.
        /// Свойство ReceivedAt используется через ORM (EF Core)
        /// </summary>
        [UsedImplicitly]
        public DateTimeOffset ReceivedAt { get; set; }
    }
}