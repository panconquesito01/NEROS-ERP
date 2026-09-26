using Microsoft.Data.SqlClient;
using Neros.Domain.Integracion;
using Neros.Messaging.Abstractions;

namespace Neros.Messaging.Sql;

/// <summary>Consumidor con inbox SQL, dominio y persistencia en una transaccion (D-03).</summary>
public sealed class ConsumidorIntegracionPersistente(
    string consumidor,
    string cadenaConexion,
    AlmacenInboxSql inbox,
    OrdenadorVersionAgregado ordenador,
    IEscritorIntegracion? escritor = null)
{
    public string Consumidor => consumidor;

    public async Task<(ResultadoProcesamientoIntegracion Resultado, IReadOnlyList<EfectoIntegracionProcesado> Efectos)> ProcesarAsync(
        IntegrationEnvelope sobre,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sobre);
        var evento = DeserializadorEventoIntegracion.DesdeSobre(sobre, consumidor);

        if (await inbox.ExisteAsync(consumidor, evento.MessageId, cancellationToken))
            return (ResultadoProcesamientoIntegracion.DuplicadoIgnorado, []);

        if (!ordenador.DebeProcesar(evento.AggregateId, evento.VersionAgregado))
            return (ResultadoProcesamientoIntegracion.DesordenIgnorado, []);

        await using var conexion = new SqlConnection(cadenaConexion);
        await conexion.OpenAsync(cancellationToken);
        await using var transaccion = (SqlTransaction)await conexion.BeginTransactionAsync(cancellationToken);

        if (!await inbox.TryRegisterAsync(consumidor, evento.MessageId, conexion, transaccion, cancellationToken))
        {
            await transaccion.RollbackAsync(cancellationToken);
            return (ResultadoProcesamientoIntegracion.DuplicadoIgnorado, []);
        }

        var efectos = ManejadorEventosIntegracion.Manejar(consumidor, evento);
        if (efectos.Count == 0)
        {
            await transaccion.RollbackAsync(cancellationToken);
            return (ResultadoProcesamientoIntegracion.TipoDesconocido, []);
        }

        if (escritor is not null)
            await escritor.PersistirAsync(consumidor, evento, efectos, conexion, transaccion, cancellationToken);

        await transaccion.CommitAsync(cancellationToken);
        return (ResultadoProcesamientoIntegracion.Procesado, efectos);
    }
}
