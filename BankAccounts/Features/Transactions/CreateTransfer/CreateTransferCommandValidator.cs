using BankAccounts.Abstractions.Services;
using BankAccounts.Database.Interfaces;
using BankAccounts.Features.Accounts;
using FluentValidation;
using JetBrains.Annotations;

namespace BankAccounts.Features.Transactions.CreateTransfer
{
    /// <summary>
    /// Валидатор команды создания перевода между счетами.
    /// Проверяет корректность и бизнес-правила для данных транзакции.
    /// </summary>
    [UsedImplicitly]
    public class CreateTransferCommandValidator : AbstractValidator<CreateTransferCommand>
    {
        /// <summary>
        /// Инициализирует новый экземпляр <see cref="CreateTransferCommandValidator"/>.
        /// </summary>
        /// <param name="accountRepository">Репозиторий для доступа к счетам.</param>
        /// <param name="verificationService">Сервис для проверки верификации владельцев счетов.</param>
        /// <param name="currencyService">Сервис для проверки поддерживаемых валют.</param>
        public CreateTransferCommandValidator(IAccountRepository accountRepository,
            ICustomerVerificationService verificationService, ICurrencyService currencyService)
        {
            // Проверка существования и открытости счета источника
            RuleFor(x => x.TransactionDto.AccountId)
                .MustAsync(async (accountId, ct) =>
                {
                    var account = await accountRepository.GetByIdAsync(accountId, ct);
                    return account != null && account.CloseDate == null;
                })
                .WithMessage("Дебетовый счет с указанным идентификатором не найден или уже закрыт.");
            // Проверка существования и открытости счета контрагента
            RuleFor(x => x.TransactionDto.CounterpartyAccountId)
                .MustAsync(async (accountId, ct) =>
                {
                    if (!accountId.HasValue)
                    {
                        return false;
                    }
                    var account = await accountRepository.GetByIdAsync(accountId.Value, ct);
                    return account != null && account.CloseDate == null;
                })
                .WithMessage("Кредитовый счет с указанным идентификатором не найден или уже закрыт.");
            // Обязательность идентификатора контрагента
            RuleFor(x => x.TransactionDto.CounterpartyAccountId)
                .NotEmpty().WithMessage("Идентификатор контрагента при переводе должен быть указан.");
            // Кастомная логика проверки счетов, владельцев, закрытия и совпадения счетов
            RuleFor(x => x)
                .CustomAsync(async (command, context, ct) =>
                {
                    if (command.TransactionDto.CounterpartyAccountId == Guid.Empty || command.TransactionDto.CounterpartyAccountId == null)
                        return;

                    var sourceAccount = await accountRepository.GetByIdAsync(command.TransactionDto.AccountId, ct);
                    var targetAccount = await accountRepository.GetByIdAsync(command.TransactionDto.CounterpartyAccountId.Value, ct);

                    if (sourceAccount == null)
                    {
                        context.AddFailure(nameof(command.TransactionDto.AccountId), "Счет источника не найден.");
                        return;
                    } 
                    else if (targetAccount == null)
                    {
                        context.AddFailure(nameof(command.TransactionDto.CounterpartyAccountId), "Счет контрагента не найден.");
                        return;
                    }

                    var sourceOwnerExists = await verificationService.OwnerExistsAsync(sourceAccount.OwnerId, ct);
                    var targetOwnerExists = await verificationService.OwnerExistsAsync(targetAccount.OwnerId, ct);

                    if (!sourceOwnerExists)
                    {
                        context.AddFailure(nameof(sourceAccount.OwnerId), "Владелец счета источника не верифицирован.");
                    }
                    else if (!targetOwnerExists)
                    {
                        context.AddFailure(nameof(targetAccount.OwnerId), "Владелец счета контрагента не верифицирован.");
                    }

                    if (sourceAccount.CloseDate != null)
                    {
                        context.AddFailure(nameof(targetAccount.OwnerId), "Cчет источника закрыт.");
                    }
                    else if (targetAccount.CloseDate != null)
                    {
                        context.AddFailure(nameof(targetAccount.OwnerId), "Cчет контрагента закрыт.");
                    }

                    if (sourceAccount.Id == targetAccount.Id)
                    {
                        context.AddFailure(nameof(targetAccount.Id), "Указаны одинаковые идентификаторы.");
                    }
                });
            // Проверка валидности валюты исходной транзакции
            RuleFor(x => x.TransactionDto.Currency)
                .NotEmpty().WithMessage("Поле \"Валюта\" обязательно к заполнению.")
                .Length(3).WithMessage("Валюта должна иметь трехбуквенный код ISO.")
                .Must(currencyCode => currencyService.IsSupported(currencyCode))
                .WithMessage("Валюта счета источника не поддерживается.");
            // Проверка валюты контрагента и запрет мультивалютных операций
            RuleFor(x => x.TransactionDto)
                .CustomAsync(async (dto, context, ct) =>
                {
                    if (dto.CounterpartyAccountId == Guid.Empty || dto.CounterpartyAccountId == null)
                        return;

                    var targetAccount = await accountRepository.GetByIdAsync(dto.CounterpartyAccountId.Value, ct);

                    if (targetAccount == null)
                        return;

                    if (!currencyService.IsSupported(targetAccount.Currency))
                    {
                        context.AddFailure(nameof(targetAccount.Currency), "Валюта счета контрагента не поддерживается.");
                    }

                    if (!dto.Currency.Equals(targetAccount.Currency))
                    {
                        context.AddFailure(nameof(targetAccount.Currency), "Мультивалютные операции не поддерживаются.");
                    }
                });
            // Проверка типа транзакции
            RuleFor(x => x.TransactionDto.Type)
                .NotEmpty().WithMessage("Тип транзакции должен быть указан.")
                .Must(x => Enum.TryParse<TransactionType>(x, true, out _))
                .WithMessage("Неверно указан тип транзакции. Тип должен быть Debit или Credit.");

            // Проверка суммы перевода
            RuleFor(x => x.TransactionDto.Amount)
                .GreaterThan(0).WithMessage("Сумма для перевода должна быть положительной.");
            
            // Проверка баланса и типов счетов для списания/зачисления
            RuleFor(x => x.TransactionDto)
                .CustomAsync(async (dto, context, ct) =>
                {
                    if (dto.CounterpartyAccountId == Guid.Empty || dto.CounterpartyAccountId == null)
                        return;

                    var targetAccount = await accountRepository.GetByIdAsync(dto.CounterpartyAccountId.Value, ct);
                    var sourceAccount = await accountRepository.GetByIdAsync(dto.AccountId, ct);

                    if (targetAccount == null || sourceAccount == null)
                        return;

                    if (!Enum.TryParse<TransactionType>(dto.Type, out var type))
                        return;
                    


                    if (type == TransactionType.Credit)
                    {
                        if (sourceAccount.Type == AccountType.Deposit || sourceAccount.Type == AccountType.Credit)
                        {
                            context.AddFailure(nameof(sourceAccount.Type),
                                "Запрещено снятие денежных средств с счетов типа Deposit или Credit.");
                        }

                        if (sourceAccount.Balance < dto.Amount)
                        {
                            context.AddFailure(nameof(targetAccount.Currency),
                                "На балансе счета источника недостаточно средств.");
                        }
                    }
                    else
                    {
                        if (targetAccount.Type == AccountType.Deposit || targetAccount.Type == AccountType.Credit)
                        {
                            context.AddFailure(nameof(targetAccount.Type),
                                "Запрещено снятие денежных средств с счетов типа Deposit или Credit.");
                        }

                        if (targetAccount.Balance < dto.Amount)
                        {
                            context.AddFailure(nameof(targetAccount.Currency),
                                "На балансе счета контрагента недостаточно средств.");
                        }
                    }
                });
            // Проверка описания транзакции
            RuleFor(x => x.TransactionDto.Description)
                .NotEmpty().WithMessage("Описание транзакции обязательно.")
                .MaximumLength(255).WithMessage("Описание не должно превышать 255 символов.");


        }
    }
}
