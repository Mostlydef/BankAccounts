using BankAccounts.Abstractions.CQRS;
using BankAccounts.Common.Results;
using BankAccounts.Database.Interfaces;
using BankAccounts.Features.Transactions.DTOs;

namespace BankAccounts.Features.Accounts.UpdateAccountCloseDate
{
    /// <summary>
    /// Обработчик команды обновления даты закрытия счета.
    /// </summary>
    public class UpdateCloseDateCommandHandler : ICommandHandler<UpdateCloseDateCommand, MbResult<bool>>
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
        public async Task<MbResult<bool>> Handle(UpdateCloseDateCommand request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
            if (account == null)
            {
                return MbResult<bool>.NotFound("Счет не найден.");
            }

            account.CloseDate = request.CloseDateDto.CloseDate;
            await _accountRepository.UpdateAsync(account);
            return MbResult<bool>.Success(true);
        }
    }
}
