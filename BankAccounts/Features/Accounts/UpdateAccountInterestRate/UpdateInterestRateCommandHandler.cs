using BankAccounts.Abstractions.CQRS;
using BankAccounts.Database.Interfaces;

namespace BankAccounts.Features.Accounts.UpdateAccountInterestRate
{
    /// <summary>
    /// Обработчик команды обновления процентной ставки по счету.
    /// </summary>
    public class UpdateInterestRateCommandHandler : ICommandHandler<UpdateInterestRateCommand, bool>
    {
        private readonly IAccountRepository _accountRepository;

        /// <summary>
        /// Создает новый экземпляр <see cref="UpdateInterestRateCommandHandler"/>.
        /// </summary>
        /// <param name="accountRepository">Репозиторий для работы со счетами.</param>
        public UpdateInterestRateCommandHandler(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        /// <summary>
        /// Обрабатывает команду обновления процентной ставки.
        /// </summary>
        /// <param name="request">Команда с данными для обновления ставки.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Возвращает <c>true</c>, если обновление выполнено успешно; иначе <c>false</c>.</returns>
        public async Task<bool> Handle(UpdateInterestRateCommand request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
            if (account == null)
            {
                return false;
            } 
            account.InterestRate = request.InterestRateDto.InterestRate;

            await _accountRepository.UpdateAsync(account);
            return true;
        }
    }
}
