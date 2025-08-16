using JetBrains.Annotations;

namespace BankAccounts.Features.Transactions.Events
{
    /// <summary>
    /// Событие, представляющее зачисление денежных средств на счет.
    /// </summary>
    public class MoneyCreditedEvent
    {
        /// <summary>
        /// Уникальный идентификатор события.
        /// Свойство косвенно используется в EF.
        /// </summary>
        [UsedImplicitly]
        public Guid EventId { get; set; }

        /// <summary>
        /// Дата и время возникновения события.
        /// Свойство косвенно используется в EF.
        /// </summary>
        [UsedImplicitly]
        public DateTimeOffset OccurredAt { get; set; }

        /// <summary>
        /// Идентификатор счета, на который зачислены средства.
        /// Свойство косвенно используется в EF.
        /// </summary>
        [UsedImplicitly]
        public Guid AccountId { get; set; }

        /// <summary>
        /// Сумма зачисления.
        /// Свойство косвенно используется в EF.
        /// </summary>
        [UsedImplicitly]
        public decimal Amount { get; set; }

        /// <summary>
        /// Валюта зачисления.
        /// Свойство косвенно используется в EF.
        /// </summary>
        [UsedImplicitly]
        public required string Currency { get; set; }

        /// <summary>
        /// Идентификатор операции, связанной с зачислением.
        /// Свойство косвенно используется в EF.
        /// </summary>
        [UsedImplicitly]
        public Guid OperationId { get; set; }
    }
}