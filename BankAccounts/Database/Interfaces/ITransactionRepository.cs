using BankAccounts.Features.Transactions;
using Microsoft.EntityFrameworkCore.Storage;

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
        public Task<Transaction?> GetById(Guid id);
        public Task<IDbContextTransaction> BeginTransationAsync();
        public Task SaveChangesAsync();
    }
}
