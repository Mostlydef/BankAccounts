using JetBrains.Annotations;

namespace BankAccounts.Features.Accounts.Events
{
    /// <summary>
    /// Событие, которое сигнализирует о разблокировке клиента.
    /// </summary>
    public class ClientUnblockedEvent
    {
        /// <summary>
        /// Уникальный идентификатор события.
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public Guid EventId { get; set; }

        /// <summary>
        /// Дата и время, когда произошло событие.
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public DateTimeOffset OccuredAt { get; set; }

        /// <summary>
        /// Уникальный идентификатор клиента, который был разблокирован.
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public Guid ClientId { get; set; }
    }
}