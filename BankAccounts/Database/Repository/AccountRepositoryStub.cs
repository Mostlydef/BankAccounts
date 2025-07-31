using BankAccounts.Database.Interfaces;
using BankAccounts.Features.Accounts;
using BankAccounts.Features.Transactions;

namespace BankAccounts.Database.Repository
{
    /// <summary>
    /// Заглушка (stub) репозитория аккаунтов для тестирования и разработки.
    /// Хранит данные в памяти в списке.
    /// </summary>
    public class AccountRepositoryStub : IAccountRepository
    {
        private readonly List<Account> _accounts;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="AccountRepositoryStub"/>.
        /// </summary>
        public AccountRepositoryStub()
        {
            _accounts = [];
        }

        /// <summary>
        /// Добавляет новый аккаунт в репозиторий.
        /// </summary>
        /// <param name="account">Аккаунт для добавления.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public Task AddAsync(Account account)
        {
            _accounts.Add(account);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Устанавливает дату закрытия аккаунта, имитируя удаление.
        /// </summary>
        /// <param name="accountId">Идентификатор аккаунта.</param>
        /// <returns>True, если аккаунт найден и удален, иначе false.</returns>
        public Task<bool> Delete(Guid accountId)
        {
            var account = _accounts.FirstOrDefault(x => x.Id == accountId);
            if (account != null)
            {
                account.CloseDate = DateTime.Now;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        /// <summary>
        /// Получает аккаунт по идентификатору.
        /// </summary>
        /// <param name="accountId">Идентификатор аккаунта.</param>
        /// <param name="cancellation">Токен отмены операции.</param>
        /// <returns>Account, если найден, иначе null.</returns>
        public Task<Account?> GetByIdAsync(Guid accountId, CancellationToken cancellation)
        {
            var account = _accounts.FirstOrDefault(x => x.Id == accountId);
            return Task.FromResult(account);
        }

        /// <summary>
        /// Получает список аккаунтов по идентификатору владельца.
        /// </summary>
        /// <param name="ownerId">Идентификатор владельца.</param>
        /// <returns>Список аккаунтов данного владельца.</returns>
        public Task<List<Account>> GetByOwnerIdAsync(Guid ownerId)
        {
            var accounts = _accounts.Where(x => x.OwnerId == ownerId).ToList();
            return Task.FromResult(accounts);
        }

        /// <summary>
        /// Обновляет информацию об аккаунте.
        /// </summary>
        /// <param name="account">Аккаунт с обновленными данными.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public Task UpdateAsync(Account account)
        {
            var index = _accounts.FindIndex(x => x.Id == account.Id);
            if (index >= 0)
            {
                _accounts.RemoveAt(index);
                _accounts.Insert(index, account);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Получает список транзакций аккаунта за указанный период.
        /// </summary>
        /// <param name="id">Идентификатор аккаунта.</param>
        /// <param name="from">Дата начала периода.</param>
        /// <param name="to">Дата окончания периода.</param>
        /// <returns>Список транзакций.</returns>
        public Task<List<Transaction>> GetTransactions(Guid id, DateTime from, DateTime to)
        {
            var account = _accounts.FirstOrDefault(x => x.Id == id);

            if (account == null)
                return Task.FromResult(new List<Transaction>());

            var transactions = account.Transactions
                .Where(t => t.Timestamp >= from && t.Timestamp <= to)
                .ToList();

            return Task.FromResult(transactions);
        }
    }
}
