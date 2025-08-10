using BankAccounts.Database.Interfaces;
using BankAccounts.Features.Accounts;
using BankAccounts.Features.Transactions;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Database.Repository
{
    /// <summary>
    /// Заглушка (stub) репозитория аккаунтов для тестирования и разработки.
    /// Хранит данные в памяти в списке.
    /// </summary>
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="AccountRepository"/>.
        /// </summary>
        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Добавляет новый аккаунт в репозиторий.
        /// </summary>
        /// <param name="account">Аккаунт для добавления.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public async Task AddAsync(Account account)
        {
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Устанавливает дату закрытия аккаунта, имитируя удаление.
        /// </summary>
        /// <param name="accountId">Идентификатор аккаунта.</param>
        /// <returns>True, если аккаунт найден и удален, иначе false.</returns>
        public async Task<bool> Delete(Guid accountId)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == accountId);
            if (account != null)
            {
                account.CloseDate = DateTime.Now;
                return true;
            }

            await _context.SaveChangesAsync();
            return false;
        }

        /// <summary>
        /// Получает аккаунт по идентификатору.
        /// </summary>
        /// <param name="accountId">Идентификатор аккаунта.</param>
        /// <param name="cancellation">Токен отмены операции.</param>
        /// <returns>Account, если найден, иначе null.</returns>
        public async Task<Account?> GetByIdAsync(Guid accountId, CancellationToken cancellation)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == accountId, cancellationToken: cancellation);
            return account;
        }

        /// <summary>
        /// Получает список аккаунтов по идентификатору владельца.
        /// </summary>
        /// <param name="ownerId">Идентификатор владельца.</param>
        /// <returns>Список аккаунтов данного владельца.</returns>
        public async Task<List<Account>> GetByOwnerIdAsync(Guid ownerId)
        {
            var accounts = await _context.Accounts.Where(x => x.OwnerId == ownerId).ToListAsync();
            return accounts;
        }

        /// <summary>
        /// Обновляет информацию об аккаунте.
        /// </summary>
        /// <param name="account">Аккаунт с обновленными данными.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public Task UpdateAsync(Account account)
        {
            _context.Accounts.Update(account);
            _context.SaveChangesAsync();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Получает список транзакций аккаунта за указанный период.
        /// </summary>
        /// <param name="id">Идентификатор аккаунта.</param>
        /// <param name="from">Дата начала периода.</param>
        /// <param name="to">Дата окончания периода.</param>
        /// <returns>Список транзакций.</returns>
        public async Task<List<Transaction>> GetTransactions(Guid id, DateTime from, DateTime to)
        {
            return await _context.Transactions
                .Where(t => t.AccountId == id && t.Timestamp >= from.ToUniversalTime() && t.Timestamp <= to.ToUniversalTime())
                .ToListAsync();
        }
    }
}
