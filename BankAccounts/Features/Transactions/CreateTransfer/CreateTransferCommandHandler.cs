using AutoMapper;
using BankAccounts.Abstractions.CQRS;
using BankAccounts.Common.Results;
using BankAccounts.Database.Interfaces;
using BankAccounts.Features.Transactions.DTOs;
using BankAccounts.Features.Transactions.Events;
using BankAccounts.Infrastructure.Rabbit.PublishEvents;
using Microsoft.EntityFrameworkCore;

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
        private readonly IPublishEvent _publishEvent;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="CreateTransferCommandHandler"/>.
        /// </summary>
        /// <param name="transactionRepository">Репозиторий для работы с транзакциями.</param>
        /// <param name="accountRepository">Репозиторий для работы со счетами.</param>
        /// <param name="mapper">Объект для маппинга между DTO и моделями.</param>
        public CreateTransferCommandHandler(ITransactionRepository transactionRepository, IAccountRepository accountRepository, IMapper mapper, IPublishEvent publishEvent)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _mapper = mapper;
            _publishEvent = publishEvent;
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
            TransactionDto? dto;
            MoneyCreditedEvent moneyCreditedEvent;
            MoneyDebitedEvent moneyDebitedEvent;
            await using var tx = await _transactionRepository.BeginTransactionAsync();
            try
            {
                if (transaction.CounterpartyAccountId == null)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return MbResult<TransactionDto?>.BadRequest("Контрагент не указан.");
                }

                // Получаем счета источника и получателя
                var sourceAccount = await _accountRepository.GetByIdAsync(transaction.AccountId, cancellationToken);
                var targetAccount =
                    await _accountRepository.GetByIdAsync(transaction.CounterpartyAccountId.Value, cancellationToken);

                if (sourceAccount == null || targetAccount == null)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return MbResult<TransactionDto?>.NotFound("Счет не найден.");
                }

                // Создаем обратную транзакцию для контрагента
                var otherTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = targetAccount.Id,
                    CounterpartyAccountId = sourceAccount.Id,
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    Description = transaction.Description,
                    Timestamp = DateTime.UtcNow,
                    Type = TransactionType.Credit
                };

                var sourceBalanceStart = sourceAccount.Balance;
                var targetBalanceStart = targetAccount.Balance;

                // Обновляем балансы счетов в зависимости от типа транзакции
                if (transaction.Type == TransactionType.Credit)
                {
                    // Снимаем деньги с источника, добавляем к получателю
                    sourceAccount.Balance -= transaction.Amount;
                    targetAccount.Balance += transaction.Amount;
                    moneyCreditedEvent = new MoneyCreditedEvent
                    {
                        AccountId = transaction.AccountId,
                        Amount = transaction.Amount,
                        Currency = transaction.Currency,
                        EventId = Guid.NewGuid(),
                        OccurredAt = DateTimeOffset.UtcNow,
                        OperationId = transaction.Id
                    };
                    moneyDebitedEvent = new MoneyDebitedEvent
                    {
                        AccountId = otherTransaction.AccountId,
                        Amount = otherTransaction.Amount,
                        Currency = otherTransaction.Currency,
                        EventId = Guid.NewGuid(),
                        OccurredAt = DateTimeOffset.UtcNow,
                        OperationId = otherTransaction.Id
                    };
                    await _publishEvent.PublishEventAsync(moneyDebitedEvent, otherTransaction.AccountId);
                    await _publishEvent.PublishEventAsync(moneyCreditedEvent, transaction.AccountId);
                }
                else
                {
                    // Сценарий, когда получатель инициирует зачисление
                    sourceAccount.Balance += transaction.Amount;
                    targetAccount.Balance -= transaction.Amount;
                    otherTransaction.Type = TransactionType.Debit;

                    moneyCreditedEvent = new MoneyCreditedEvent
                    {
                        AccountId = otherTransaction.AccountId,
                        Amount = otherTransaction.Amount,
                        Currency = otherTransaction.Currency,
                        EventId = Guid.NewGuid(),
                        OccurredAt = DateTimeOffset.UtcNow,
                        OperationId = otherTransaction.Id
                    };
                    moneyDebitedEvent = new MoneyDebitedEvent
                    {
                        AccountId = transaction.AccountId,
                        Amount = transaction.Amount,
                        Currency = transaction.Currency,
                        EventId = Guid.NewGuid(),
                        OccurredAt = DateTimeOffset.UtcNow,
                        OperationId = transaction.Id
                    };
                    await _publishEvent.PublishEventAsync(moneyCreditedEvent, otherTransaction.AccountId);
                    await _publishEvent.PublishEventAsync(moneyDebitedEvent, transaction.AccountId);
                }

                if ((sourceBalanceStart - transaction.Amount != sourceAccount.Balance ||
                     targetBalanceStart + transaction.Amount != targetAccount.Balance) &&
                    transaction.Type == TransactionType.Credit ||
                    (sourceBalanceStart + transaction.Amount != sourceAccount.Balance ||
                     targetBalanceStart - transaction.Amount != targetAccount.Balance) &&
                    transaction.Type == TransactionType.Debit)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return MbResult<TransactionDto?>.BadRequest("Итоговые балансы не соответствуют ожиданиям.");
                }


                // Привязываем транзакцию к счету источника
                await _transactionRepository.RegisterAsync(transaction);
                await _transactionRepository.RegisterAsync(otherTransaction);
                await _transactionRepository.SaveChangesAsync();

                dto = _mapper.Map<TransactionDto>(otherTransaction);

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

            return MbResult<TransactionDto?>.Success(dto);
        }
    }
}
