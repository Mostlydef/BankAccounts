using BankAccounts.Configurations;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;

namespace BankAccounts.Infrastructure.Rabbit.PublishEvents
{
    public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
    {
        private IConnection? _connection;
        private IChannel _channel;
        private readonly string _exchangeName;
        private readonly RabbitMqSettings _settings;

        public RabbitMqPublisher(IOptions<RabbitMqSettings> options)
        {
            _settings = options.Value;
            _exchangeName = _settings.ExchangeName;
        }

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

        public async Task PublishRaw(string routingKey, string payloadJson, string? correlationId, string? causationId, string? messageId)
        {
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

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
