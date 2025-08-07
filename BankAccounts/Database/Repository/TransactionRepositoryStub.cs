using BankAccounts.Database.Interfaces;
using BankAccounts.Features.Transactions;
using JetBrains.Annotations;

namespace BankAccounts.Database.Repository
{
    /// <summary>
    /// Заглушка (stub) репозитория транзакций для тестирования и разработки.
    /// Хранит транзакции в памяти в списке.
    /// </summary>
    public class TransactionRepositoryStub : ITransactionRepository
    {
        private readonly List<Transaction> _stubTransaction;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="TransactionRepositoryStub"/>.
        /// </summary>
        public TransactionRepositoryStub()
        {
            _stubTransaction = [];
        }

        /// <summary>
        /// Регистрирует новую транзакцию.
        /// </summary>
        /// <param name="transaction">Транзакция для добавления.</param>
        /// <returns>Асинхронная задача.</returns>
        public Task RegisterAsync(Transaction transaction)
        {
            _stubTransaction.Add(transaction);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Получает транзакцию по её идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор транзакции.</param>
        /// <returns>Transaction, если найдена, иначе null.</returns>
        /// Аннотация <see cref="UsedImplicitlyAttribute"/> подавляет предупреждения о необходимости использовании метода.
        [UsedImplicitly]
        public Task<Transaction?> GetById(Guid id)
        {
            var transaction = _stubTransaction.FirstOrDefault(x => x.Id == id);
            return Task.FromResult(transaction);
        }
    }
}
