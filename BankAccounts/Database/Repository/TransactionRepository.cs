using BankAccounts.Database.Interfaces;
using BankAccounts.Features.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace BankAccounts.Database.Repository
{
    /// <summary>
    /// Заглушка (stub) репозитория транзакций для тестирования и разработки.
    /// Хранит транзакции в памяти в списке.
    /// </summary>
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="TransactionRepository"/>.
        /// </summary>
        public TransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Регистрирует новую транзакцию.
        /// </summary>
        /// <param name="transaction">Транзакция для добавления.</param>
        /// <returns>Асинхронная задача.</returns>
        public async Task RegisterAsync(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
        }

        /// <summary>
        /// Сохраняет изменения в контексте базы данных.
        /// </summary>
        /// <returns>Асинхронная задача.</returns>
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        }

        /// <summary>
        /// Сохраняет изменения в контексте базы данных.
        /// </summary>
        /// <returns>Асинхронная задача.</returns>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
