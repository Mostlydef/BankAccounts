using BankAccounts.Features.Transactions;
using JetBrains.Annotations;

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
        /// Атрибут <see cref="UsedImplicitlyAttribute"/> указывает, что это свойство используется косвенно в ORM, даже если в коде прямых обращений нет.
        /// </summary> 
        [UsedImplicitly]
        public List<Transaction> Transactions { get; set; } = [];

        /// <summary>
        /// Системное поле xmin для версии строки в PostgreSQL.
        /// Атрибут <see cref="UsedImplicitlyAttribute"/> применяется, поскольку свойство используется косвенно в ORM.
        /// </summary>
        [UsedImplicitly]
        public uint Xmin { get; set; }

        public bool Frozen { get; set; }

    }
}
