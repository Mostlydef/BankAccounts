namespace BankAccounts.Features.Accounts.DTOs
{
    /// <summary>
    /// DTO для передачи информации о процентной ставке по счету.
    /// </summary>
    public class AccountInterestRateDto
    {
        /// <summary>
        /// Процентная ставка по счету.
        /// </summary>
        public decimal InterestRate { get; set; }
    }
}
