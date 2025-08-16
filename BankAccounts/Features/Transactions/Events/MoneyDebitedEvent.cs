using JetBrains.Annotations;

namespace BankAccounts.Features.Transactions.Events
{
    /// <summary>
    /// Событие, представляющее списание денежных средств со счета.
    /// </summary>
    public class MoneyDebitedEvent
    {
        /// <summary>
        /// Уникальный идентификатор события.
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public Guid EventId { get; set; }

        /// <summary>
        /// Дата и время возникновения события.
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public DateTimeOffset OccurredAt { get; set; }

        /// <summary>
        /// Идентификатор счета, с которого списаны средства.
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public Guid AccountId { get; set; }

        /// <summary>
        /// Сумма списания.
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public decimal Amount { get; set; }

        /// <summary>
        /// Валюта списания.
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public required string Currency { get; set; }

        /// <summary>
        /// Идентификатор операции, связанной со списанием.
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public Guid OperationId { get; set; }
    }
}