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
        /// </summary>
        public Guid EventId { get; set; }

        /// <summary>
        /// Дата и время возникновения события.
        /// </summary>
        public DateTimeOffset OccurredAt { get; set; }

        /// <summary>
        /// Идентификатор счета-отправителя перевода.
        /// </summary>
        public Guid SourceAccountId { get; set; }

        /// <summary>
        /// Идентификатор счета-получателя перевода.
        /// </summary>
        public Guid DestinationAccountId { get; set; }

        /// <summary>
        /// Сумма перевода.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Валюта перевода.
        /// </summary>
        public required string Currency { get; set; }

        /// <summary>
        /// Уникальный идентификатор перевода.
        /// </summary>
        public Guid TransferId { get; set; }
    }
}