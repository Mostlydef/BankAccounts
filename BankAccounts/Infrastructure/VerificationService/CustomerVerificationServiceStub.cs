using BankAccounts.Abstractions.Services;

namespace BankAccounts.Infrastructure.VerificationService
{
    /// <summary>
    /// Заглушка сервиса проверки существования владельца счёта.
    /// Используется для имитации проверки без обращения к реальному хранилищу данных.
    /// </summary>
    public class CustomerVerificationServiceStub : ICustomerVerificationService
    {
        /// <summary>
        /// Проверяет, существует ли владелец счёта.
        /// </summary>
        /// <param name="ownerId">Идентификатор владельца счёта.</param>
        /// <param name="cancellation">Токен отмены операции.</param>
        /// <returns>
        /// Возвращает <c>true</c>, если <paramref name="ownerId"/> не равен <see cref="Guid.Empty"/>, иначе <c>false</c>.
        /// </returns>
        public Task<bool> OwnerExistsAsync(Guid ownerId, CancellationToken cancellation)
        {
            _ = cancellation;
            return Task.FromResult(ownerId != Guid.Empty);
        }
    }
}
