namespace BankAccounts.Infrastructure.Messaging
{
    public interface IPublishEvent
    {
        public Task PublishEventAsync<T>(T @event, Guid accountId);
    }
}
