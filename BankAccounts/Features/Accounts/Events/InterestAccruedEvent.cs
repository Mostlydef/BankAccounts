using JetBrains.Annotations;

namespace BankAccounts.Features.Accounts.Events
{
    /// <summary>
    /// Событие начисления процентов на счёт.
    /// </summary>
    public class InterestAccruedEvent
    {
        /// <summary>
        /// Уникальный идентификатор события.
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public Guid EventId { get; set; }

        /// <summary>
        /// Время возникновения события.
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public DateTimeOffset OccurredAt { get; set; }

        /// <summary>
        /// Идентификатор счёта, на который начислены проценты.
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public Guid AccountId { get; set; }

        /// <summary>
        /// Начало периода, за который начисляются проценты.
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public DateTimeOffset PeriodFrom { get; set; }

        /// <summary>
        /// Конец периода, за который начисляются проценты.
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public DateTimeOffset PeriodTo { get; set; }

        /// <summary>
        /// Сумма начисленных процентов.
        /// Свойство косвенно используется при сериализации\десериализации. 
        /// </summary>
        [UsedImplicitly]
        public decimal Amount { get; set; }
    }
}