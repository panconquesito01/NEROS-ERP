using Neros.Messaging.Abstractions;
using RabbitMQ.Client;

namespace Neros.Messaging.RabbitMQ;

public sealed class BusEventosRabbitMq : IEventBus, IEventBusReceptor, IAsyncDisposable
{
    private readonly HashSet<string> _colasAseguradas = new(StringComparer.Ordinal);
    private readonly OpcionesRabbitMq _opciones;
    private readonly ConnectionFactory _fabrica;
    private IConnection? _conexion;
    private IChannel? _canal;

    public BusEventosRabbitMq(OpcionesRabbitMq opciones)
    {
        _opciones = opciones ?? throw new ArgumentNullException(nameof(opciones));
        _fabrica = new ConnectionFactory
        {
            HostName = opciones.Host,
            Port = opciones.Puerto,
            UserName = opciones.Usuario,
            Password = opciones.Clave
        };
    }

    public async Task PublishAsync(IntegrationEnvelope message, CancellationToken cancellationToken)
    {
        await AsegurarCanalAsync(cancellationToken);
        var cuerpo = IntegrationEnvelopeSerde.Serializar(message);
        var propiedades = new BasicProperties
        {
            ContentType = "application/json",
            MessageId = message.MessageId.ToString(),
            CorrelationId = message.CorrelationId.ToString(),
            Type = message.Type,
            DeliveryMode = DeliveryModes.Persistent
        };
        await _canal!.BasicPublishAsync(_opciones.Exchange, message.Type, false, propiedades, cuerpo, cancellationToken);
    }

    public async Task AsegurarColaAsync(string cola, string routingKey, CancellationToken cancellationToken)
    {
        var clave = cola + "|" + routingKey;
        if (!_colasAseguradas.Add(clave)) return;
        await AsegurarCanalAsync(cancellationToken);
        await _canal!.QueueDeclareAsync(cola, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await _canal.QueueBindAsync(cola, _opciones.Exchange, routingKey, cancellationToken: cancellationToken);
    }

    public async Task<bool> IntentarRecibirAsync(
        string cola,
        Func<IntegrationEnvelope, CancellationToken, Task> manejador,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(manejador);
        await AsegurarCanalAsync(cancellationToken);
        var resultado = await _canal!.BasicGetAsync(cola, autoAck: false, cancellationToken);
        if (resultado is null) return false;
        try
        {
            var mensaje = IntegrationEnvelopeSerde.Deserializar(resultado.Body.ToArray());
            await manejador(mensaje, cancellationToken);
            await _canal.BasicAckAsync(resultado.DeliveryTag, false, cancellationToken);
            return true;
        }
        catch
        {
            await _canal.BasicNackAsync(resultado.DeliveryTag, false, requeue: true, cancellationToken);
            throw;
        }
    }

    private async Task AsegurarCanalAsync(CancellationToken cancellationToken)
    {
        if (_canal is not null) return;
        _conexion ??= await _fabrica.CreateConnectionAsync(cancellationToken);
        _canal = await _conexion.CreateChannelAsync(cancellationToken: cancellationToken);
        await _canal.ExchangeDeclareAsync(_opciones.Exchange, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_canal is not null) await _canal.CloseAsync();
        if (_conexion is not null) await _conexion.CloseAsync();
        _canal?.Dispose();
        _conexion?.Dispose();
    }
}
