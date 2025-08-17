using BankAccounts.Database;
using BankAccounts.Features.Accounts.Events;
using BankAccounts.Features.Transactions.Events;
using BankAccounts.Infrastructure.Rabbit.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;

namespace BankAccounts.Infrastructure.Rabbit.PublishEvents
{
    /// <summary>
    /// Реализует публикацию событий в Outbox для последующей отправки через RabbitMQ.
    /// </summary>
    public class PublishEvent : IPublishEvent
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PublishEvent> _logger;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="PublishEvent"/>.
        /// </summary>
        /// <param name="context">Контекст базы данных приложения.</param>
        /// <param name="logger">Логгер для записи информации о публикации.</param>
        public PublishEvent(AppDbContext context, ILogger<PublishEvent> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Публикует событие <typeparamref name="T"/> в Outbox, формируя метаданные и заголовки для RabbitMQ.
        /// </summary>
        /// <typeparam name="T">Тип события.</typeparam>
        /// <param name="event">Событие для публикации.</param>
        /// <param name="accountId">Идентификатор аккаунта, связанный с событием.</param>
        /// <returns>Асинхронная задача, представляющая публикацию события.</returns>
        public async Task PublishEventAsync<T>(T @event, Guid accountId)
        {
            var stopwatch = Stopwatch.StartNew();
            // Получаем последнее событие для этого аккаунта (по accountId), чтобы использовать correlation и causation
            var lastEvent = await _context.OutboxMessages
                .Where(e => EF.Functions.JsonContains(e.Payload, $"{{\"accountId\": \"{accountId}\"}}"))
                .OrderByDescending(e => e.OccurredAt)
                .FirstOrDefaultAsync();

            // По умолчанию генерируем новые идентификаторы
            Guid correlationId = Guid.NewGuid();
            Guid causationId = Guid.NewGuid();

            // Если есть предыдущее событие, используем его идентификаторы для корреляции
            if (lastEvent != null)
            {
                correlationId = JsonSerializer.Deserialize<EventEnvelope<T>>(lastEvent.Payload)!.Meta.CorrelationId;
                causationId = JsonSerializer.Deserialize<EventEnvelope<T>>(lastEvent.Payload)!.EventId;
            }

            // Формируем "конверт" события с метаданными
            var envelope = new EventEnvelope<T>
            {
                EventId = Guid.NewGuid(),
                OccurredAt = DateTimeOffset.UtcNow.ToString("o"),
                Meta = new EventMeta
                {
                    Version = "v1",
                    Source = "account-service",
                    CorrelationId = correlationId,
                    CausationId = causationId
                },
                Payload = @event
            };

            // Создаём Outbox-сообщение для последующей публикации через RabbitMQ
            var outboxMessage = new OutboxMessage
            {
                Id = envelope.EventId,
                OccurredAt = DateTimeOffset.UtcNow,
                Type = @event!.GetType().Name,
                RoutingKey = GetRoutingKeyForEvent(@event),
                Payload = JsonSerializer.Serialize(envelope),
                Headers = JsonSerializer.Serialize(envelope.Meta),
                Status = nameof(MessageStatus.Pending)
            };
            stopwatch.Stop();
            // Логируем факт публикации в Outbox
            _logger.LogInformation("Published event {@EventLog}", new
            {
                envelope.EventId,
                Type = @event.GetType().Name,
                CorrelationId = correlationId,
                Retry = 0,
                LatencyMs = stopwatch.ElapsedMilliseconds
            });

            // Добавляем сообщение в контекст для сохранения в базу данных
            await _context.OutboxMessages.AddAsync(outboxMessage);
        }

        /// <summary>
        /// Определяет ключ маршрутизации RabbitMQ для конкретного события.
        /// </summary>
        /// <typeparam name="TEvent">Тип события.</typeparam>
        /// <param name="event">Событие для публикации.</param>
        /// <returns>Строка ключа маршрутизации.</returns>
        /// <exception cref="InvalidOperationException">Выбрасывается, если событие неизвестного типа.</exception>
        private string GetRoutingKeyForEvent<TEvent>(TEvent @event)
        {
            return @event switch
            {
                AccountOpenedEvent => "account.opened",
                MoneyCreditedEvent => "money.credited",
                MoneyDebitedEvent => "money.debited",
                TransferCompletedEvent => "money.transfer.completed",
                InterestAccruedEvent => "money.interest.accrued",
                ClientBlockedEvent => "client.blocked",
                ClientUnblockedEvent => "client.unblocked",
                _ => throw new InvalidOperationException("Unknown event type")
            };
        }
    }
}
