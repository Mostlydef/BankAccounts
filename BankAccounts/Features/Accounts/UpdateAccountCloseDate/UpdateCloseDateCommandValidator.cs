using BankAccounts.Database.Interfaces;
using FluentValidation;
using JetBrains.Annotations;

namespace BankAccounts.Features.Accounts.UpdateAccountCloseDate
{
    /// <summary>
    /// Валидатор для команды обновления даты закрытия счета.
    /// </summary>
    [UsedImplicitly]
    public class UpdateCloseDateCommandValidator : AbstractValidator<UpdateCloseDateCommand>
    {
        /// <summary>
        /// Инициализирует новый экземпляр <see cref="UpdateCloseDateCommandValidator"/>.
        /// </summary>
        /// <param name="repository">Репозиторий для работы со счетами.</param>
        public UpdateCloseDateCommandValidator(IAccountRepository repository)
        {
            // Проверка на существование счета
            RuleFor(x => x.AccountId)
                .MustAsync(async (accountId, ct) =>
                {
                    var account = await repository.GetByIdAsync(accountId, ct);
                    return account != null;
                })
                .WithMessage("Аккаунт с указанным идентификатором не найден.");

            // Проверка указанной даты
            RuleFor(x => x.CloseDateDto.CloseDate)
                .MustAsync(async (command, closeDate, ct) =>
                {
                    var account = await repository.GetByIdAsync(command.AccountId, ct);
                    return account != null && closeDate > account.OpenDate;
                })
                .WithMessage("Дата закрытия должна быть позже даты открытия.");
        }
    }
}
