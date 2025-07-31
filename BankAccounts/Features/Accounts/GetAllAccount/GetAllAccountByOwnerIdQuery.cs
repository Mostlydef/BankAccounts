using BankAccounts.Abstractions.CQRS;
using BankAccounts.Common.Results;
using BankAccounts.Features.Accounts.DTOs;

namespace BankAccounts.Features.Accounts.GetAllAccount
{
    /// <summary>
    /// Запрос для получения всех счетов по идентификатору владельца.
    /// </summary>
    /// <param name="OwnerId">Идентификатор владельца счетов.</param>
    public record GetAllAccountByOwnerIdQuery(Guid OwnerId) : IQuery<MbResult<List<AccountDto>>>;
}
