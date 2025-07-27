namespace BankAccounts.Features.Transactions.DTOs
{
    /// <summary>
    /// DTO для передачи данных о транзакции.
    /// </summary>
    public class TransactionDto
    {
        /// <summary>
        /// Уникальный идентификатор транзакции.
        /// </summary>
        public Guid Id { get; init; }
        /// <summary>
        /// Идентификатор счёта, к которому относится транзакция.
        /// </summary>
        public Guid AccountId { get; init; }
        /// <summary>
        /// Идентификатор контрагента (если применимо).
        /// </summary>
        public Guid? CounterpartyAccountId { get; init; }
        /// <summary>
        /// Сумма транзакции.
        /// </summary>
        public decimal Amount { get; init; }
        /// <summary>
        /// Валюта транзакции, например, "USD", "RUB".
        /// </summary>
        public required string Currency { get; init; }
        /// <summary>
        /// Тип транзакции — "Credit" или "Debit".
        /// </summary>
        public required string Type { get; init; }
        /// <summary>
        /// Описание транзакции.
        /// </summary>
        public required string Description { get; init; }
        /// <summary>
        /// Время создания транзакции.
        /// </summary>
        public DateTime Timestamp { get; init; }
    }
}
