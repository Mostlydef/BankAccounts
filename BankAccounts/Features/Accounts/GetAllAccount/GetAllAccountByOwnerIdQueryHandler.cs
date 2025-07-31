using AutoMapper;
using BankAccounts.Abstractions.CQRS;
using BankAccounts.Common.Results;
using BankAccounts.Database.Interfaces;
using BankAccounts.Features.Accounts.DTOs;

namespace BankAccounts.Features.Accounts.GetAllAccount
{
    /// <summary>
    /// Обработчик запроса получения всех счетов по идентификатору владельца.
    /// </summary>
    public class GetAllAccountByOwnerIdQueryHandler : IQueryHandler<GetAllAccountByOwnerIdQuery, MbResult<List<AccountDto>>>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Конструктор для <see cref="GetAllAccountByOwnerIdQueryHandler"/>.
        /// </summary>
        /// <param name="accountRepository">Репозиторий для работы со счетами.</param>
        /// <param name="mapper">Автомаппер для преобразования сущностей в DTO.</param>
        public GetAllAccountByOwnerIdQueryHandler(IAccountRepository accountRepository, IMapper mapper)
        {
            _accountRepository = accountRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Обрабатывает запрос получения всех счетов для указанного владельца.
        /// </summary>
        /// <param name="request">Запрос, содержащий идентификатор владельца.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Список DTO счетов, принадлежащих владельцу.</returns>
        public async Task<MbResult<List<AccountDto>>> Handle(GetAllAccountByOwnerIdQuery request, CancellationToken cancellationToken)
        {
            var list = await _accountRepository.GetByOwnerIdAsync(request.OwnerId);
            var listDto = list.Select(_mapper.Map<AccountDto>).ToList();
            return MbResult<List<AccountDto>>.Success(listDto);
        }
    }
}
