using JetBrains.Annotations;

namespace BankAccounts.Common.Results
{
    /// <summary>
    /// Обобщённый результат выполнения операции. Содержит либо успешный результат, либо информацию об ошибке.
    /// </summary>
    /// <typeparam name="T">Тип значения, возвращаемого при успешном выполнении.</typeparam>
    public class MbResult <T> 
    {
        /// <summary>
        /// Показывает, была ли операция успешной.
        /// Аннотация <c>[UsedImplicitly]</c>  подавляет предупреждение об необходимости изменения set -> init, так как данное свойство используется косвенно в тестах.
        /// </summary>
        [UsedImplicitly]
        public bool IsSuccess { get; set; }
        /// <summary>
        /// Значение, возвращаемое при успешной операции.
        /// Аннотация <c>[UsedImplicitly]</c>  подавляет предупреждение об необходимости изменения set -> init, так как данное свойство используется косвенно в тестах. 
        /// </summary>
        [UsedImplicitly]
        public T? Value { get; set; }
        /// <summary>
        /// Информация об ошибке, если операция завершилась неудачей.
        /// Аннотация <c>[UsedImplicitly]</c>  подавляет предупреждение об необходимости изменения set -> init, так как данное свойство используется косвенно в тестах. 
        /// </summary>
        [UsedImplicitly]
        public MbError? Error { get; set; }

        /// <summary>
        /// Создаёт успешный результат с переданным значением.
        /// </summary>
        /// <param name="value">Значение результата.</param>
        /// <returns>Объект <see cref="MbResult{T}"/> с успешным результатом.</returns>
        public static MbResult<T> Success(T value)
        {
            return new MbResult<T>
            {
                IsSuccess = true,
                Value = value
            };
        }

        /// <summary>
        /// Создаёт результат с ошибкой, не связанной с валидацией.
        /// </summary>
        /// <param name="title">Краткое описание ошибки.</param>
        /// <param name="detail">Подробное описание ошибки.</param>
        /// <param name="code">HTTP-код ошибки. По умолчанию 400.</param>
        /// <returns>Объект <see cref="MbResult{T}"/> с ошибкой.</returns>
        private static MbResult<T> Failure(string title, string detail, int code = 400)
        {
            return new MbResult<T>()
            {
                IsSuccess = false,
                Error = new MbError()
                {
                    Title = title,
                    Code = code,
                    Detail = detail
                }
            };
        }

        /// <summary>
        /// Создаёт результат ошибки валидации с подробной информацией по каждому полю.
        /// </summary>
        /// <param name="errors">Словарь ошибок, где ключ имя поля, а значение список сообщений об ошибках.</param>
        /// <param name="code">HTTP-код ошибки. По умолчанию 400.</param>
        /// <returns>Объект <see cref="MbResult{T}"/> с ошибками валидации.</returns>
        public static MbResult<T> ValidatorError(Dictionary<string, string[]> errors, int code = 400)
        {
            return new MbResult<T>()
            {
                IsSuccess = false,
                Error = new MbError()
                {
                    Title = "Validation failed.",
                    Code = code,
                    Detail = "Один или несколько параметров не прошли валидацию.",
                    Errors = errors
                }
            };
        }

        /// <summary>
        /// Возвращает результат ошибки "Не найдено" (404 Not Found).
        /// </summary>
        /// <param name="detail">Подробности ошибки. По умолчанию: "Ресурс не найден."</param>
        /// <returns>Объект <see cref="MbResult{T}"/> с кодом ошибки 404.</returns>
        public static MbResult<T> NotFound(string detail = "Ресурс не найден.")
        {
            return Failure("Not Found", detail, StatusCodes.Status404NotFound);
        }

        /// <summary>
        /// Возвращает результат ошибки "Неверный запрос" (400 Bad Request).
        /// </summary>
        /// <param name="detail">Подробности ошибки. По умолчанию: "Неверный запрос."</param>
        /// <returns>Объект <see cref="MbResult{T}"/> с кодом ошибки 400.</returns>
        public static MbResult<T> BadRequest(string detail = "Неверный запрос.")
        {
            return Failure("Bad Request", detail);
        }

        /// <summary>
        /// Создает результат с кодом состояния 409 Conflict и заданным сообщением детали.
        /// </summary>
        /// <param name="detail">Описание причины конфликта. По умолчанию: "Неверный запрос."</param>
        /// <returns>Экземпляр <see cref="MbResult{T}"/>, представляющий ошибку конфликта.</returns>
        public static MbResult<T> Conflict(string detail = "Неверный запрос.")
        {
            return Failure("Conflict", detail, StatusCodes.Status409Conflict);
        }
    }
}
