using BankAccounts.Abstractions.CQRS;
using BankAccounts.Features.Transactions.DTOs;

namespace BankAccounts.Features.Transactions.CreateTransaction
{
    /// <summary>
    /// Команда для создания простой транзакции (ввод или снятие средств).
    /// </summary>
    /// <param name="TransactionDto">Данные транзакции для создания.</param>
    public record CreateTransactionCommand(TransactionCreateDto TransactionDto) : ICommand<TransactionDto?>;
}
