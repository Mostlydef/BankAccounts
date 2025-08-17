using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace BankAccounts.Infrastructure.Rabbit
{
    /// <summary>
    /// HealthCheck для проверки доступности RabbitMQ.
    /// Пытается установить соединение и создать канал.
    /// </summary>
    public class RabbitMqHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;


        /// <summary>
        /// Создаёт новый экземпляр <see cref="RabbitMqHealthCheck"/>.
        /// </summary>
        /// <param name="configuration">Конфигурация приложения для чтения параметров RabbitMQ.</param>
        public RabbitMqHealthCheck(IConfiguration configuration)
        {
            var host = configuration["RabbitMq:HostName"];
            var user = configuration["RabbitMq:UserName"];
            var pass = configuration["RabbitMq:Password"];
            _connectionString = $"amqp://{user}:{pass}@{host}:5672/";
        }

        /// <summary>
        /// Проверяет доступность RabbitMQ, выполняя подключение и открытие канала.
        /// </summary>
        /// <param name="context">Контекст проверки здоровья.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>
        /// <see cref="HealthCheckResult.Healthy"/> если RabbitMQ доступен, 
        /// либо <see cref="HealthCheckResult.Unhealthy"/> в случае ошибки.
        /// </returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var factory = new ConnectionFactory { Uri = new Uri(_connectionString) };
                await using var connection = await factory.CreateConnectionAsync(cancellationToken);
                await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

                return HealthCheckResult.Healthy("RabbitMQ доступен");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("RabbitMQ недоступен", ex);
            }
        }
    }

}
