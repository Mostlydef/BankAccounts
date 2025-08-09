using AutoMapper;
using BankAccounts.Abstractions.CQRS;
using BankAccounts.Common.Results;
using BankAccounts.Database.Interfaces;
using BankAccounts.Features.Transactions.DTOs;

namespace BankAccounts.Features.Transactions.CreateTransaction
{
    /// <summary>
    /// Обработчик команды создания простой транзакции (ввод или снятие средств).
    /// </summary>
    public class CreateTransactionCommandHandler : ICommandHandler<CreateTransactionCommand, MbResult<TransactionDto?>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="CreateTransactionCommandHandler"/>.
        /// </summary>
        /// <param name="transactionRepository">Репозиторий для работы с транзакциями.</param>
        /// <param name="repository">Репозиторий для работы с банковскими счетами.</param>
        /// <param name="mapper">Автоматический маппер объектов.</param>
        public CreateTransactionCommandHandler(ITransactionRepository transactionRepository, IAccountRepository repository, IMapper mapper)
        {
            _transactionRepository = transactionRepository;
            _accountRepository = repository;
            _mapper = mapper;
        }

        /// <summary>
        /// Обрабатывает команду создания транзакции.
        /// Выполняет обновление баланса счета и сохраняет транзакцию.
        /// </summary>
        /// <param name="request">Команда создания транзакции с данными транзакции.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>DTO созданной транзакции либо <c>null</c>, если операция не удалась.</returns>
        public async Task<MbResult<TransactionDto?>> Handle(CreateTransactionCommand request,
            CancellationToken cancellationToken)
        {
            var transaction = _mapper.Map<Transaction>(request.TransactionDto);

            await using var tx = await _transactionRepository.BeginTransationAsync();

            try
            {
                var account = await _accountRepository.GetByIdAsync(transaction.AccountId, cancellationToken);
                if (account == null)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return MbResult<TransactionDto?>.NotFound("Счет не найден.");
                }

                var balanceBegin = account.Balance;

                if (transaction.Type == TransactionType.Credit)
                {
                    account.Balance -= transaction.Amount;
                }
                else
                {
                    account.Balance += transaction.Amount;
                }

                await _transactionRepository.RegisterAsync(transaction);

                if (account.Balance != balanceBegin - transaction.Amount &&
                    transaction.Type == TransactionType.Credit ||
                    account.Balance != balanceBegin + transaction.Amount && transaction.Type == TransactionType.Debit)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return MbResult<TransactionDto?>.BadRequest("Итоговый баланс не соответствует ожиданиям.");
                }

                await tx.CommitAsync(cancellationToken);
            }
            catch
            {
                await tx.RollbackAsync(cancellationToken);
                return MbResult<TransactionDto?>.BadRequest("Ошибка во время выполнения транзакции.");
            }

            var dto = _mapper.Map<TransactionDto>(transaction);

            return MbResult<TransactionDto?>.Success(dto);
        }

    }
}
