using FluentValidation;
using System.Text.Json;

namespace BankAccounts.Middlewares
{
    /// <summary>
    /// Middleware, глобально перехватывающее и обрабатывающее исключения в приложении.
    /// Преобразует исключения в структурированный JSON-ответ в формате <c>ProblemDetails</c>.
    /// </summary>
    public class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// Создает экземпляр <see cref="ExceptionHandlingMiddleware"/>.
        /// </summary>
        /// <param name="logger">Логгер для записи ошибок.</param>
        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Метод, вызываемый при обработке HTTP-запроса. Выполняет перехват исключений.
        /// </summary>
        /// <param name="context">Контекст HTTP-запроса.</param>
        /// <param name="next">Следующий делегат запроса в конвейере.</param>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");

                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Обрабатывает исключение и формирует JSON-ответ с деталями.
        /// </summary>
        /// <param name="context">Контекст HTTP-запроса.</param>
        /// <param name="exception">Перехваченное исключение.</param>
        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = GetStatusCode(exception);

            var problemDetails = new
            {
                title = GetTitle(exception),
                status = statusCode,
                detail = GetDetail(exception),
                errors = GetErrors(exception)
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }

        /// <summary>
        /// Возвращает соответствующий HTTP статус-код для заданного исключения.
        /// </summary>
        /// <param name="exception">Объект исключения.</param>
        /// <returns>HTTP статус-код.</returns>
        private static int GetStatusCode(Exception exception) =>
            exception switch
            {
                BadHttpRequestException => StatusCodes.Status400BadRequest,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                ValidationException => StatusCodes.Status422UnprocessableEntity,
                _ => StatusCodes.Status500InternalServerError
            };

        /// <summary>
        /// Возвращает краткий заголовок ошибки на основе типа исключения.
        /// </summary>
        /// <param name="exception">Объект исключения.</param>
        /// <returns>Строка с заголовком ошибки.</returns>
        private static string GetTitle(Exception exception) =>
            exception switch
            {
                BadHttpRequestException => "Bad Request",
                KeyNotFoundException => "Not Found",
                ValidationException => "Validation Failed",
                _ => "Internal Server Error"
            };

        /// <summary>
        /// Возвращает описание ошибки на основе типа исключения.
        /// </summary>
        /// <param name="exception">Объект исключения.</param>
        /// <returns>Строка с описанием ошибки.</returns>
        private static string GetDetail(Exception exception) =>
            exception switch
            {
                ValidationException => "Один или несколько параметров запроса не прошли валидацию.",
                _ => exception.Message
            };

        /// <summary>
        /// Возвращает список ошибок валидации, сгруппированных по свойствам модели, если исключение связано с валидацией.
        /// </summary>
        /// <param name="exception">Объект исключения.</param>
        /// <returns>
        /// Словарь, где ключ - это имя свойства, а значение - массив строк с ошибками валидации.
        /// </returns>
        private static IReadOnlyDictionary<string, string[]> GetErrors(Exception exception)
        {
            IReadOnlyDictionary<string, string[]> errors = new Dictionary<string, string[]>();

            if (exception is ValidationException validationException)
            {
                errors = validationException.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Select(e => e.ErrorMessage)
                            .Distinct().ToArray());
            }

            return errors;
        }
    }
}
