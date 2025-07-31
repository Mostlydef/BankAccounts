using BankAccounts.Abstractions.CQRS;
using BankAccounts.Common.Results;
using BankAccounts.Features.Accounts.DTOs;

namespace BankAccounts.Features.Accounts.UpdateAccountInterestRate
{
    /// <summary>
    /// Команда для обновления процентной ставки по счету.
    /// </summary>
    /// <param name="AccountId">Идентификатор счета, для которого обновляется процентная ставка.</param>
    /// <param name="InterestRateDto">DTO с новым значением процентной ставки.</param>
    public record UpdateInterestRateCommand(Guid AccountId, AccountInterestRateDto InterestRateDto) : ICommand<MbResult<bool>>;
}
