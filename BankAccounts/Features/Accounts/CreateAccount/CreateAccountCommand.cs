using BankAccounts.Abstractions.CQRS;
using BankAccounts.Common.Results;
using BankAccounts.Features.Accounts.DTOs;

namespace BankAccounts.Features.Accounts.CreateAccount
{
    /// <summary>
    /// Команда для создания нового аккаунта.
    /// </summary>
    /// <param name="CreateDto">Данные для создания аккаунта.</param>
    public record CreateAccountCommand(
        AccountCreateDto CreateDto) : ICommand<MbResult<AccountDto>>;
}
