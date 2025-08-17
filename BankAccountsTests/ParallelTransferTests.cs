using BankAccounts;
using BankAccounts.Common.Results;
using BankAccounts.Database;
using BankAccounts.Features.Accounts.DTOs;
using BankAccounts.Features.Transactions;
using BankAccounts.Features.Transactions.DTOs;
using BankAccounts.Infrastructure.Rabbit.PublishEvents;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Connections;
using Testcontainers.PostgreSql;
using Moq;
using BankAccounts.Infrastructure.Rabbit.Outbox;

namespace BankAccountsTests
{
    public class ParallelTransferTests(WebApplicationFactory<Program> factory) : IAsyncLifetime, IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithDatabase("db")
            .WithUsername("admin")
            .WithPassword("admin")
            .Build();
        private WebApplicationFactory<Program> _factory = factory;
        private HttpClient _client = new();

        public async Task InitializeAsync()
        { 
            await _dbContainer.StartAsync();

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Test");
                    builder.ConfigureServices(services =>
                    {
                        services.RemoveAll<IRabbitMqPublisher>();
                        services.RemoveAll<IConnectionFactory>();
                        services.RemoveAll<DbContextOptions<AppDbContext>>();

                        services.AddDbContext<AppDbContext>(options =>
                            options.UseNpgsql(_dbContainer.GetConnectionString()));

                        services.AddAuthentication("TestScheme")
                            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });

                        var rabbitPublisherMock = new Mock<IRabbitMqPublisher>();

                        rabbitPublisherMock.Setup(x => x.PublishRaw(It.IsAny<string>(),
                                It.IsAny<OutboxMessage>()))
                            .Returns(Task.CompletedTask);

                        services.AddSingleton(rabbitPublisherMock.Object);

                        var connectionFactoryMock = new Mock<IConnectionFactory>();
                        services.AddSingleton(connectionFactoryMock.Object);
                    });
                });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
        }

        public async Task DisposeAsync()
        {
            await _dbContainer.StopAsync();
            await _dbContainer.DisposeAsync();
        }

        [Fact]
        public async Task ShouldPreserveTotalBalance_AfterParallelTransfers()
        {
            // Arrange: создать два счёта с балансом по 1000
            var ownerId = Guid.NewGuid();
            var accountFrom = await CreateAccountAsync(ownerId);
            var accountTo = await CreateAccountAsync(ownerId);

            var initialTotal = 2000m;

            var transactionDto = new TransactionCreateDto()
            {
                AccountId = accountFrom,
                CounterpartyAccountId = accountTo,
                Amount = 10,
                Currency = "RUB",
                Type = "Credit",
                Description = "Test"
            };

            // Act: запустить 50 параллельных переводов
            var tasks = Enumerable.Range(0, 50).Select(_ =>
                _client.PostAsJsonAsync("/Transactions/Transfer", transactionDto));

            await Task.WhenAll(tasks);

            // Assert: проверить, что сумма не изменилась
            var accounts = await GetAllAccountsAsync(ownerId);
            var finalTotal = accounts.Sum(a => a.Balance);

            Assert.Equal(initialTotal, finalTotal);
            await _dbContainer.StopAsync();
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

        private async Task<List<AccountDto>> GetAllAccountsAsync(Guid ownerId)
        {
            var response = await _client.GetAsync($"/Accounts?ownerId={ownerId}");
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MbResult<List<AccountDto>>>(responseContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result == null || result.Value == null)
                return new List<AccountDto>();

            return result.Value;
        }
    }
}
