using BankAccounts.Database.Interfaces;
using FluentValidation;
using JetBrains.Annotations;

namespace BankAccounts.Features.Accounts.DeleteAccount
{
    /// <summary>
    /// Валидатор команды удаления аккаунта.
    /// Проверяет, что идентификатор аккаунта задан и аккаунт существует и не закрыт.
    /// </summary>
    [UsedImplicitly]
    public class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
    {
        /// <summary>
        /// Инициализирует новый экземпляр <see cref="DeleteAccountCommandValidator"/>.
        /// </summary>
        /// <param name="repository">Репозиторий аккаунтов для проверки существования аккаунта.</param>
        public DeleteAccountCommandValidator(IAccountRepository repository)
        {
            // Проверка идентификатора 
            RuleFor(x => x.AccountId)
                .NotEmpty().WithMessage("Идентификатор аккаунта обязателен.")
                .MustAsync(async (accountId, ct) =>
                {
                    var account = await repository.GetByIdAsync(accountId, ct);
                    return account != null && account.CloseDate == null;
                })
                .WithMessage("Аккаунт с таким идентификатором не найден или уже закрыт.");
        }
    }
}
