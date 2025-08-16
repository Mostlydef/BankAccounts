using BankAccounts.Configurations;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;

namespace BankAccounts.Infrastructure.Rabbit.PublishEvents
{
    /// <summary>
    /// Реализация <see cref="IRabbitMqPublisher"/> для публикации сообщений в RabbitMQ.
    /// </summary>
    public class RabbitMqPublisher (IOptions<RabbitMqSettings> options) : IRabbitMqPublisher
    {
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly string _exchangeName = options.Value.ExchangeName;
        private readonly RabbitMqSettings _settings = options.Value;



        /// <summary>
        /// Асинхронно создает подключение и канал к RabbitMQ.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password,
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Публикует "сырой" JSON-сообщение в RabbitMQ с указанным routing key и заголовками.
        /// </summary>
        /// <param name="routingKey">Ключ маршрутизации для RabbitMQ.</param>
        /// <param name="payloadJson">JSON-содержимое сообщения.</param>
        /// <param name="correlationId">Идентификатор корреляции (опционально).</param>
        /// <param name="causationId">Идентификатор причинного события (опционально).</param>
        /// <param name="messageId">Идентификатор сообщения (опционально).</param>
        /// <returns>Асинхронная задача публикации.</returns>
        public async Task PublishRaw(string routingKey, string payloadJson, string? correlationId, string? causationId, string? messageId)
        {
            if (_channel == null)
                throw new InvalidOperationException("RabbitMQ channel is not initialized. Call StartAsync before publishing messages.");

            var body = Encoding.UTF8.GetBytes(payloadJson);
            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent, // Сообщение будет сохранено на диске
                MessageId = messageId,
                Headers = new Dictionary<string, object?>()
            };

            if (!string.IsNullOrWhiteSpace(correlationId))
                props.Headers["X-Correlation-Id"] = correlationId;
            if (!string.IsNullOrWhiteSpace(causationId))
                props.Headers["X-Causation-Id"] = causationId;

            await _channel.BasicPublishAsync(_exchangeName, routingKey, mandatory: false, props, body);
        }

        /// <summary>
        /// Освобождает ресурсы: канал и подключение к RabbitMQ.
        /// </summary>
        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
