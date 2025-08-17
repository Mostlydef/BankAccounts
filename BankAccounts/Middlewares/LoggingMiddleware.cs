using System.Diagnostics;
using System.Text;

namespace BankAccounts.Middlewares
{
    /// <summary>
    /// Middleware для логирования HTTP-запросов и их метрик.
    /// </summary>
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        /// <summary>
        /// Конструктор middleware.
        /// </summary>
        /// <param name="next">Следующий middleware в конвейере.</param>
        /// <param name="logger">Логгер для записи информации о запросах.</param>
        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Асинхронный метод обработки HTTP-запроса.
        /// Логирует метод, путь, статус, время обработки и тело запроса.
        /// </summary>
        /// <param name="context">Контекст HTTP-запроса.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;

            string body = "";

            // Чтение тела запроса, если оно присутствует и поток можно перемещать
            if (request.ContentLength > 0 && request.Body.CanSeek)
            {
                request.Body.Position = 0;
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }

            var stopwatch = Stopwatch.StartNew();
            await _next(context);
            stopwatch.Stop();

            // Логируем детали запроса и время обработки
            _logger.LogInformation("HTTP request {@HttpRequestLog}", new
            {
                request.Method,
                request.Path,
                QueryString = request.QueryString.ToString(),
                context.Response.StatusCode,
                LatencyMs = stopwatch.ElapsedMilliseconds,
                RequestBody = body
            });
        }
    }

}
