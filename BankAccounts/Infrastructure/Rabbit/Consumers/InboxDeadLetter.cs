using JetBrains.Annotations;

namespace BankAccounts.Infrastructure.Rabbit.Consumers
{
    /// <summary>
    /// Представляет сообщение, которое не удалось обработать и
    /// было перенесено в "мертвую" очередь (dead-letter queue).
    /// </summary>
    public class InboxDeadLetter
    {
        /// <summary>
        /// Уникальный идентификатор сообщения.
        /// Совпадает с <c>MessageId</c> из оригинальной очереди.
        /// </summary>
        [UsedImplicitly]
        public Guid MessageId { get; set; }

        /// <summary>
        /// Время получения сообщения.
        /// Используется для анализа и отладки.
        /// Свойство ReceivedAt используется через ORM (EF Core)
        /// </summary>
        [UsedImplicitly]
        public DateTimeOffset ReceivedAt { get; set; }

        /// <summary>
        /// Имя обработчика, который пытался обработать сообщение.
        /// Свойство Handler используется через ORM (EF Core)
        /// </summary>
        [UsedImplicitly]
        public required string Handler { get; set; }

        /// <summary>
        /// Содержимое сообщения (JSON-представление).
        /// Свойство Payload используется через ORM (EF Core)
        /// </summary>
        [UsedImplicitly]
        public required string Payload { get; set; }

        /// <summary>
        /// Описание ошибки, возникшей при обработке.
        /// Обычно содержит stack trace или текст исключения.
        /// Свойство Error используется через ORM (EF Core)
        /// </summary>
        [UsedImplicitly]
        public required string Error { get; set; }
    }
}