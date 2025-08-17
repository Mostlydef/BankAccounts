using BankAccounts.Database.Interfaces;
using BankAccounts.Features.Accounts.Events;
using BankAccounts.Infrastructure.Rabbit.PublishEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankAccounts.Features.Accounts
{
    /// <summary>
    /// Контроллер-заглушка для блокировки и разблокировки аккаунта клиента.
    /// Используется для тестирования взаимодействия с событиями RabbitMQ.
    /// </summary>
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]

    public class AccountFrozenStubController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IPublishEvent _publishEvent;

        /// <summary>
        /// Конструктор контроллера.
        /// </summary>
        /// <param name="accountRepository">Репозиторий аккаунтов.</param>
        /// <param name="publishEvent">Сервис публикации событий.</param>
        public AccountFrozenStubController(IAccountRepository accountRepository, IPublishEvent publishEvent)
        {
            _accountRepository = accountRepository;
            _publishEvent = publishEvent;
        }

        /// <summary>
        /// Блокирует все аккаунты клиента и публикует событие ClientBlockedEvent.
        /// </summary>
        /// <param name="ownerId">Идентификатор владельца аккаунта.</param>
        /// <returns>HTTP 200 OK при успешной блокировке.</returns>
        [HttpPatch("{ownerId}/Frozen")]
        public async Task<IActionResult> FrozeAccount(Guid ownerId)
        {
            var account = await _accountRepository.GetByOwnerIdAsync(ownerId);

            if (!account.Any())
            {
                return NotFound(); // или другой ответ, если счетов нет
            }

            var clientBlockedEvent = new ClientBlockedEvent
            {
                ClientId = ownerId,
                EventId = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.UtcNow
            };
            await _publishEvent.PublishEventAsync(clientBlockedEvent, account[0].Id);
            await _accountRepository.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Разблокирует все аккаунты клиента и публикует событие ClientUnblockedEvent.
        /// </summary>
        /// <param name="ownerId">Идентификатор владельца аккаунта.</param>
        /// <returns>HTTP 200 OK при успешной разблокировке.</returns>
        [HttpPatch("{ownerId}/Unfrozen")]
        public async Task<IActionResult> UnfrozenAccount(Guid ownerId)
        {
            var account = await _accountRepository.GetByOwnerIdAsync(ownerId);

            if (!account.Any())
            {
                return NotFound(); // или другой ответ, если счетов нет
            }

            var clientUnblockedEvent = new ClientUnblockedEvent()
            {
                ClientId = ownerId,
                EventId = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.UtcNow
            };

            await _publishEvent.PublishEventAsync(clientUnblockedEvent, account[0].Id);
            await _accountRepository.SaveChangesAsync();
            return Ok();
        }
    }
}
