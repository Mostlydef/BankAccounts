using System.Data;
using BankAccounts.Database.Interfaces;
using BankAccounts.Features.Transactions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

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
        /// Получает транзакцию по её идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор транзакции.</param>
        /// <returns>Transaction, если найдена, иначе null.</returns>
        /// Аннотация <see cref="UsedImplicitlyAttribute"/> подавляет предупреждения о необходимости использовании метода.
        [UsedImplicitly]
        public async Task<Transaction?> GetById(Guid id)
        {
            var transaction = await _context.Transactions.FirstOrDefaultAsync(x => x.Id == id);
            return transaction;
        }

        public async Task<IDbContextTransaction> BeginTransationAsync()
        {
            return await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
