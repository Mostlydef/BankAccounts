using BankAccounts.Database.Interfaces;
using FluentValidation;
using JetBrains.Annotations;

namespace BankAccounts.Features.Accounts.UpdateAccountInterestRate
{
    /// <summary>
    /// Валидатор для команды обновления процентной ставки по счету.
    /// Проверяет существование и статус счета, а также соответствие бизнес-правилам для процентной ставки
    /// в зависимости от типа счета.
    /// </summary>
    [UsedImplicitly]
    public class UpdateInterestRateCommandValidator : AbstractValidator<UpdateInterestRateCommand>
    {
        /// <summary>
        /// Инициализирует новый экземпляр <see cref="UpdateInterestRateCommandValidator"/> с зависимостью репозитория счетов.
        /// </summary>
        /// <param name="repository">Репозиторий для доступа к счетам.</param>
        public UpdateInterestRateCommandValidator(IAccountRepository repository)
        {
            // Проверка закрыт ли аккаунт
            RuleFor(x => x.AccountId)
                .MustAsync(async (accountId, ct) =>
                {
                    var account = await repository.GetByIdAsync(accountId, ct);
                    return account != null && account.CloseDate == null;
                })
                .WithMessage("Аккаунт с указанным идентификатором не найден или уже закрыт.");

            // Проверка ставки для типов счетов
            RuleFor(x => x)
                .CustomAsync(async (command, context, ct) =>
                {
                    var account = await repository.GetByIdAsync(command.AccountId, ct);
                    if (account == null)
                        return;

                    switch (account.Type)
                    {
                        case AccountType.Checking:
                            context.AddFailure(nameof(command.InterestRateDto.InterestRate),
                                "Для расчетного счета процентная ставка не должна быть указана.");
                            break;
                        case AccountType.Deposit:
                            context.AddFailure(nameof(command.InterestRateDto.InterestRate),
                                "Для депозитного счета процентная ставка не изменяема.");
                            break;
                        case AccountType.Credit:
                            if (command.InterestRateDto.InterestRate < 0)
                            {
                                context.AddFailure(nameof(command.InterestRateDto.InterestRate),
                                    "Ставка процента должна быть больше или равна 0.");
                            }
                            break;
                    }
                });
        }
    }
}
