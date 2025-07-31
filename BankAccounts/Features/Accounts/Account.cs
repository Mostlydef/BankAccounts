using BankAccounts.Features.Transactions;

namespace BankAccounts.Features.Accounts
{
    /// <summary>
    /// Модель банковского счета.
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Уникальный идентификатор счета.
        /// </summary>
        public Guid Id { get; init; }
        /// <summary>
        /// Идентификатор владельца счета.
        /// </summary>
        public Guid OwnerId { get; init; }
        /// <summary>
        /// Тип счета (Checking, Deposit, Credit).
        /// </summary>
        public AccountType Type { get; init; }
        /// <summary>
        /// Валюта счета в формате трехбуквенного кода ISO.
        /// </summary>
        public required string Currency { get; init; }
        /// <summary>
        /// Текущий баланс счета.
        /// </summary>
        public decimal Balance { get; set; }
        /// <summary>
        /// Процентная ставка по счету, если применимо.
        /// </summary>
        public decimal? InterestRate { get; set; }
        /// <summary>
        /// Дата открытия счета.
        /// </summary>
        public DateTime OpenDate { get; init; }
        /// <summary>
        /// Дата закрытия счета. Может быть null, если счет открыт.
        /// </summary>
        public DateTime? CloseDate { get; set; }

        /// <summary>
        /// Список транзакций, связанных с этим счетом.
        /// </summary>
        public List<Transaction> Transactions { get; init; } = [];
    }
}
