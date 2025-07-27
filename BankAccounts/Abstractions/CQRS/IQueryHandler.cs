using MediatR;

namespace BankAccounts.Abstractions.CQRS
{
    /// <summary>
    /// Обработчик запроса (Query) для паттерна CQRS.
    /// </summary>
    /// <typeparam name="TQuery">Тип запроса, реализующий <see cref="IQuery{TResponse}"/>.</typeparam>
    /// <typeparam name="TResponse">Тип возвращаемого результата обработки запроса.</typeparam>
    public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>;
}
