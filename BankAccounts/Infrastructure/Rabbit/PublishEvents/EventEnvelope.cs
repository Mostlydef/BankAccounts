using JetBrains.Annotations;

namespace BankAccounts.Infrastructure.Rabbit.PublishEvents
{
    /// <summary>
    /// Обёртка для событий, публикуемых в RabbitMQ.
    /// Содержит метаданные и полезную нагрузку (payload).
    /// </summary>
    /// <typeparam name="T">Тип полезной нагрузки события.</typeparam>
    public class EventEnvelope<T>
    {
        /// <summary>
        /// Уникальный идентификатор события.
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public Guid EventId { get; set; }

        /// <summary>
        /// Время возникновения события в формате ISO-8601 (UTC).
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public required string OccurredAt { get; set; }

        /// <summary>
        /// Метаданные события (тип, версия, correlationId и т.п.).
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public required EventMeta Meta { get; set; }

        /// <summary>
        /// Полезная нагрузка события.
        /// </summary>
        [UsedImplicitly]
        public required T Payload { get; set; }
    }
}