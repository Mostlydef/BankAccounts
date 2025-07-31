using MediatR;

namespace BankAccounts.Abstractions.CQRS
{
    /// <summary>
    /// Интерфейс обработчика команды (Command Handler) с возвращаемым результатом типа <typeparamref name="TResponse"/>.
    /// Наследует <see cref="IRequestHandler{TCommand, TResponse}"/> из MediatR и предназначен для обработки команд.
    /// </summary>
    /// <typeparam name="TCommand">Тип команды, реализующий интерфейс <see cref="ICommand{TResponse}"/>.</typeparam>
    /// <typeparam name="TResponse">Тип возвращаемого результата обработки команды.</typeparam>
    public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>;
}
