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

        /// <summary>
        /// Начинает новую транзакцию базы данных.
        /// </summary>
        /// <returns>Асинхронная задача с объектом транзакции для управления транзакцией базы данных.</returns>
        public Task<IDbContextTransaction> BeginTransactionAsync();
        /// <summary>
        /// Сохраняет изменения в контексте базы данных.
        /// </summary>
        /// <returns>Асинхронная задача, представляющая операцию сохранения изменений.</returns>
        public Task SaveChangesAsync();
    }
}
