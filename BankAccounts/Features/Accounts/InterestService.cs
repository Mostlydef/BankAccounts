using BankAccounts.Database;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Features.Accounts
{
    /// <summary>
    /// Сервис для начисления процентов на все счета с заданной процентной ставкой.
    /// </summary>
    public class InterestService
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="InterestService"/>.
        /// </summary>
        /// <param name="context">Контекст базы данных приложения.</param>
        public InterestService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Асинхронно вызывает хранимую процедуру <c>accrue_interest</c> для всех счетов, у которых задан <see cref="Account.InterestRate"/>.
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public async Task AccrueInterestForAllAccountAsync()
        {
            var accountIds = await _context.Accounts
                .Where(a => a.InterestRate != null)
                .Select(a => a.Id)
                .ToListAsync();

            foreach (var accountId in accountIds)
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($"CALL accrue_interest({accountId})");
            }
        }
    }
}
