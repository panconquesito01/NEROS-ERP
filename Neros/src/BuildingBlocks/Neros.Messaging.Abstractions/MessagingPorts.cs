namespace Neros.Messaging.Abstractions;

public interface IEventBus
{
    Task PublishAsync(IntegrationEnvelope message, CancellationToken cancellationToken);
}

public interface IOutbox
{
    Task AddAsync(IntegrationEnvelope message, CancellationToken cancellationToken);
}

public interface IInbox
{
    Task<bool> TryRegisterAsync(string consumer, Guid messageId, CancellationToken cancellationToken);
}