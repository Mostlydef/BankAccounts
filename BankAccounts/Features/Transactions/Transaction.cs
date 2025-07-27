using BankAccounts.Features.Accounts;

namespace BankAccounts.Features.Transactions
{
    /// <summary>
    /// Представляет банковскую транзакцию (списание, пополнение, перевод).
    /// </summary>
    public class Transaction
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
        /// Идентификатор контрагента (если это перевод). Может быть null.
        /// </summary>
        public Guid? CounterpartyAccountId { get; init; }
        /// <summary>
        /// Сумма транзакции.
        /// </summary>
        public decimal Amount { get; init; }
        /// <summary>
        /// Валюта транзакции (например, "RUB", "USD").
        /// </summary>
        public required string Currency { get; init; }
        /// <summary>
        /// Тип транзакции: Debit или Credit.
        /// </summary>
        public TransactionType Type { get; set; }
        /// <summary>
        /// Описание транзакции (например, "Пополнение через кассу").
        /// </summary>
        public required string Description { get; init; }
        /// <summary>
        /// Временная метка, когда транзакция была совершена.
        /// </summary>
        public DateTime Timestamp { get; init; }

        /// <summary>
        /// Счёт, к которому относится транзакция.
        /// </summary>
        public required Account Account { get; set; }
    }
}
