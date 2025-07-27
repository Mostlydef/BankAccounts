using BankAccounts.Abstractions.Services;
using FluentValidation;
using JetBrains.Annotations;

namespace BankAccounts.Features.Accounts.CreateAccount
{
    /// <summary>
    /// Валидатор команды создания аккаунта.
    /// Выполняет проверку корректности данных для создания счета,
    /// включая существование владельца, тип счета, валюту и процентную ставку.
    /// </summary>
    [UsedImplicitly]
    public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
    {
        /// <summary>
        /// Инициализирует новый экземпляр <see cref="CreateAccountCommandValidator"/>.
        /// </summary>
        /// <param name="verificationService">Сервис проверки существования владельца.</param>
        /// <param name="currencyService">Сервис проверки поддерживаемых валют.</param>
        public CreateAccountCommandValidator(ICustomerVerificationService verificationService, ICurrencyService currencyService)
        {
            // Проверка верификации владельца
            RuleFor(x => x.CreateDto.OwnerId)
                .MustAsync(async (ownerId, ct) =>
                    await verificationService.OwnerExistsAsync(ownerId, ct))
                .WithMessage("Владелец с указанным идентификатором не существует.");
            // Проверка типа счета
            RuleFor(x => x.CreateDto.AccountType)
                .NotEmpty().WithMessage("Поле \"Тип счёта\" обязательно к заполнению.")
                .Must(value => Enum.TryParse<AccountType>(value, ignoreCase: true, out _))
                .WithMessage("Тип счёта должен быть: Checking, Deposit, Credit.");
            // Проверка валюты и её поддержки
            RuleFor(x => x.CreateDto.Currency)
                .NotEmpty().WithMessage("Поле \"Валюта\" обязательно к заполнению.")
                .Length(3).WithMessage("Валюта должна иметь трехбуквенный код ISO.")
                .Must(currencyCode => currencyService.IsSupported(currencyCode))
                .WithMessage("Валюта не поддерживается.");
            // Проверка соответствия процентной ставки и типа аккаунта
            RuleFor(x => x.CreateDto)
                .Custom((dto, context) =>
                {
                    if (!Enum.TryParse<AccountType>(dto.AccountType, true, out var parsedType))
                    {
                        return;
                    }

                    switch (parsedType)
                    {
                        case AccountType.Checking:
                            if (dto.InterestRate.HasValue)
                            {
                                context.AddFailure(nameof(dto.InterestRate),
                                    "Для расчетного счета процентная ставка не должна быть указана.");
                            }
                            break;
                        case AccountType.Deposit:
                        case AccountType.Credit:
                            if (!dto.InterestRate.HasValue)
                            {
                                context.AddFailure(nameof(dto.InterestRate),
                                    "Для данного типа счета необходимо указать процентную ставку.");
                            }
                            else if (dto.InterestRate < 0)
                            {
                                context.AddFailure(nameof(dto.InterestRate),
                                    "Ставка процента должна быть больше или равна 0.");
                            }
                            break;
                    }
                });
        }
    }
}
