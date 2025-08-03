using AutoMapper;
using BankAccounts.Abstractions.CQRS;
using BankAccounts.Common.Results;
using BankAccounts.Database.Interfaces;
using BankAccounts.Features.Transactions.DTOs;

namespace BankAccounts.Features.Transactions.CreateTransfer
{
    /// <summary>
    /// Обработчик команды создания перевода между счетами.
    /// </summary>
    public class CreateTransferCommandHandler : ICommandHandler<CreateTransferCommand, MbResult<TransactionDto?>>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="CreateTransferCommandHandler"/>.
        /// </summary>
        /// <param name="transactionRepository">Репозиторий для работы с транзакциями.</param>
        /// <param name="accountRepository">Репозиторий для работы со счетами.</param>
        /// <param name="mapper">Объект для маппинга между DTO и моделями.</param>
        public CreateTransferCommandHandler(ITransactionRepository transactionRepository, IAccountRepository accountRepository, IMapper mapper)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Обрабатывает команду создания перевода между счетами.
        /// </summary>
        /// <param name="request">Команда создания перевода с деталями транзакции.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>
        /// Объект <see cref="TransactionDto"/> с информацией о связанной транзакции или <c>null</c>, если перевод невозможен.
        /// </returns>
        public async Task<MbResult<TransactionDto?>> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
        {
            var transaction = _mapper.Map<Transaction>(request.TransactionDto);

            if (transaction.CounterpartyAccountId == null)
                return MbResult<TransactionDto?>.BadRequest("Контрагент не указан.");

            // Получаем счета источника и получателя
            var sourceAccount = await _accountRepository.GetByIdAsync(transaction.AccountId, cancellationToken);
            var targetAccount = await _accountRepository.GetByIdAsync(transaction.CounterpartyAccountId.Value, cancellationToken);

            if (sourceAccount == null || targetAccount == null)
                return MbResult<TransactionDto?>.NotFound("Счет не найден.");

            // Создаем обратную транзакцию для контрагента
            var otherTransaction = new Transaction()
            {
                Id = Guid.NewGuid(),
                AccountId = targetAccount.Id,
                CounterpartyAccountId = transaction.Id,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                Description = transaction.Description,
                Timestamp = DateTime.UtcNow,
                Account = targetAccount,
                Type = TransactionType.Credit
            };

            // Обновляем балансы счетов в зависимости от типа транзакции
            if (transaction.Type == TransactionType.Debit)
            {
                // Снимаем деньги с источника, добавляем к получателю
                sourceAccount.Balance -= transaction.Amount;
                targetAccount.Balance += transaction.Amount;
            }
            else if (transaction.Type == TransactionType.Credit)
            {
                // Сценарий, когда получатель инициирует зачисление
                sourceAccount.Balance += transaction.Amount;
                targetAccount.Balance -= transaction.Amount;
                otherTransaction.Type = TransactionType.Debit;
            }

            // Привязываем транзакцию к счету источника
            transaction.Account = sourceAccount;
            sourceAccount.Transactions.Add(transaction);
            targetAccount.Transactions.Add(otherTransaction);
            // Сохраняем транзакции и обновляем счета
            await _transactionRepository.RegisterAsync(otherTransaction);
            await _transactionRepository.RegisterAsync(transaction);
            await _accountRepository.UpdateAsync(sourceAccount);
            await _accountRepository.UpdateAsync(targetAccount);

            var dto = _mapper.Map<TransactionDto>(otherTransaction);

            return MbResult<TransactionDto?>.Success(dto);
        }
    }
}
