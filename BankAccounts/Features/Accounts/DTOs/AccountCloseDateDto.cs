using JetBrains.Annotations;

namespace BankAccounts.Features.Accounts.DTOs
{
    /// <summary>
    /// DTO для обновления даты закрытия счета.
    /// </summary>
    public class AccountCloseDateDto
    {
        /// <summary>
        /// Дата закрытия счета.
        /// </summary>
        public DateTime CloseDate { get; [UsedImplicitly] set; }
    }
}
