using BankAccounts.Abstractions.CQRS;
using BankAccounts.Features.Accounts.DTOs;

namespace BankAccounts.Features.Accounts.GetAccount
{
    /// <summary>
    /// Запрос на получение информации о счёте по его идентификатору.
    /// </summary>
    /// <param name="AccountId">Идентификатор счёта.</param>
    public record GetAccountByIdQuery(Guid AccountId) : IQuery<AccountDto?>;
}
