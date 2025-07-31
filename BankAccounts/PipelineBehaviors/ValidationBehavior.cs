using BankAccounts.Abstractions.CQRS;
using BankAccounts.Common.Results;
using FluentValidation;
using MediatR;
using System.Reflection;

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
                var error = failures
                    .GroupBy(f => f.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(f => f.ErrorMessage).ToArray()
                    );

                //  Проверяем, что возвращаемый тип является generic и MbResult
                if (typeof(TResponse).IsGenericType &&
                    typeof(TResponse).GetGenericTypeDefinition() == typeof(MbResult<>))
                {
                    // Получаем тип, который используется в generic
                    var innerType = typeof(TResponse).GetGenericArguments()[0];

                    // Получаем тип MbResult<T> с подставленным T
                    var resultType = typeof(MbResult<>).MakeGenericType(innerType);

                    // Ищем метод с указным именем и флагами
                    var method = resultType
                        .GetMethod(nameof(MbResult<object>.ValidatorError), BindingFlags.Public | BindingFlags.Static);

                    // Если такой метод не найден выбрасываем ошибку
                    if (method == null)
                    {
                        throw new InvalidOperationException("Метод ValidatorError не найден.");
                    }

                    // Получаем статический объект типа MbResult<T>.ValidatorError
                    var result = method.Invoke(null, new object[] { error, StatusCodes.Status422UnprocessableEntity});

                    return (TResponse)result!;
                }


                throw new ValidationException("Валидация не поддерживается для типа: " + typeof(TResponse).Name);
            }

            return await next(cancellationToken);
        }
    }
}
