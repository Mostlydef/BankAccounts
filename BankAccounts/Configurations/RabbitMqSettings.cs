namespace BankAccounts.Configurations
{
    /// <summary>
    /// Настройки подключения к RabbitMQ.
    /// </summary>
    public class RabbitMqSettings
    {
        /// <summary>
        /// Адрес хоста RabbitMQ.
        /// По умолчанию <c>localhost</c>.
        /// </summary>
        public string HostName { get; set; } = "localhost";
        /// <summary>
        /// Имя пользователя для подключения к RabbitMQ.
        /// По умолчанию <c>admin</c>.
        /// </summary>
        public string UserName { get; set; } = "admin";
        /// <summary>
        /// Пароль пользователя для подключения к RabbitMQ.
        /// По умолчанию <c>admin</c>.
        /// </summary>
        public string Password { get; set; } = "admin";
        /// <summary>
        /// Имя обменника (exchange) RabbitMQ для публикации событий.
        /// По умолчанию <c>account.events</c>.
        /// </summary>
        public string ExchangeName { get; set; } = "account.events";
    }
}
