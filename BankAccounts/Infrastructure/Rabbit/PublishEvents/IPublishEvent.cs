namespace BankAccounts.Infrastructure.Rabbit.PublishEvents
{
    public interface IPublishEvent
    {
        public Task PublishEventAsync<T>(T @event, Guid accountId);
    }
}
