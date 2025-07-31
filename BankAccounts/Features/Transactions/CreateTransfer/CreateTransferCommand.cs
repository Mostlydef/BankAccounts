using BankAccounts.Abstractions.CQRS;
using BankAccounts.Features.Transactions.DTOs;

namespace BankAccounts.Features.Transactions.CreateTransfer
{
    /// <summary>
    /// Команда для создания перевода между счетами.
    /// </summary>
    /// <param name="TransactionDto">Данные транзакции для создания перевода.</param>
    public record CreateTransferCommand(
        TransactionCreateDto TransactionDto)
        : ICommand<TransactionDto?>;
}
