namespace BankAccounts.Infrastructure.Rabbit.PublishEvents
{
    /// <summary>
    /// Определяет контракт для публикации событий в систему обмена сообщениями (например, RabbitMQ).
    /// </summary>
    public interface IPublishEvent
    {
        /// <summary>
        /// Публикует событие указанного типа асинхронно.
        /// </summary>
        /// <typeparam name="T">Тип события, которое нужно опубликовать.</typeparam>
        /// <param name="event">Объект события для публикации.</param>
        /// <param name="accountId">Идентификатор аккаунта, к которому относится событие. Используется для маршрутизации или корреляции.</param>
        /// <returns>Задача <see cref="Task"/>, представляющая асинхронную операцию публикации.</returns>
        Task PublishEventAsync<T>(T @event, Guid accountId);
    }
}