using BankAccounts.Database;
using BankAccounts.Infrastructure.Rabbit.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BankAccounts.Infrastructure.Rabbit
{
    /// <summary>
    /// HealthCheck для проверки состояния Outbox-паттерна.
    /// Отслеживает количество сообщений в таблице Outbox, которые ещё не опубликованы.
    /// </summary>
    public class OutboxHealthCheck : IHealthCheck
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Создаёт новый экземпляр <see cref="OutboxHealthCheck"/>.
        /// </summary>
        /// <param name="db">Экземпляр <see cref="AppDbContext"/> для доступа к Outbox-таблице.</param>
        public OutboxHealthCheck(AppDbContext db)
        {
            _context = db;
        }

        /// <summary>
        /// Выполняет проверку здоровья для Outbox.
        /// </summary>
        /// <param name="context">Контекст проверки здоровья.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>
        /// <see cref="HealthCheckResult.Healthy"/> если отставание в пределах нормы,
        /// <see cref="HealthCheckResult.Degraded"/> если в очереди больше 100 непубликованных сообщений.
        /// </returns>
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var pendingCount = await _context.OutboxMessages.CountAsync(message => message.Status != nameof(MessageStatus.Published), cancellationToken);

            if (pendingCount > 100)
            {
                return HealthCheckResult.Degraded($"Outbox отстает: {pendingCount} сообщений");
            }

            return HealthCheckResult.Healthy($"Outbox ок: {pendingCount} сообщений");
        }
    }
}
