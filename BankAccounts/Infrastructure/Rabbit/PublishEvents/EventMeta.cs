using JetBrains.Annotations;

namespace BankAccounts.Infrastructure.Rabbit.PublishEvents
{
    /// <summary>
    /// Метаданные события, передаваемого через RabbitMQ.
    /// Используются для маршрутизации, трассировки и обеспечения совместимости.
    /// </summary>
    public class EventMeta
    {
        /// <summary>
        /// Версия контракта события.
        /// Обычно используется для обратной совместимости при изменении структуры.
        /// Значение по умолчанию: "v1".
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public string Version { get; set; } = "v1";

        /// <summary>
        /// Источник события — сервис или компонент, который его сгенерировал.
        /// Например: "BankAccountsService".
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public required string Source { get; set; }

        /// <summary>
        /// Корреляционный идентификатор.
        /// Используется для объединения связанных событий в рамках одного бизнес-процесса.
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// Идентификатор события-инициатора (causation).
        /// Помогает отследить, какое событие породило текущее.
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public Guid CausationId { get; set; }
    }
}