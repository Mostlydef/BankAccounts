using BankAccounts.Abstractions.Services;
using BankAccounts.Database.Interfaces;
using BankAccounts.Features.Accounts;
using FluentValidation;
using JetBrains.Annotations;

namespace BankAccounts.Features.Transactions.CreateTransaction
{
    /// <summary>
    /// Валидатор для команды создания транзакции.
    /// Выполняет проверку корректности данных транзакции,
    /// включая проверку существования и статуса счета, верификации владельца,
    /// корректности валюты, типа транзакции, суммы и описания.
    /// </summary>
    [UsedImplicitly]
    public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
    {
        /// <summary>
        /// Инициализирует новый экземпляр <see cref="CreateTransactionCommandValidator"/>.
        /// </summary>
        /// <param name="accountRepository">Репозиторий для доступа к счетам.</param>
        /// <param name="verificationService">Сервис для верификации владельцев счетов.</param>
        /// <param name="currencyService">Сервис проверки поддерживаемых валют.</param>
        public CreateTransactionCommandValidator(IAccountRepository accountRepository,
            ICustomerVerificationService verificationService, ICurrencyService currencyService)
        {
            // Проверка счета на закрытие
            RuleFor(x => x.TransactionDto.AccountId)
                .MustAsync(async (accountId, ct) =>
                {
                    var account = await accountRepository.GetByIdAsync(accountId, ct);
                    return account != null && account.CloseDate == null;
                })
                .WithMessage("Счет с указанным идентификатором закрыт.");
            // Проверка существования счета и верификации обладателя счета
            RuleFor(x => x)
                .CustomAsync(async (command, context, ct) =>
                {
                    var account = await accountRepository.GetByIdAsync(command.TransactionDto.AccountId, ct);
                    if (account == null)
                    {
                        context.AddFailure(nameof(command.TransactionDto.AccountId), "Счет не найден.");
                        return;
                    }

                    var ownerExists = await verificationService.OwnerExistsAsync(account.OwnerId, ct);
                    if (!ownerExists)
                    {
                        context.AddFailure(nameof(command.TransactionDto.AccountId), "Владелец счета не найден.");
                    }
                });
            // Проверка валюты и её поддержки
            RuleFor(x => x.TransactionDto.Currency)
                .NotEmpty().WithMessage("Поле \"Валюта\" обязательно к заполнению.")
                .Length(3).WithMessage("Валюта должна иметь трехбуквенный код ISO.")
                .Must(currencyCode => currencyService.IsSupported(currencyCode))
                .WithMessage("Валюта не поддерживается.");
            // Проверка суммы перевода
            RuleFor(x => x.TransactionDto.Amount)
                .GreaterThan(0).WithMessage("Сумма должна быть положительной.");
            // Проверка типа транзакции
            RuleFor(x => x.TransactionDto.Type)
                .NotEmpty().WithMessage("Поле \"Тип транзакции\" обязательно к заполнению.")
                .Must(type => Enum.TryParse<TransactionType>(type, true, out _))
                .WithMessage("Недопустимый тип транзакции. Допустимые значения: Credit, Debit.");
            // Проверка описания транзакции
            RuleFor(x => x.TransactionDto.Description)
                .NotEmpty().WithMessage("Описание обязательно.")
                .MaximumLength(255).WithMessage("Описание не должно превышать 255 символов.");
            // Проверка на запрет мультивалютных операций, снятия со счетов типа Deposit или Credit, проверка баланса
            RuleFor(x => x)
                .CustomAsync(async (command, context, ct) =>
                {
                    if (!Enum.TryParse(command.TransactionDto.Type, true, out TransactionType type))
                    {
                        return;
                    }

                    if (type == TransactionType.Debit)
                    {
                        var account = await accountRepository.GetByIdAsync(command.TransactionDto.AccountId, ct);
                        if (account == null)
                            return;

                        if (!command.TransactionDto.Currency.Equals(account.Currency))
                        {
                            context.AddFailure(nameof(command.TransactionDto.Currency),
                                "Мультивалютные транзакции запрещены.");
                        }


                        if (Enum.Parse<TransactionType>(command.TransactionDto.Type) == TransactionType.Debit &&
                            (account.Type == AccountType.Credit || account.Type == AccountType.Deposit))
                        {
                            context.AddFailure(nameof(account.Type),
                                "Запрещено снятие денежных средств со счетов типа Deposit или Credit.");
                        }

                        if (account.Balance < command.TransactionDto.Amount)
                            context.AddFailure(nameof(command.TransactionDto.Amount),
                                "Недостаточно средств на счете");
                    }
                });
            // Проверка на пустой Guid контрагента
            RuleFor(x => x.TransactionDto.CounterpartyAccountId)
                .Empty().WithMessage("Для регистрации транзакции счет контрагента не указывается.");

        }
    }
}
