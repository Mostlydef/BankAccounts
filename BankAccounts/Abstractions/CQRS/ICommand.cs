using MediatR;

namespace BankAccounts.Abstractions.CQRS
{
    /// <summary>
    /// Интерфейс команды (Command) с возвращаемым результатом типа <typeparamref name="TResponse"/>.
    /// Реализует <see cref="IRequest{TResponse}"/> из MediatR.
    /// </summary>
    /// <typeparam name="TResponse">Тип результата, возвращаемого при обработке команды.</typeparam>
    public interface ICommand<out TResponse> : IRequest<TResponse>;
}
