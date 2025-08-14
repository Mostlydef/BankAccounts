using JetBrains.Annotations;

namespace BankAccounts.Features.Transactions.DTOs
{
    /// <summary>
    /// DTO для создания новой транзакции.
    /// </summary>
    public class TransactionCreateDto
    {
        /// <summary>
        /// Идентификатор счёта, на котором создается транзакция.
        /// Атрибут <see cref="UsedImplicitlyAttribute"/> указывает что это свойство используется косвенно через сериализацию в тестах.
        /// </summary>
        [UsedImplicitly]
        public Guid AccountId { get; set; }
        /// <summary>
        /// Идентификатор контрагента (если применимо).
        /// Атрибут <see cref="UsedImplicitlyAttribute"/> указывает что это свойство используется косвенно через сериализацию в тестах.
        /// </summary>
        [UsedImplicitly]
        public Guid? CounterpartyAccountId { get; set; }
        /// <summary>
        /// Сумма транзакции.
        /// Атрибут <see cref="UsedImplicitlyAttribute"/> указывает что это свойство используется косвенно через сериализацию в тестах.
        /// </summary>
        [UsedImplicitly]
        public decimal Amount { get; set; }
        /// <summary>
        /// Валюта транзакции, например "USD", "EUR".
        /// Атрибут <see cref="UsedImplicitlyAttribute"/> указывает что это свойство используется косвенно через сериализацию в тестах.
        /// </summary>
        [UsedImplicitly]
        public required string Currency { get; set; }
        /// <summary>
        /// Тип транзакции. Значения: "Credit" или "Debit".
        /// </summary>
        public required string Type { get; init; } = string.Empty;
        /// <summary>
        /// Описание транзакции.
        /// </summary>
        public required string Description { get; init; } = string.Empty;
    }
}
