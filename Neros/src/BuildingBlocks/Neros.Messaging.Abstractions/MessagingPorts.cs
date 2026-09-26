namespace Neros.Messaging.Abstractions;

public interface IEventBus
{
    Task PublishAsync(IntegrationEnvelope message, CancellationToken cancellationToken);
}

public interface IEventBusReceptor
{
    Task AsegurarColaAsync(string cola, string routingKey, CancellationToken cancellationToken);

    Task<bool> IntentarRecibirAsync(
        string cola,
        Func<IntegrationEnvelope, CancellationToken, Task> manejador,
        CancellationToken cancellationToken);
}

public interface IOutbox
{
    Task AddAsync(IntegrationEnvelope message, CancellationToken cancellationToken);
}

public interface IInbox
{
    Task<bool> ExisteAsync(string consumer, Guid messageId, CancellationToken cancellationToken);

    Task<bool> TryRegisterAsync(string consumer, Guid messageId, CancellationToken cancellationToken);
}