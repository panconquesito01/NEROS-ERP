using System.Text.Json;
using Microsoft.Data.SqlClient;
using Neros.Domain.Contabilidad;
using Neros.Domain.Globalizacion;
using Neros.Domain.Integracion;

namespace Neros.Messaging.Sql;

public sealed class EscritorContabilidadIntegracionSql(string cadenaConexion) : IEscritorIntegracion
{
    private static readonly JsonSerializerOptions JsonOpciones = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public Task PersistirAsync(
        string consumidor,
        EventoIntegracionEntrada evento,
        IReadOnlyList<EfectoIntegracionProcesado> efectos,
        SqlConnection? conexion = null,
        SqlTransaction? transaccion = null,
        CancellationToken cancellationToken = default)
    {
        if (consumidor != MotorEnrutamientoIntegracion.ConsumidorContabilidad)
            return Task.CompletedTask;
        if (conexion is null || transaccion is null)
            return PersistirConConexionPropiaAsync(consumidor, evento, cancellationToken);
        return PersistirEnTransaccionAsync(evento, conexion, transaccion, cancellationToken);
    }

    private async Task PersistirConConexionPropiaAsync(
        string consumidor, EventoIntegracionEntrada evento, CancellationToken cancellationToken)
    {
        await using var conexion = new SqlConnection(cadenaConexion);
        await conexion.OpenAsync(cancellationToken);
        await using var tx = (SqlTransaction)await conexion.BeginTransactionAsync(cancellationToken);
        await PersistirEnTransaccionAsync(evento, conexion, tx, cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    private async Task PersistirEnTransaccionAsync(
        EventoIntegracionEntrada evento, SqlConnection conexion, SqlTransaction transaccion, CancellationToken cancellationToken)
    {
        var politica = CatalogoIso.PoliticaPorDefecto("COP");
        IReadOnlyList<MotorContabilizacion.LineaRegla>? reglas = evento.Carga switch
        {
            ContabilizacionPorRecepcion recepcion => FlujosIntegracion.RecepcionAReglasContables(recepcion, politica),
            RecepcionCompraIntegracion compra => FlujosIntegracion.RecepcionAReglasContables(
                FlujosIntegracion.RecepcionAContabilizacion(compra), politica),
            ContabilizacionPorNomina nomina => FlujosIntegracion.NominaAReglasContables(nomina, politica),
            _ => null
        };
        if (reglas is null || reglas.Count == 0)
            return;

        var tenantId = evento.TenantId;
        var empresaId = evento.EmpresaId ?? ExtraerEmpresa(evento);
        if (tenantId == Guid.Empty || empresaId == Guid.Empty)
            return;

        var lineasJson = JsonSerializer.Serialize(reglas.Select(r => new { Rol = r.Rol.ToString(), Lado = r.Lado.ToString(), r.Importe }), JsonOpciones);
        var solicitudId = Guid.NewGuid();
        await using (var cmd = conexion.CreateCommand())
        {
            cmd.Transaction = transaccion;
            cmd.CommandText = """
                IF NOT EXISTS (SELECT 1 FROM [contabilidad].[SolicitudContabilizacionIntegracion] WHERE [EventoIntegracionId]=@EventoId)
                INSERT INTO [contabilidad].[SolicitudContabilizacionIntegracion]
                    ([Id],[EventoIntegracionId],[TenantId],[EmpresaId],[OrigenModulo],[OrigenAgregadoId],[TipoEvento],[LineasJson])
                VALUES (@Id,@EventoId,@TenantId,@EmpresaId,@Modulo,@Agregado,@TipoEvento,@Lineas);
                """;
            cmd.Parameters.AddWithValue("@Id", solicitudId);
            cmd.Parameters.AddWithValue("@EventoId", evento.MessageId);
            cmd.Parameters.AddWithValue("@TenantId", tenantId);
            cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
            cmd.Parameters.AddWithValue("@Modulo", ModuloOrigen(evento));
            cmd.Parameters.AddWithValue("@Agregado", evento.AggregateId);
            cmd.Parameters.AddWithValue("@TipoEvento", evento.TipoEvento);
            cmd.Parameters.AddWithValue("@Lineas", lineasJson);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        await ContabilizadorIntegracionAutomaticoSql.IntentarContabilizarAsync(
            conexion, transaccion, tenantId, empresaId, evento.MessageId,
            ModuloOrigen(evento), evento.AggregateId, lineasJson, cancellationToken);
    }

    private static Guid ExtraerEmpresa(EventoIntegracionEntrada evento) => evento.Carga switch
    {
        PedidoConfirmadoIntegracion p => p.EmpresaId,
        RecepcionCompraIntegracion r => r.EmpresaId,
        _ => Guid.Empty
    };

    private static string ModuloOrigen(EventoIntegracionEntrada evento) => evento.Carga switch
    {
        RecepcionCompraIntegracion => "Compras",
        ContabilizacionPorNomina => "Nomina",
        _ => "Integracion"
    };
}
