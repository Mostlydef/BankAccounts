using JetBrains.Annotations;

namespace BankAccounts.Infrastructure.Rabbit.Outbox
{
    /// <summary>
    /// Представляет сообщение в Outbox, предназначенное для публикации в RabbitMQ.
    /// </summary>
    public class OutboxMessage
    {
        /// <summary>
        /// Уникальный идентификатор события (eventId).
        /// Используется для корреляции сообщений и логирования.
        /// </summary>
        [UsedImplicitly]
        public Guid Id { get; set; }

        /// <summary>
        /// Время возникновения события.
        /// Используется для упорядочивания сообщений при публикации.
        /// </summary>
        [UsedImplicitly]
        public DateTimeOffset OccurredAt { get; set; }

        /// <summary>
        /// Тип события.
        /// Используется для логирования и маршрутизации.
        /// </summary>
        [UsedImplicitly]
        public required string Type { get; set; }

        /// <summary>
        /// Ключ маршрутизации для RabbitMQ.
        /// Определяет очередь или exchange, в которую будет отправлено сообщение.
        /// </summary>
        [UsedImplicitly]
        public required string RoutingKey { get; set; }

        /// <summary>
        /// Содержимое сообщения в формате JSON.
        /// Содержит данные события, которые будут обработаны подписчиками.
        /// </summary>
        [UsedImplicitly]
        public required string Payload { get; set; }

        /// <summary>
        /// Заголовки сообщения в формате JSON.
        /// Обычно содержат метаданные: CorrelationId, CausationId, EventType и другие.
        /// </summary>
        [UsedImplicitly]
        public required string Headers { get; set; } = "{}";

        /// <summary>
        /// Статус публикации сообщения.
        /// Возможные значения: "Pending", "Publishing", "Published", "Failed".
        /// </summary>
        [UsedImplicitly]
        public string Status { get; set; } = nameof(MessageStatus.Pending);

        /// <summary>
        /// Количество попыток публикации сообщения.
        /// Используется для расчёта задержки повторной попытки (retry).
        /// </summary>
        [UsedImplicitly]
        public int Attempts { get; set; }

        /// <summary>
        /// Время, когда сообщение можно будет повторно попытаться отправить.
        /// Используется для реализации экспоненциального бэкоффа.
        /// </summary>
        public DateTimeOffset? NextAttemptAt { get; set; }

        /// <summary>
        /// Время успешной публикации сообщения.
        /// Используется для аналитики и логирования.
        /// </summary>
        public DateTimeOffset? PublishedAt { get; set; }
    }
}
