using AutoMapper;
using BankAccounts.Abstractions.CQRS;
using BankAccounts.Database.Interfaces;
using BankAccounts.Features.Accounts.DTOs;

namespace BankAccounts.Features.Accounts.GetAccount
{
    /// <summary>
    /// Обработчик запроса получения счета по идентификатору.
    /// </summary>
    public class GetAccountByIdQueryHandler : IQueryHandler<GetAccountByIdQuery, AccountDto?>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="GetAccountByIdQueryHandler"/>.
        /// </summary>
        /// <param name="accountRepository">Репозиторий для работы со счетами.</param>
        /// <param name="mapper">Автоматическое отображение моделей.</param>
        public GetAccountByIdQueryHandler(IAccountRepository accountRepository, IMapper mapper)
        {
            _accountRepository = accountRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Обрабатывает запрос получения счета по идентификатору.
        /// </summary>
        /// <param name="request">Запрос с идентификатором счета.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>DTO счета или null, если счет не найден.</returns>
        public async Task<AccountDto?> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
            return _mapper.Map<AccountDto>(account);
        }
    }
}
