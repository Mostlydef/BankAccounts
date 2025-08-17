using JetBrains.Annotations;

namespace BankAccounts.Features.Accounts.Events
{
    /// <summary>
    /// Событие, которое возникает при открытии нового банковского счета.
    /// </summary>
    public class AccountOpenedEvent
    {
        /// <summary>
        /// Уникальный идентификатор открытого счета.
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public Guid AccountId { get; set; }
        /// <summary>
        /// Уникальный идентификатор владельца счета
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public Guid OwnerId { get; set; }
        /// <summary>
        /// Валюта счета в формате ISO 4217 (например, "USD", "EUR").
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public required string Currency { get; set; }
        /// <summary>
        /// Тип счета (например, "Checking", "Savings").
        /// Свойство косвенно используется при сериализации\десериализации.
        /// </summary>
        [UsedImplicitly]
        public required string Type { get; set; }
    }
}
