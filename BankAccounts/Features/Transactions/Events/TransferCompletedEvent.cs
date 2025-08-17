using JetBrains.Annotations;

namespace BankAccounts.Features.Transactions.Events
{
    /// <summary>
    /// Событие, представляющее успешное завершение перевода между счетами.
    /// </summary>
    public class TransferCompletedEvent
    {
        /// <summary>
        /// Уникальный идентификатор события.
        /// Свойство неявно используется для сериализации/десериализации.
        /// </summary>
        [UsedImplicitly]
        public Guid EventId { get; set; }

        /// <summary>
        /// Дата и время возникновения события.
        /// Свойство неявно используется для сериализации/десериализации.
        /// </summary>
        [UsedImplicitly]
        public DateTimeOffset OccurredAt { get; set; }

        /// <summary>
        /// Идентификатор счета-отправителя перевода.
        /// Свойство неявно используется для сериализации/десериализации.
        /// </summary>
        [UsedImplicitly]
        public Guid SourceAccountId { get; set; }

        /// <summary>
        /// Идентификатор счета-получателя перевода.
        /// Свойство неявно используется для сериализации/десериализации.
        /// </summary>
        [UsedImplicitly]
        public Guid DestinationAccountId { get; set; }

        /// <summary>
        /// Сумма перевода.
        /// Свойство неявно используется для сериализации/десериализации.
        /// </summary>
        [UsedImplicitly]
        public decimal Amount { get; set; }

        /// <summary>
        /// Валюта перевода.
        /// Свойство неявно используется для сериализации/десериализации.
        /// </summary>
        [UsedImplicitly]
        public required string Currency { get; set; }

        /// <summary>
        /// Уникальный идентификатор перевода.
        /// Свойство неявно используется для сериализации/десериализации.
        /// </summary>
        [UsedImplicitly]
        public Guid TransferId { get; set; }
    }
}