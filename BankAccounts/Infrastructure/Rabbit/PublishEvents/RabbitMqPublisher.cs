using BankAccounts.Configurations;
using BankAccounts.Infrastructure.Rabbit.Outbox;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace BankAccounts.Infrastructure.Rabbit.PublishEvents
{
    /// <summary>
    /// Реализация <see cref="IRabbitMqPublisher"/> для публикации сообщений в RabbitMQ.
    /// </summary>
    public class RabbitMqPublisher (IOptions<RabbitMqSettings> options, ILogger<RabbitMqPublisher> logger) : IRabbitMqPublisher
    {
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly string _exchangeName = options.Value.ExchangeName;
        private readonly RabbitMqSettings _settings = options.Value;
        private readonly ILogger<RabbitMqPublisher> _logger = logger;


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
        /// Освобождает ресурсы: канал и подключение к RabbitMQ.
        /// </summary>
        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }

        /// <summary>
        /// Публикует "сырой" JSON-сообщение в RabbitMQ с указанным routing key и заголовками.
        /// </summary>
        /// <param name="routingKey">Ключ маршрутизации для RabbitMQ.</param>
        /// <param name="message">Модель данных OutboxMessage.</param>
        /// <returns>Асинхронная задача публикации.</returns>
        public async Task PublishRaw(string routingKey, OutboxMessage message)
        {
            if (_channel == null)
                throw new InvalidOperationException(
                    "RabbitMQ channel is not initialized. Call StartAsync before publishing messages.");

            // Достаём заголовки
            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(message.Headers);

            if (headers == null)
                throw new InvalidOperationException($"Outbox message {message.Id} has invalid or missing headers.");

            var correlationId = headers.GetValueOrDefault("CorrelationId");
            var causationId = headers.GetValueOrDefault("CausationId");

            // Логируем попытку публикации
            _logger.LogInformation("Publishing outbox message {@EventContext}",
                new
                {
                    EventId = message.Id,
                    EventType = message.Type,
                    CorrelationId = correlationId,
                    CausationId = causationId,
                    Retry = message.Attempts,
                    message.Status
                });

            var body = Encoding.UTF8.GetBytes(message.Payload);
            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent, // Сообщение будет сохранено на диске
                MessageId = message.Id.ToString(),
                Headers = new Dictionary<string, object?>()
            };

            if (!string.IsNullOrWhiteSpace(correlationId))
                props.Headers["X-Correlation-Id"] = correlationId;
            if (!string.IsNullOrWhiteSpace(causationId))
                props.Headers["X-Causation-Id"] = causationId;
            if (!string.IsNullOrWhiteSpace(message.Type))
                props.Headers["X-Event-Type"] = message.Type;
            await _channel.BasicPublishAsync(_exchangeName, routingKey, mandatory: false, props, body);
        }
    }
}
