using BankAccounts.Abstractions.CQRS;
using FluentValidation;
using MediatR;

namespace BankAccounts.PipelineBehaviors
{
    /// <summary>
    /// Поведение пайплайна MediatR, выполняющее валидацию запроса с помощью FluentValidation перед передачей его обработчику.
    /// </summary>
    /// <typeparam name="TRequest">Тип запроса, реализующий <see cref="ICommand{TResponse}"/>.</typeparam>
    /// <typeparam name="TResponse">Тип ответа, возвращаемого обработчиком запроса.</typeparam>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : class, ICommand<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="ValidationBehavior{TRequest, TResponse}"/>.
        /// </summary>
        /// <param name="validators">Коллекция валидаторов для типа запроса.</param>
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        /// <summary>
        /// Выполняет валидацию запроса до передачи его следующему обработчику в цепочке.
        /// </summary>
        /// <param name="request">Запрос, который необходимо обработать.</param>
        /// <param name="next">Делегат, представляющий следующий обработчик в пайплайне.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Результат выполнения запроса.</returns>
        /// <exception cref="ValidationException">Выбрасывается, если валидация не проходит.</exception>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next(cancellationToken);
            }

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(x => x.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(result => result.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }

            return await next(cancellationToken);
        }
    }
}
