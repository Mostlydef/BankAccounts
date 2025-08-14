using JetBrains.Annotations;

namespace BankAccounts.Features.Accounts.DTOs
{
    /// <summary>
    /// DTO для создания нового счета.
    /// </summary>
    public class AccountCreateDto
    {
        /// <summary>
        /// Идентификатор владельца счета.
        /// </summary>
        public Guid OwnerId { get; [UsedImplicitly] set; }
        /// <summary>
        /// Тип счета. Например, "Checking", "Deposit" или "Credit".
        /// </summary>
        public required string AccountType { get; [UsedImplicitly] set; }

        /// <summary>
        /// Валюта счета в формате трехбуквенного кода ISO.
        /// </summary>
        public required string Currency { get; [UsedImplicitly] set; }

        /// <summary>
        /// Процентная ставка по счету, если применимо (например, для кредитных счетов).
        /// Атрибут <see cref="UsedImplicitlyAttribute"/> указывает что это свойство используется косвенно через сериализацию в тестах.
        /// </summary>
        [UsedImplicitly]
        public decimal? InterestRate { get; set; }
    }
}
