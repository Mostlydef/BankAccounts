using BankAccounts.Abstractions.CQRS;

namespace BankAccounts.Features.Accounts.DeleteAccount
{
    /// <summary>
    /// Команда для удаления аккаунта по идентификатору.
    /// </summary>
    /// <param name="AccountId">Идентификатор аккаунта для удаления.</param>
    public record DeleteAccountCommand(Guid AccountId) : ICommand<bool>;
}
