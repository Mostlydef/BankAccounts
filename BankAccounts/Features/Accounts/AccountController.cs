using BankAccounts.Common.Results;
using BankAccounts.Features.Accounts.CreateAccount;
using BankAccounts.Features.Accounts.DeleteAccount;
using BankAccounts.Features.Accounts.DTOs;
using BankAccounts.Features.Accounts.GetAccount;
using BankAccounts.Features.Accounts.GetAllAccount;
using BankAccounts.Features.Accounts.GetStatement;
using BankAccounts.Features.Accounts.UpdateAccountCloseDate;
using BankAccounts.Features.Accounts.UpdateAccountInterestRate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankAccounts.Features.Accounts
{
    /// <summary>
    /// Контроллер для управления банковскими счетами.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Конструктор контроллера счетов.
        /// </summary>
        /// <param name="mediator">Интерфейс MediatR для отправки команд и запросов.</param>
        public AccountsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Создает новый банковский счет.
        /// </summary>
        /// <param name="dto">DTO с данными для создания счета.</param>
        /// <returns>Созданный счет.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(MbResult<AccountDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(MbResult<>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreateAccount([FromBody] AccountCreateDto dto)
        {
            var result = await _mediator.Send(new CreateAccountCommand(dto));

            if (!result.IsSuccess && result.Error != null)
                return StatusCode(result.Error.Code, result);

            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result);
        }

        /// <summary>
        /// Получает банковский счет по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор счета.</param>
        /// <returns>Информация о счете.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MbResult<AccountDto?>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var account = await _mediator.Send(new GetAccountByIdQuery(id));
            return Ok(account);
        }

        /// <summary>
        /// Удаляет банковский счет по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор счета.</param>
        /// <returns>Результат удаления.</returns>
        /// 
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MbResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MbResult<>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DeleteAccount(Guid id)
        {
            return Ok(await _mediator.Send(new DeleteAccountCommand(id)));
        }

        /// <summary>
        /// Обновляет процентную ставку счета.
        /// </summary>
        /// <param name="id">Идентификатор счета.</param>
        /// <param name="dto">DTO с новой процентной ставкой.</param>
        /// <returns>Обновленная информация по ставке.</returns>
        [HttpPatch("{id}/InterestRate")]
        [ProducesResponseType(typeof(MbResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MbResult<>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> PatchAccountInterestRate(Guid id, [FromBody] AccountInterestRateDto dto)
        {
            var result = await _mediator.Send((new UpdateInterestRateCommand(id, dto)));
            return Ok(result);
        }

        /// <summary>
        /// Обновляет дату закрытия счета.
        /// </summary>
        /// <param name="id">Идентификатор счета.</param>
        /// <param name="dto">DTO с новой датой закрытия.</param>
        /// <returns>Обновленная информация по дате закрытия.</returns>
        [HttpPatch("{id}/CloseDate")]
        [ProducesResponseType(typeof(MbResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MbResult<>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> PatchAccountCloseDate(Guid id, [FromBody] AccountCloseDateDto dto)
        {
            var result = await _mediator.Send(new UpdateCloseDateCommand(id, dto));
            return Ok(result);
        }

        /// <summary>
        /// Получает список всех счетов владельца.
        /// </summary>
        /// <param name="ownerId">Идентификатор владельца.</param>
        /// <returns>Список счетов.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(MbResult<List<AccountDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAccounts(Guid ownerId)
        {
            return Ok(await _mediator.Send(new GetAllAccountByOwnerIdQuery(ownerId)));
        }

        /// <summary>
        /// Получает выписку по счету за указанный период.
        /// </summary>
        /// <param name="id">Идентификатор счета.</param>
        /// <param name="from">Начальная дата периода.</param>
        /// <param name="to">Конечная дата периода.</param>
        /// <returns>Выписка по счету.</returns>
        [HttpGet("Statement")]
        [ProducesResponseType(typeof(MbResult<StatementDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatement([FromQuery] Guid id, [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            return Ok(await _mediator.Send(new GetStatementQuery(id, from, to)));
        }
    }
}
