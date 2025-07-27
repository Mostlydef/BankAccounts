namespace BankAccounts.Features.Accounts
{
    /// <summary>
    /// Типы счетов, доступные в системе банка.
    /// </summary>
    public enum AccountType
    {
        /// <summary>
        /// Текущий счет (Checking account).
        /// </summary>
        Checking,
        /// <summary>
        /// Депозитный счет (Deposit account).
        /// </summary>
        Deposit,
        /// <summary>
        /// Кредитный счет (Credit account).
        /// </summary>
        Credit
    }
}
