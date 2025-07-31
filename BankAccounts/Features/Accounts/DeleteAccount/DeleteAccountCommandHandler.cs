using BankAccounts.Abstractions.CQRS;
using BankAccounts.Database.Interfaces;

namespace BankAccounts.Features.Accounts.DeleteAccount
{
    /// <summary>
    /// Обработчик команды удаления аккаунта.
    /// Выполняет удаление аккаунта по заданному идентификатору.
    /// </summary>
    public class DeleteAccountCommandHandler : ICommandHandler<DeleteAccountCommand, bool>
    {
        private readonly IAccountRepository _accountRepository;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="DeleteAccountCommandHandler"/>.
        /// </summary>
        /// <param name="accountRepository">Репозиторий аккаунтов.</param>
        public DeleteAccountCommandHandler(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        /// <summary>
        /// Обрабатывает команду удаления аккаунта.
        /// </summary>
        /// <param name="request">Команда удаления аккаунта с идентификатором.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Возвращает <see langword="true"/>, если аккаунт успешно удалён, иначе <see langword="false"/>.</returns>
        public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            return await _accountRepository.Delete(request.AccountId);
        }
    }
}
