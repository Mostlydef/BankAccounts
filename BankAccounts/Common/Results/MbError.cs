namespace BankAccounts.Common.Results
{
    /// <summary>
    /// Представляет информацию об ошибке, произошедшей при выполнении запроса.
    /// Используется в обертке <see cref="MbResult{T}"/> для возврата ошибок из REST API.
    /// </summary>
    public class MbError
    {
        /// <summary>
        /// Краткое описание ошибки.
        /// </summary>
        public string? Title { get; set; }
        /// <summary>
        /// Код ошибки.
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// Подробное сообщение об ошибке, объясняющее причину.
        /// </summary>
        public required string Detail { get; set; }
        /// <summary>
        /// Коллекция ошибок валидации, где ключ имя свойства, а значение список сообщений об ошибках.
        /// </summary>
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
