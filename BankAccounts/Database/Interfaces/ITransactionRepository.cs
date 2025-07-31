using BankAccounts.Features.Transactions;

namespace BankAccounts.Database.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с транзакциями.
    /// </summary>
    public interface ITransactionRepository
    {
        /// <summary>
        /// Регистрирует (добавляет) новую транзакцию.
        /// </summary>
        /// <param name="transaction">Транзакция для регистрации.</param>
        /// <returns>Асинхронная задача.</returns>
        public Task RegisterAsync(Transaction transaction);
    }
}
