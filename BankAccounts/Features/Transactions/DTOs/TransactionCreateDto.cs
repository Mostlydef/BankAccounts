namespace BankAccounts.Features.Transactions.DTOs
{
    /// <summary>
    /// DTO для создания новой транзакции.
    /// </summary>
    public class TransactionCreateDto
    {
        /// <summary>
        /// Идентификатор счёта, на котором создается транзакция.
        /// </summary>
        public Guid AccountId { get; set; }
        /// <summary>
        /// Идентификатор контрагента (если применимо).
        /// </summary>
        public Guid? CounterpartyAccountId { get; set; }
        /// <summary>
        /// Сумма транзакции.
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// Валюта транзакции, например "USD", "EUR".
        /// </summary>
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
