using BankAccounts.Abstractions.CQRS;
using BankAccounts.Features.Accounts.DTOs;

namespace BankAccounts.Features.Accounts.UpdateAccountCloseDate
{
    /// <summary>
    /// Команда для обновления даты закрытия счета.
    /// </summary>
    /// <param name="AccountId">Идентификатор счета, для которого обновляется дата закрытия.</param>
    /// <param name="CloseDateDto">DTO с новой датой закрытия счета.</param>
    public record UpdateCloseDateCommand(Guid AccountId, AccountCloseDateDto CloseDateDto) : ICommand<bool>;
}
