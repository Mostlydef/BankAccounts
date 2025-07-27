using BankAccounts.Abstractions.Services;

namespace BankAccounts.Infrastructure.CurrencyService
{
    /// <summary>
    /// Заглушка сервиса валют, предоставляющая проверку поддерживаемых валют.
    /// Используется для тестирования и отладки без подключения к внешним API.
    /// </summary>
    public class CurrencyServiceStub : ICurrencyService
    {
        /// <summary>
        /// Заглушка сервиса валют, предоставляющая проверку поддерживаемых валют.
        /// Используется для тестирования и отладки без подключения к внешним API.
        /// </summary>
        private readonly HashSet<string> _supportedCurrencies =
        [
            "USD", "EUR", "RUB", "JPY"
        ];

        /// <summary>
        /// Проверяет, поддерживается ли валюта по её коду.
        /// </summary>
        /// <param name="currencyCode">Код валюты (например, "USD", "EUR").</param>
        /// <returns>
        /// <c>true</c>, если валюта поддерживается; в противном случае — <c>false</c>.
        /// </returns>
        public bool IsSupported(string currencyCode)
        {
            return _supportedCurrencies.Contains(currencyCode);
        }
    }
}
