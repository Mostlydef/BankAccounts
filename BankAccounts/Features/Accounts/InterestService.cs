using BankAccounts.Common.Results;
using BankAccounts.Database;
using BankAccounts.Database.Interfaces;
using BankAccounts.Features.Accounts.Events;
using BankAccounts.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Features.Accounts
{
    /// <summary>
    /// Сервис для начисления процентов на все счета с заданной процентной ставкой.
    /// </summary>
    public class InterestService
    {
        private readonly AppDbContext _context;
        private readonly IPublishEvent _publishEvent;
        private readonly ITransactionRepository _transactionRepository;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="InterestService"/>.
        /// </summary>
        /// <param name="context">Контекст базы данных приложения.</param>
        public InterestService(AppDbContext context, IPublishEvent publishEvent, ITransactionRepository transactionRepository)
        {
            _context = context;
            _publishEvent = publishEvent;
            _transactionRepository = transactionRepository;
        }

        /// <summary>
        /// Асинхронно вызывает хранимую процедуру <c>accrue_interest</c> для всех счетов, у которых задан <see cref="Account.InterestRate"/>.
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public async Task<MbResult<List<Guid>>> AccrueInterestForAllAccountAsync()
        {
            var accountIds = await _context.Accounts
                .Where(a => a.InterestRate != null)
                .Select(a => a.Id)
                .ToListAsync();

            await using  var tx = await _transactionRepository.BeginTransactionAsync();
            try
            {
                foreach (var accountId in accountIds)
                {

                    var lastInterestDate = await _context.Transactions
                        .Where(t => t.AccountId == accountId && t.Description == "Interest Accrual")
                        .OrderByDescending(t => t.Timestamp)
                        .Select(t => (DateTimeOffset?)t.Timestamp)
                        .FirstOrDefaultAsync();

                    var periodFrom = lastInterestDate ?? DateTimeOffset.UtcNow.AddMonths(-1); // например, за последний месяц
                    var periodTo = DateTimeOffset.UtcNow;

                    await _context.Database.ExecuteSqlInterpolatedAsync($"CALL accrue_interest({accountId})");

                    var amount = await _context.Transactions
                        .Where(t => t.AccountId == accountId &&
                                    t.Description == "Interest Accrual" &&
                                    t.Timestamp >= periodFrom &&
                                    t.Timestamp <= periodTo)
                        .SumAsync(t => t.Amount);

                    var interestAccruedEvent = new InterestAccruedEvent
                    {
                        EventId = Guid.NewGuid(),
                        OccurredAt = DateTimeOffset.UtcNow,
                        AccountId = accountId,
                        PeriodFrom = periodFrom,
                        PeriodTo = periodTo,
                        Amount = amount
                    };

                    await _publishEvent.PublishEventAsync(interestAccruedEvent, accountId);
                }

                await tx.CommitAsync();
            }
            catch (Exception e)
            {
                await tx.RollbackAsync();
                return MbResult<List<Guid>>.BadRequest("Ошибка при начислении процентов");
            }

            return MbResult<List<Guid>>.Success(accountIds);
        }
    }
}
