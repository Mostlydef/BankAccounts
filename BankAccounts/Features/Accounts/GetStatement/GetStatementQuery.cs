using BankAccounts.Abstractions.CQRS;
using BankAccounts.Common.Results;
using BankAccounts.Features.Accounts.DTOs;

namespace BankAccounts.Features.Accounts.GetStatement
{
    /// <summary>
    /// Запрос для получения выписки по счету за определённый период.
    /// </summary>
    /// <param name="Id">Идентификатор счёта.</param>
    /// <param name="From">Дата начала периода выписки.</param>
    /// <param name="To">Дата окончания периода выписки.</param>
    public record GetStatementQuery(Guid Id, DateTime From, DateTime To) : IQuery<MbResult<StatementDto?>>;
}
