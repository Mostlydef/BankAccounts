using AutoMapper;
using BankAccounts.Database.Interfaces;
using BankAccounts.Features.Accounts;
using BankAccounts.Features.Transactions;
using BankAccounts.Features.Transactions.CreateTransaction;
using BankAccounts.Features.Transactions.DTOs;
using BankAccounts.Infrastructure.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace BankAccountsTests
{
    public class CreateTransactionCommandHandlerTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<IAccountRepository> _accountRepositoryMock;
        private readonly Mock<IPublishEvent> _publishEventMock;

        public CreateTransactionCommandHandlerTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AccountMappingProfile());
                cfg.AddProfile(new TransactionMappingProfile());
            }, new LoggerFactory());

            _mapper = config.CreateMapper();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _accountRepositoryMock = new Mock<IAccountRepository>();
            _publishEventMock = new Mock<IPublishEvent>();
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenAccountDoesNotExist()
        {
            // Arrange
            _accountRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), CancellationToken.None))
                .ReturnsAsync((Account?)null);

            _transactionRepositoryMock
                .Setup(r => r.BeginTransactionAsync())
                .ReturnsAsync(new FakeDbContextTransaction());

            var cmd = new CreateTransactionCommand(new TransactionCreateDto()
            {
                AccountId = Guid.NewGuid(), 
                Amount = 100, 
                Type = nameof(TransactionType.Credit),
                Currency = "RUB",
                Description = "Тест",
            });

            var handler = new CreateTransactionCommandHandler(
                _transactionRepositoryMock.Object,
                _accountRepositoryMock.Object,
                _mapper,
                _publishEventMock.Object
            );

            // Act
            var result = await handler.Handle(cmd, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Error);
            Assert.Equal(StatusCodes.Status404NotFound, result.Error.Code);
            Assert.Equal("Счет не найден.", result.Error.Detail);
        }

        [Fact]
        public async Task Handle_ShouldDecreaseBalance_OnCreditTransaction()
        {
            // Arrange
            var account = new Account
            {
                Id = Guid.NewGuid(), 
                Balance = 1000,
                Currency = "RUB"
            };
            _accountRepositoryMock.Setup(r => r.GetByIdAsync(account.Id, CancellationToken.None)).ReturnsAsync(account);

            _transactionRepositoryMock
                .Setup(r => r.BeginTransactionAsync())
                .ReturnsAsync(new FakeDbContextTransaction());

            var handler = new CreateTransactionCommandHandler(
                _transactionRepositoryMock.Object,
                _accountRepositoryMock.Object,
                _mapper,
                _publishEventMock.Object
            );

            var cmd = new CreateTransactionCommand(new TransactionCreateDto
            {
                AccountId = account.Id,
                Amount = 200,
                Type = nameof(TransactionType.Credit),
                Currency = "RUB",
                Description = "Test"
            });

            // Act
            await handler.Handle(cmd, cancellationToken: CancellationToken.None);

            // Assert
            Assert.Equal(800, account.Balance);
        }

        [Fact]
        public async Task Handle_ShouldRollback_WhenBalanceMismatchDetected()
        {
            // Arrange
            var account = new Account
            {
                Id = Guid.NewGuid(), 
                Balance = 500,
                Currency = "RUB"
            };

            _accountRepositoryMock.Setup(r => r.GetByIdAsync(account.Id, CancellationToken.None)).ReturnsAsync(account);

            _transactionRepositoryMock
                .Setup(r => r.BeginTransactionAsync())
                .ReturnsAsync(new FakeDbContextTransaction());

            _transactionRepositoryMock
                .Setup(r => r.RegisterAsync(It.IsAny<Transaction>()))
                .Callback(() =>
                {
                    account.Balance = 999;
                })
                .Returns(Task.CompletedTask);

            var handler = new CreateTransactionCommandHandler(
                _transactionRepositoryMock.Object,
                _accountRepositoryMock.Object,
                _mapper,
                _publishEventMock.Object
            );

            var cmd = new CreateTransactionCommand(new TransactionCreateDto
            {
                AccountId = account.Id,
                Amount = 100, 
                Type = nameof(TransactionType.Debit),
                Currency = "RUB",
                Description = "Test"
            });

            // Act
            var result = await handler.Handle(cmd, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.Error.Code);
        }
    }
}
