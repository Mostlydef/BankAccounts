using BankAccounts;
using BankAccounts.Common.Results;
using BankAccounts.Configurations;
using BankAccounts.Database;
using BankAccounts.Features.Accounts.DTOs;
using BankAccounts.Features.Transactions;
using BankAccounts.Features.Transactions.DTOs;
using BankAccounts.Features.Transactions.Events;
using BankAccounts.Infrastructure.Rabbit.Consumers;
using BankAccounts.Infrastructure.Rabbit.Outbox;
using BankAccounts.Infrastructure.Rabbit.PublishEvents;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Moq;
using RabbitMQ.Client;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace BankAccountsTests
{
    public class RabbitMqTests(WebApplicationFactory<Program> factory) : IAsyncLifetime, IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithDatabase("db")
            .WithUsername("admin")
            .WithPassword("admin")
            .Build();
        private readonly RabbitMqContainer _rabbitContainer = new RabbitMqBuilder()
            .WithUsername("admin")
            .WithPassword("admin")
            .Build();
        private WebApplicationFactory<Program> _factory = factory;
        private HttpClient _client = new();

        public async Task DisposeAsync()
        {
            await _dbContainer.StopAsync();
            await _dbContainer.DisposeAsync();
            await _rabbitContainer.StopAsync();
            await _rabbitContainer.DisposeAsync();
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
            await _rabbitContainer.StartAsync();

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Test");
                    builder.ConfigureServices(services =>
                    {
                        services.RemoveAll<IRabbitMqPublisher>();

                        services.RemoveAll<DbContextOptions<AppDbContext>>();

                        services.AddDbContext<AppDbContext>(options =>
                            options.UseNpgsql(_dbContainer.GetConnectionString()));

                        services.Configure<RabbitMqSettings>(opts =>
                        {
                            opts.HostName = _rabbitContainer.Hostname;
                            opts.UserName = "admin";
                            opts.Password = "admin";
                            opts.ExchangeName = "account.event"; 
                        });

                        services.AddHostedService<OutboxDispatcher>();
                        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
                        services.AddHostedService<RabbitMqBackgroundService>();
                        services.AddScoped<IPublishEvent, PublishEvent>();

                        services.AddSingleton<IConnectionFactory>(sp =>
                        {
                            var settings = sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

                            return new ConnectionFactory
                            {
                                HostName = settings.HostName,
                                UserName = settings.UserName,
                                Password = settings.Password,
                            };
                        });

                        services.AddHostedService<AntifraudConsumer>();
                        services.AddHostedService<AuditConsumer>();

                        services.AddAuthentication("TestScheme")
                            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });

                    });
                });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
        }

        [Fact]
        public async Task OutboxPublishesAfterFailure()
        {
            // Создаём аккаунт и выполняем транзакцию, которая публикует событие
            var ownerId = Guid.NewGuid();
            var account = await CreateAccountAsync(ownerId);

            var transactionDto = new TransactionCreateDto
            {
                AccountId = account,
                Amount = 10,
                Currency = "RUB",
                Type = nameof(TransactionType.Debit),
                Description = "Test"
            };
            //  останавливаем RabbitMQ, чтобы имитировать недоступность
            await _rabbitContainer.StopAsync();

            var response = await _client.PostAsJsonAsync("/Transactions", transactionDto);
            var createdTransaction = await response.Content.ReadFromJsonAsync<MbResult<TransactionDto?>>();
            // : ждём немного, чтобы Outbox пытался публиковать (безуспешно)
            await Task.Delay(2000);

            //  запускаем RabbitMQ обратно
            await _rabbitContainer.StartAsync();

            // ждем, пока Outbox успешно опубликует событие
            await Task.Delay(3000);

            // проверяем, что сообщение появилось в очереди
            var factory = new ConnectionFactory()
            {
                HostName = _rabbitContainer.Hostname,
                UserName = "admin",
                Password = "admin"
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();
            var result =  await channel.BasicGetAsync("account.crm", true);
            Assert.NotNull(result);
            Assert.NotNull(createdTransaction);
            
            var body = System.Text.Encoding.UTF8.GetString(result.Body.ToArray());

            Assert.Contains("account-service", body);
        }

        [Fact]
        public async Task TransferEmitsSingleEvent()
        {
            // Arrange: создаём мок
            var eventPublisherMock = new Mock<IPublishEvent>();

            // Создаём тестовый фабричный клиент с этим мок-сервисом
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Test");
                    builder.ConfigureServices(services =>
                    {
                        services.RemoveAll<IRabbitMqPublisher>();
                        services.RemoveAll<IPublishEvent>();
                        var rabbitMqPublisherMock = new Mock<IRabbitMqPublisher>();
                        // Регистрируем мок вместо реального RabbitMQ
                        services.AddSingleton(eventPublisherMock.Object);
                        services.AddSingleton(rabbitMqPublisherMock.Object);
                        services.AddDbContext<AppDbContext>(options =>
                            options.UseNpgsql(_dbContainer.GetConnectionString()));

                        services.AddAuthentication("TestScheme")
                            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });
                    });
                });

            _client = _factory.CreateClient();

            eventPublisherMock
                .Setup(p => p.PublishEventAsync(It.IsAny<object>(), It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);
            // Создаём два счёта
            var ownerId = Guid.NewGuid();
            var accountFrom = await CreateAccountAsync(ownerId);
            var accountTo = await CreateAccountAsync(ownerId);

            var transactionDto = new TransactionCreateDto()
            {
                AccountId = accountFrom,
                CounterpartyAccountId = accountTo,
                Amount = 10,
                Currency = "RUB",
                Type = "Credit",
                Description = "Test"
            };

            // Act: делаем 50 переводов
            for (int i = 0; i < 50; i++)
            {
                var response = await _client.PostAsJsonAsync("/Transactions/Transfer", transactionDto);
                response.EnsureSuccessStatusCode();
            }

            // Проверяем, что PublishEventAsync вызван ровно 100 раз
            eventPublisherMock.Verify(
                p => p.PublishEventAsync(It.IsAny<TransferCompletedEvent>(), It.IsAny<Guid>()),
                Times.Exactly(100));
        }


        [Fact]
        public async Task ClientBlockedPreventsDebit()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _rabbitContainer.Hostname,
                UserName = "admin",
                Password = "admin"
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync(
                exchange: "account.event",
                type: ExchangeType.Topic,  // или Fanout, если нужен broadcat
                durable: true
            );
            await channel.QueueDeclareAsync(
                queue: "account.antifraud",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            await channel.QueueBindAsync(
                queue: "account.antifraud",
                exchange: "account.event",
                routingKey: "client.*"  // пустой ключ для fanout
            );

            // Arrange: создать два счёта с балансом по 1000
            var ownerId = Guid.NewGuid();
            var accountFrom = await CreateAccountAsync(ownerId);
            var accountTo = await CreateAccountAsync(ownerId);

            var transactionDto = new TransactionCreateDto()
            {
                AccountId = accountFrom,
                CounterpartyAccountId = accountTo,
                Amount = 10,
                Currency = "RUB",
                Type = "Credit",
                Description = "Test"
            };

            // Шлём событие блокировки через Stub
            var freezeResponse = await _client.PatchAsync($"/AccountFrozenStub/{ownerId}/Frozen", null);
            freezeResponse.EnsureSuccessStatusCode();

            // ждем, пока Outbox успешно опубликует событие
            await Task.Delay(30000);

            // Act: пробуем списать деньги
            var response = await _client.PostAsJsonAsync("/Transactions/Transfer", transactionDto);

            // Assert: должно вернуться 409 Conflict
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        private async Task<Guid> CreateAccountAsync(Guid ownerId)
        {

            var accountDto = new AccountCreateDto
            {
                OwnerId = ownerId,
                AccountType = "Checking",
                Currency = "RUB",
                InterestRate = null
            };

            var response = await _client.PostAsJsonAsync("/Accounts", accountDto);
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);
            var result = JsonSerializer.Deserialize<MbResult<AccountDto>>(responseContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result == null || !result.IsSuccess || result.Value == null)
                return Guid.Empty;

            var transactionDto = new TransactionCreateDto()
            {
                AccountId = result.Value.Id,
                Amount = 1000,
                Currency = "RUB",
                Type = nameof(TransactionType.Debit),
                Description = "Test"
            };
            await _client.PostAsJsonAsync("/Transactions", transactionDto);

            return result.Value.Id;
        }
    }
}
