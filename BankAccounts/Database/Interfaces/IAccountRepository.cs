using BankAccounts.Features.Accounts;
using BankAccounts.Features.Transactions;

namespace BankAccounts.Database.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с банковскими счетами.
    /// </summary>
    public interface IAccountRepository
    {
        /// <summary>
        /// Добавляет новый банковский счет.
        /// </summary>
        /// <param name="account">Объект счета для добавления.</param>
        /// <returns>Асинхронная задача.</returns>
        public Task AddAsync(Account account);
        /// <summary>
        /// Обновляет информацию о существующем банковском счете.
        /// </summary>
        /// <param name="account">Обновлённый объект счета.</param>
        /// <returns>Асинхронная задача.</returns>
        public Task UpdateAsync(Account account);
        /// <summary>
        /// Удаляет счет по идентификатору (например, закрывает счет).
        /// </summary>
        /// <param name="accountId">Идентификатор счета для удаления.</param>
        /// <returns>Возвращает <c>true</c>, если удаление прошло успешно, иначе <c>false</c>.</returns>
        public Task<bool> Delete(Guid accountId);
        /// <summary>
        /// Получает счет по уникальному идентификатору.
        /// </summary>
        /// <param name="accountId">Идентификатор счета.</param>
        /// <param name="cancellation">Токен отмены операции.</param>
        /// <returns>Объект счета или <c>null</c>, если счет не найден.</returns>
        public Task<Account?> GetByIdAsync(Guid accountId, CancellationToken cancellation);
        /// <summary>
        /// Получает список счетов, принадлежащих определённому владельцу.
        /// </summary>
        /// <param name="ownerId">Идентификатор владельца счетов.</param>
        /// <returns>Список счетов владельца.</returns>
        public Task<List<Account>> GetByOwnerIdAsync(Guid ownerId);
        /// <summary>
        /// Получает список транзакций для счета за указанный период.
        /// </summary>
        /// <param name="id">Идентификатор счета.</param>
        /// <param name="from">Дата начала периода.</param>
        /// <param name="to">Дата окончания периода.</param>
        /// <returns>Список транзакций в указанном периоде.</returns>
        public Task<List<Transaction>> GetTransactions(Guid id, DateTime from, DateTime to);
    }
}
