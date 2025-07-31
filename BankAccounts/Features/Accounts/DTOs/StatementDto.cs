using BankAccounts.Features.Transactions.DTOs;
using JetBrains.Annotations;

namespace BankAccounts.Features.Accounts.DTOs
{
    /// <summary>
    /// Представляет выписку по счету с транзакциями за определённый период.
    /// </summary>
    public class StatementDto
    {
        /// <summary>
        /// Уникальный идентификатор счета.
        /// </summary>
        public Guid Id { [UsedImplicitly] get; set; }
        /// <summary>
        /// Уникальный идентификатор владельца счета.
        /// </summary>
        public Guid OwnerId { [UsedImplicitly] get; set; }
        /// <summary>
        /// Список транзакций, входящих в выписку.
        /// </summary>
        public required List<TransactionDto> TransactionsDto { [UsedImplicitly] get; set; } 
    }
}
