using Hangfire.Dashboard;

namespace BankAccounts.Features.Accounts
{

    /// <summary>
    /// Фильтр авторизации для Hangfire Dashboard, который разрешает доступ всем пользователям.
    /// </summary>
    public class AllowAllAuthorizationFilter : IDashboardAuthorizationFilter
    {
        /// <summary>
        /// Разрешает доступ к Hangfire Dashboard без ограничений.
        /// </summary>
        /// <param name="context">Контекст Dashboard.</param>
        /// <returns>Всегда возвращает <c>true</c>, разрешая доступ.</returns>
        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }
}