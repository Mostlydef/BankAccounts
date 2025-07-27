namespace BankAccounts.Features.Transactions
{
    /// <summary>
    /// Определяет тип транзакции.
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// Кредитовая транзакция — поступление средств на счёт.
        /// </summary>
        Credit,
        /// <summary>
        /// Дебетовая транзакция — списание средств со счёта.
        /// </summary>
        Debit
    }
}
