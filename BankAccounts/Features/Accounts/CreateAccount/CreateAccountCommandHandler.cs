using AutoMapper;
using BankAccounts.Abstractions.CQRS;
using BankAccounts.Database.Interfaces;
using BankAccounts.Features.Accounts.DTOs;

namespace BankAccounts.Features.Accounts.CreateAccount
{
    /// <summary>
    /// Обработчик команды создания нового аккаунта.
    /// </summary>
    public class CreateAccountCommandHandler : ICommandHandler<CreateAccountCommand, AccountDto>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="CreateAccountCommandHandler"/>.
        /// </summary>
        /// <param name="repository">Репозиторий для работы с аккаунтами.</param>
        /// <param name="mapper">Автомаппер для преобразования DTO и сущностей.</param>
        public CreateAccountCommandHandler(IAccountRepository repository, IMapper mapper)
        {
            _accountRepository = repository;
            _mapper = mapper;
        }

        /// <summary>
        /// Обрабатывает команду создания аккаунта, добавляя новый аккаунт в базу и возвращая DTO созданного аккаунта.
        /// </summary>
        /// <param name="request">Команда создания аккаунта с данными для создания.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>DTO созданного аккаунта.</returns>
        public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            var account = _mapper.Map<Account>(request.CreateDto);

            await _accountRepository.AddAsync(account);
            return _mapper.Map<AccountDto>(account);
        }
    }
}
