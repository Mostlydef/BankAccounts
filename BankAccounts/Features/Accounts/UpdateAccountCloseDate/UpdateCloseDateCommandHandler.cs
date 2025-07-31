using BankAccounts.Abstractions.CQRS;
using BankAccounts.Database.Interfaces;

namespace BankAccounts.Features.Accounts.UpdateAccountCloseDate
{
    /// <summary>
    /// Обработчик команды обновления даты закрытия счета.
    /// </summary>
    public class UpdateCloseDateCommandHandler : ICommandHandler<UpdateCloseDateCommand, bool>
    {
        private readonly IAccountRepository _accountRepository;

        /// <summary>
        /// Создает новый экземпляр обработчика команды обновления даты закрытия счета.
        /// </summary>
        /// <param name="accountRepository">Репозиторий для работы со счетами.</param>
        public UpdateCloseDateCommandHandler(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        /// <summary>
        /// Обрабатывает команду обновления даты закрытия счета.
        /// </summary>
        /// <param name="request">Команда с данными обновления.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Возвращает true, если обновление выполнено успешно, иначе false.</returns>
        public async Task<bool> Handle(UpdateCloseDateCommand request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
            if (account == null)
            {
                return false;
            }

            account.CloseDate = request.CloseDateDto.CloseDate;
            await _accountRepository.UpdateAsync(account);
            return true;
        }
    }
}
