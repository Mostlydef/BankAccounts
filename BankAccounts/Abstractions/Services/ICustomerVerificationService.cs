namespace BankAccounts.Abstractions.Services
{
    /// <summary>
    /// Сервис для проверки существования владельца счета.
    /// </summary>
    public interface ICustomerVerificationService
    {
        /// <summary>
        /// Проверяет, существует ли владелец с указанным идентификатором.
        /// </summary>
        /// <param name="ownerId">Идентификатор владельца.</param>
        /// <param name="cancellation">Токен отмены операции.</param>
        /// <returns>Возвращает <c>true</c>, если владелец существует, иначе <c>false</c>.</returns>
        public Task<bool> OwnerExistsAsync(Guid ownerId, CancellationToken cancellation);
    }
}
