using AutoMapper;
using BankAccounts.Abstractions.CQRS;
using BankAccounts.Common.Results;
using BankAccounts.Database.Interfaces;
using BankAccounts.Features.Accounts.DTOs;
using BankAccounts.Features.Accounts.Events;
using BankAccounts.Infrastructure.Rabbit.PublishEvents;

namespace BankAccounts.Features.Accounts.CreateAccount
{
    /// <summary>
    /// Обработчик команды создания нового аккаунта.
    /// </summary>
    public class CreateAccountCommandHandler : ICommandHandler<CreateAccountCommand, MbResult<AccountDto>>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;
        private readonly IPublishEvent _publishEvent;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="CreateAccountCommandHandler"/>.
        /// </summary>
        /// <param name="repository">Репозиторий для работы с аккаунтами.</param>
        /// <param name="mapper">Автомаппер для преобразования DTO и сущностей.</param>
        /// <param name="publishEvent">Сервис для публикации событий (event publishing) после успешного выполнения команды.</param>
        public CreateAccountCommandHandler(IAccountRepository repository, IMapper mapper, IPublishEvent publishEvent)
        {
            _accountRepository = repository;
            _mapper = mapper;
            _publishEvent = publishEvent;
        }

        /// <summary>
        /// Обрабатывает команду создания аккаунта, добавляя новый аккаунт в базу и возвращая DTO созданного аккаунта.
        /// </summary>
        /// <param name="request">Команда создания аккаунта с данными для создания.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>DTO созданного аккаунта.</returns>
        public async Task<MbResult<AccountDto>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            var account = _mapper.Map<Account>(request.CreateDto);

            await _accountRepository.AddAsync(account);

            var accountOpenedEvent = new AccountOpenedEvent
            {
                AccountId = account.Id,
                Currency = account.Currency,
                OwnerId = account.OwnerId,
                Type = account.Type.ToString()
            };

            await _publishEvent.PublishEventAsync(accountOpenedEvent, account.Id);

            await _accountRepository.SaveChangesAsync();

            var dto = _mapper.Map<AccountDto>(account);

            return MbResult<AccountDto>.Success(dto);
        }
    }
}
