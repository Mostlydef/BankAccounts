using AutoMapper;
using BankAccounts.Abstractions.CQRS;
using BankAccounts.Database.Interfaces;
using BankAccounts.Features.Accounts.DTOs;
using BankAccounts.Features.Transactions.DTOs;

namespace BankAccounts.Features.Accounts.GetStatement
{
    /// <summary>
    /// Обработчик запроса для получения выписки по счету за указанный период.
    /// </summary>
    public class GetStatementQueryHandler : IQueryHandler<GetStatementQuery, StatementDto?>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="GetStatementQueryHandler"/>.
        /// </summary>
        /// <param name="accountRepository">Репозиторий для работы со счетами.</param>
        /// <param name="mapper">Автоматическое сопоставление объектов.</param>
        public GetStatementQueryHandler(IAccountRepository accountRepository, IMapper mapper)
        {
            _accountRepository = accountRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Обрабатывает запрос на получение выписки по счету.
        /// </summary>
        /// <param name="request">Запрос с параметрами: идентификатор счета, дата начала и окончания периода.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Выписка по счету с транзакциями за период или null, если счет не найден.</returns>
        public async Task<StatementDto?> Handle(GetStatementQuery request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByIdAsync(request.Id, cancellationToken);
            if (account == null)
                return null;

            var transactions = await _accountRepository.GetTransactions(request.Id, request.From, request.To);

            var transactionsDto = _mapper.Map<List<TransactionDto>>(transactions);

            var statementDto = new StatementDto
            {
                Id = request.Id,
                OwnerId = account.OwnerId,
                TransactionsDto = transactionsDto
            };

            return statementDto;
        }
    }
}
