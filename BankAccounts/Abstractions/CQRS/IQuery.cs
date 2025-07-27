using MediatR;

namespace BankAccounts.Abstractions.CQRS
{
    /// <summary>
    /// Маркерный интерфейс запроса (Query) с возвращаемым результатом типа <typeparamref name="TResponse"/>.
    /// Используется в паттерне CQRS и наследует <see cref="IRequest{TResponse}"/> из MediatR.
    /// </summary>
    /// <typeparam name="TResponse">Тип возвращаемого результата запроса.</typeparam>
    public interface IQuery<out TResponse> : IRequest<TResponse>;
}
