namespace BankAccounts.Features.Accounts.DTOs
{
    /// <summary>
    /// DTO для представления информации о счете.
    /// </summary>
    public class AccountDto
    {
        /// <summary>
        /// Уникальный идентификатор счета.
        /// </summary>
        public Guid Id { get; init; }
        /// <summary>
        /// Идентификатор владельца счета.
        /// </summary>
        public Guid OwnerId { get; init; }
        /// <summary>
        /// Тип счета (например, Checking, Deposit, Credit).
        /// </summary>
        public required string Type { get; init; }
        /// <summary>
        /// Валюта счета (трехбуквенный ISO-код).
        /// </summary>
        public required string Currency { get; init; }
        /// <summary>
        /// Текущий баланс счета.
        /// </summary>
        public decimal Balance { get; init; }
        /// <summary>
        /// Процентная ставка по счету (если применимо).
        /// </summary>
        public decimal? InterestRate { get; init; }
        /// <summary>
        /// Дата открытия счета.
        /// </summary>
        public DateTime OpenDate { get; init; }
        /// <summary>
        /// Дата закрытия счета (если счет закрыт).
        /// </summary>
        public DateTime? CloseDate { get; init; }
    }
}
