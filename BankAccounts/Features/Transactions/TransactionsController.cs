using BankAccounts.Features.Transactions.CreateTransaction;
using BankAccounts.Features.Transactions.CreateTransfer;
using BankAccounts.Features.Transactions.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankAccounts.Features.Transactions
{
    /// <summary>
    /// Контроллер для работы с транзакциями.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TransactionsController"/>.
        /// </summary>
        /// <param name="mediator">Интерфейс медиатора для обработки запросов и команд.</param>
        public TransactionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Регистрирует одну транзакцию (списание или пополнение).
        /// </summary>
        /// <param name="createDto">Данные для создания транзакции.</param>
        /// <returns>Созданная транзакция или ошибка.</returns>
        /// <response code="200">Успешное создание транзакции.</response>
        /// <response code="400">Ошибка валидации данных или бизнес-логики.</response>
        [HttpPost]
        public async Task<IActionResult> RegisterTransaction([FromBody] TransactionCreateDto createDto)
        {
            var result = await _mediator.Send(new CreateTransactionCommand(createDto));
            return Ok(result);
        }

        /// <summary>
        /// Переводит деньги между двумя счетами.
        /// </summary>
        /// <param name="transactionCreateDto">Данные перевода (откуда, куда, сумма, валюта и т.д.).</param>
        /// <returns>Результат перевода (дебет и кредит).</returns>
        /// <response code="200">Перевод выполнен успешно.</response>
        /// <response code="400">Ошибка перевода (например, недостаточно средств, неподдерживаемая валюта).</response>
        [HttpPost("Transfer")]
        public async Task<IActionResult> Trasnfer([FromBody] TransactionCreateDto transactionCreateDto)
        {
            var result = await _mediator.Send(new CreateTransferCommand(transactionCreateDto));
            return Ok(result);
        }
    }
}
