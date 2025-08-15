using AutoMapper;
using BankAccounts.Abstractions.CQRS;
using BankAccounts.Common.Results;
using BankAccounts.Database.Interfaces;
using BankAccounts.Features.Transactions.DTOs;
using BankAccounts.Features.Transactions.Events;
using BankAccounts.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;

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
        private readonly IPublishEvent _publishEvent;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="CreateTransactionCommandHandler"/>.
        /// </summary>
        /// <param name="transactionRepository">Репозиторий для работы с транзакциями.</param>
        /// <param name="repository">Репозиторий для работы с банковскими счетами.</param>
        /// <param name="mapper">Автоматический маппер объектов.</param>
        public CreateTransactionCommandHandler(ITransactionRepository transactionRepository, IAccountRepository repository, IMapper mapper, IPublishEvent publishEvent)
        {
            _transactionRepository = transactionRepository;
            _accountRepository = repository;
            _mapper = mapper;
            _publishEvent = publishEvent;
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

            await using var tx = await _transactionRepository.BeginTransactionAsync();

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
                    var moneyCreditedEvent = new MoneyCreditedEvent
                    {
                        AccountId = transaction.AccountId,
                        Amount = transaction.Amount,
                        Currency = transaction.Currency,
                        EventId = Guid.NewGuid(),
                        OccurredAt = DateTimeOffset.UtcNow,
                        OperationId = transaction.Id
                    };
                    await _publishEvent.PublishEventAsync(moneyCreditedEvent, transaction.AccountId);
                }
                else
                {
                    account.Balance += transaction.Amount;
                    var moneyDebitedEvent = new MoneyDebitedEvent()
                    {
                        AccountId = transaction.AccountId,
                        Amount = transaction.Amount,
                        Currency = transaction.Currency,
                        EventId = Guid.NewGuid(),
                        OccurredAt = DateTimeOffset.UtcNow,
                        OperationId = transaction.Id
                    };
                    await _publishEvent.PublishEventAsync(moneyDebitedEvent, transaction.AccountId);
                }

                await _transactionRepository.RegisterAsync(transaction);
                await _transactionRepository.SaveChangesAsync();

                if (account.Balance != balanceBegin - transaction.Amount &&
                    transaction.Type == TransactionType.Credit ||
                    account.Balance != balanceBegin + transaction.Amount && transaction.Type == TransactionType.Debit)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return MbResult<TransactionDto?>.BadRequest("Итоговый баланс не соответствует ожиданиям.");
                }

                await tx.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync(cancellationToken);
                return MbResult<TransactionDto?>.Conflict("Данные были изменены другим пользователем.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(cancellationToken);
                return MbResult<TransactionDto?>.BadRequest(ex.Message);
            }

            var dto = _mapper.Map<TransactionDto>(transaction);

            return MbResult<TransactionDto?>.Success(dto);
        }

    }
}
