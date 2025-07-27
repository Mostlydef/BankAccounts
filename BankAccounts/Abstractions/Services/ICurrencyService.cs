namespace BankAccounts.Abstractions.Services
{
    /// <summary>
    /// Сервис для проверки поддержки валюты.
    /// </summary>
    public interface ICurrencyService
    {
        /// <summary>
        /// Проверяет, поддерживается ли указанная валюта.
        /// </summary>
        /// <param name="currencyCode">Трёхбуквенный код валюты в формате ISO.</param>
        /// <returns>Возвращает <c>true</c>, если валюта поддерживается, иначе <c>false</c>.</returns>
        public bool IsSupported(string currencyCode);
    }
}
