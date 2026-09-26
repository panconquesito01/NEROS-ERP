namespace Neros.Domain.Integracion;

/// <summary>Inbox idempotente + orden por agregado + manejo de dominio (fase 24).</summary>
public sealed class ProcesadorInboxIntegracion(OrdenadorVersionAgregado ordenador)
{
    public async Task<(ResultadoProcesamientoIntegracion Resultado, IReadOnlyList<EfectoIntegracionProcesado> Efectos)> ProcesarAsync(
        Func<Guid, CancellationToken, Task<bool>> existeInbox,
        Func<Guid, CancellationToken, Task<bool>> registrarInbox,
        string consumidor,
        EventoIntegracionEntrada evento,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(existeInbox);
        ArgumentNullException.ThrowIfNull(registrarInbox);
        ArgumentNullException.ThrowIfNull(evento);
        if (string.IsNullOrWhiteSpace(consumidor)) throw new ArgumentException("Consumidor requerido.", nameof(consumidor));

        if (await existeInbox(evento.MessageId, cancellationToken))
            return (ResultadoProcesamientoIntegracion.DuplicadoIgnorado, []);

        if (!ordenador.DebeProcesar(evento.AggregateId, evento.VersionAgregado))
            return (ResultadoProcesamientoIntegracion.DesordenIgnorado, []);

        if (!await registrarInbox(evento.MessageId, cancellationToken))
            return (ResultadoProcesamientoIntegracion.DuplicadoIgnorado, []);

        var efectos = ManejadorEventosIntegracion.Manejar(consumidor, evento);
        if (efectos.Count == 0)
            return (ResultadoProcesamientoIntegracion.TipoDesconocido, []);

        return (ResultadoProcesamientoIntegracion.Procesado, efectos);
    }
}
