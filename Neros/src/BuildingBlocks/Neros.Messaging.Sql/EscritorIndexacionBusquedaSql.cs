using Microsoft.Data.SqlClient;
using Neros.Domain.Busqueda;
using Neros.Domain.Integracion;

namespace Neros.Messaging.Sql;

public sealed class EscritorIndexacionBusquedaSql(string cadenaConexion) : IEscritorIntegracion
{
    public Task PersistirAsync(
        string consumidor,
        EventoIntegracionEntrada evento,
        IReadOnlyList<EfectoIntegracionProcesado> efectos,
        SqlConnection? conexion = null,
        SqlTransaction? transaccion = null,
        CancellationToken cancellationToken = default)
    {
        if (consumidor != MotorEnrutamientoIntegracion.ConsumidorBusqueda)
            return Task.CompletedTask;
        if (conexion is null || transaccion is null)
            return PersistirConConexionPropiaAsync(evento, cancellationToken);
        return PersistirEnTransaccionAsync(evento, conexion, transaccion, cancellationToken);
    }

    private async Task PersistirConConexionPropiaAsync(EventoIntegracionEntrada evento, CancellationToken cancellationToken)
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
        var empresaId = evento.EmpresaId ?? Guid.Empty;
        if (empresaId == Guid.Empty)
            return;

        var fuente = ModuloFuente(evento.TipoEvento);
        var tipoEntidad = evento.TipoEvento switch
        {
            MotorEnrutamientoIntegracion.TipoPedidoConfirmado => TiposEntidadIndexable.PedidoVenta,
            _ => evento.TipoEvento
        };

        var eventoIndexacion = new EventoParaIndexacion(
            evento.MessageId, evento.TenantId, empresaId, fuente, evento.TipoEvento, evento.AggregateId,
            evento.VersionAgregado, DateTimeOffset.UtcNow, evento.CorrelationId, false, evento.PayloadJson);

        var documentoExistente = await CargarDocumentoAsync(conexion, transaccion, evento.TenantId, empresaId, tipoEntidad,
            evento.AggregateId, cancellationToken);
        var checkpoint = await CargarCheckpointAsync(conexion, transaccion, evento.TenantId, empresaId, fuente, cancellationToken);
        var motorVersion = new MotorVersionIndice();
        if (documentoExistente is not null)
            motorVersion.DebeIndexar(tipoEntidad, evento.AggregateId, documentoExistente.VersionIndice);

        var indexadoEn = DateTimeOffset.UtcNow;
        var (resultado, documento, checkpointNuevo, enlaces) = OrquestadorIndexacionBusqueda.Ejecutar(
            new HashSet<Guid>(), motorVersion, checkpoint, documentoExistente, eventoIndexacion, indexadoEn);

        if (resultado is ResultadoIndexacion.DuplicadoIgnorado or ResultadoIndexacion.VersionAntiguaIgnorada)
            return;

        await InsertarEventoIndexacionAsync(conexion, transaccion, eventoIndexacion, cancellationToken);

        if (documento is not null)
            await GuardarDocumentoAsync(conexion, transaccion, documento, documentoExistente?.Id, cancellationToken);

        if (checkpointNuevo is not null)
            await GuardarCheckpointAsync(conexion, transaccion, checkpointNuevo, cancellationToken);

        foreach (var enlace in enlaces)
            await InsertarEnlaceAsync(conexion, transaccion, evento.TenantId, empresaId, enlace, cancellationToken);
    }

    private static string ModuloFuente(string tipoEvento) => tipoEvento switch
    {
        MotorEnrutamientoIntegracion.TipoPedidoConfirmado => "Ventas",
        _ => "Integracion"
    };

    private static async Task<DocumentoIndice?> CargarDocumentoAsync(
        SqlConnection conexion, SqlTransaction transaccion, Guid tenantId, Guid empresaId, string tipoEntidad, string entidadId,
        CancellationToken cancellationToken)
    {
        await using var cmd = conexion.CreateCommand();
        cmd.Transaction = transaccion;
        cmd.CommandText = """
            SELECT [Id],[VersionIndice],[Titulo],[Resumen],[TextoBusqueda],[PermisoRequerido],[OrigenModulo],[CorrelationId],
                   [Activo],[TombstoneEnUtc],[IndexadoEnUtc]
            FROM [busqueda].[DocumentoIndice]
            WHERE [TenantId]=@TenantId AND [EmpresaId]=@EmpresaId AND [TipoEntidad]=@Tipo AND [EntidadId]=@Entidad;
            """;
        cmd.Parameters.AddWithValue("@TenantId", tenantId);
        cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
        cmd.Parameters.AddWithValue("@Tipo", tipoEntidad);
        cmd.Parameters.AddWithValue("@Entidad", entidadId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return new DocumentoIndice(
            reader.GetGuid(0), tenantId, empresaId, tipoEntidad, entidadId, reader.GetInt64(1),
            reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3), reader.GetString(4),
            reader.GetString(5), reader.GetString(6), reader.GetGuid(7), reader.GetBoolean(8),
            reader.IsDBNull(9) ? null : reader.GetDateTime(9), reader.GetDateTime(10));
    }

    private static async Task<CheckpointIngesta?> CargarCheckpointAsync(
        SqlConnection conexion, SqlTransaction transaccion, Guid tenantId, Guid empresaId, string fuente,
        CancellationToken cancellationToken)
    {
        await using var cmd = conexion.CreateCommand();
        cmd.Transaction = transaccion;
        cmd.CommandText = """
            SELECT [UltimoInstanteUtc],[UltimoEventoId] FROM [busqueda].[CheckpointIngesta]
            WHERE [TenantId]=@TenantId AND [EmpresaId]=@EmpresaId AND [FuenteModulo]=@Fuente;
            """;
        cmd.Parameters.AddWithValue("@TenantId", tenantId);
        cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
        cmd.Parameters.AddWithValue("@Fuente", fuente);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return new CheckpointIngesta(tenantId, empresaId, fuente, reader.GetDateTime(0),
            reader.IsDBNull(1) ? null : reader.GetGuid(1));
    }

    private static async Task InsertarEventoIndexacionAsync(
        SqlConnection conexion, SqlTransaction transaccion, EventoParaIndexacion evento, CancellationToken cancellationToken)
    {
        await using var cmd = conexion.CreateCommand();
        cmd.Transaction = transaccion;
        cmd.CommandText = """
            IF NOT EXISTS (SELECT 1 FROM [busqueda].[EventoIndexacion] WHERE [MessageId]=@MessageId)
            INSERT INTO [busqueda].[EventoIndexacion]
                ([Id],[MessageId],[TenantId],[EmpresaId],[FuenteModulo],[TipoEvento],[EntidadId],[VersionAgregado],
                 [OcurrioEnUtc],[CorrelationId],[EsTombstone])
            VALUES (@Id,@MessageId,@TenantId,@EmpresaId,@Fuente,@Tipo,@Entidad,@Version,@Ocurrio,@Correlation,@Tombstone);
            """;
        cmd.Parameters.AddWithValue("@Id", Guid.NewGuid());
        cmd.Parameters.AddWithValue("@MessageId", evento.MessageId);
        cmd.Parameters.AddWithValue("@TenantId", evento.TenantId);
        cmd.Parameters.AddWithValue("@EmpresaId", evento.EmpresaId);
        cmd.Parameters.AddWithValue("@Fuente", evento.FuenteModulo);
        cmd.Parameters.AddWithValue("@Tipo", evento.TipoEvento);
        cmd.Parameters.AddWithValue("@Entidad", evento.EntidadId);
        cmd.Parameters.AddWithValue("@Version", evento.VersionAgregado);
        cmd.Parameters.AddWithValue("@Ocurrio", evento.OcurrioEnUtc.UtcDateTime);
        cmd.Parameters.AddWithValue("@Correlation", evento.CorrelationId);
        cmd.Parameters.AddWithValue("@Tombstone", evento.EsTombstone);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task GuardarDocumentoAsync(
        SqlConnection conexion, SqlTransaction transaccion, DocumentoIndice documento, Guid? idExistente, CancellationToken cancellationToken)
    {
        var id = idExistente ?? documento.Id;
        await using var cmd = conexion.CreateCommand();
        cmd.Transaction = transaccion;
        cmd.CommandText = """
            MERGE [busqueda].[DocumentoIndice] AS t
            USING (SELECT @Id AS Id) AS s ON t.[Id]=s.[Id]
            WHEN MATCHED THEN UPDATE SET
                [VersionIndice]=@Version,[Titulo]=@Titulo,[Resumen]=@Resumen,[TextoBusqueda]=@Texto,
                [PermisoRequerido]=@Permiso,[OrigenModulo]=@Origen,[CorrelationId]=@Correlation,
                [Activo]=@Activo,[TombstoneEnUtc]=@Tombstone,[IndexadoEnUtc]=@Indexado
            WHEN NOT MATCHED THEN INSERT
                ([Id],[TenantId],[EmpresaId],[TipoEntidad],[EntidadId],[VersionIndice],[Titulo],[Resumen],[TextoBusqueda],
                 [PermisoRequerido],[OrigenModulo],[CorrelationId],[Activo],[TombstoneEnUtc],[IndexadoEnUtc])
            VALUES (@Id,@TenantId,@EmpresaId,@Tipo,@Entidad,@Version,@Titulo,@Resumen,@Texto,@Permiso,@Origen,@Correlation,@Activo,@Tombstone,@Indexado);
            """;
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@TenantId", documento.TenantId);
        cmd.Parameters.AddWithValue("@EmpresaId", documento.EmpresaId);
        cmd.Parameters.AddWithValue("@Tipo", documento.TipoEntidad);
        cmd.Parameters.AddWithValue("@Entidad", documento.EntidadId);
        cmd.Parameters.AddWithValue("@Version", documento.VersionIndice);
        cmd.Parameters.AddWithValue("@Titulo", documento.Titulo);
        cmd.Parameters.AddWithValue("@Resumen", (object?)documento.Resumen ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Texto", documento.TextoBusqueda);
        cmd.Parameters.AddWithValue("@Permiso", documento.PermisoRequerido);
        cmd.Parameters.AddWithValue("@Origen", documento.OrigenModulo);
        cmd.Parameters.AddWithValue("@Correlation", documento.CorrelationId);
        cmd.Parameters.AddWithValue("@Activo", documento.Activo);
        cmd.Parameters.AddWithValue("@Tombstone", (object?)documento.TombstoneEnUtc ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Indexado", documento.IndexadoEnUtc.UtcDateTime);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task GuardarCheckpointAsync(
        SqlConnection conexion, SqlTransaction transaccion, CheckpointIngesta checkpoint, CancellationToken cancellationToken)
    {
        await using var cmd = conexion.CreateCommand();
        cmd.Transaction = transaccion;
        cmd.CommandText = """
            MERGE [busqueda].[CheckpointIngesta] AS t
            USING (SELECT @TenantId AS TenantId, @EmpresaId AS EmpresaId, @Fuente AS Fuente) AS s
            ON t.[TenantId]=s.[TenantId] AND t.[EmpresaId]=s.[EmpresaId] AND t.[FuenteModulo]=s.[Fuente]
            WHEN MATCHED THEN UPDATE SET [UltimoInstanteUtc]=@Instante,[UltimoEventoId]=@EventoId,[ActualizadoEnUtc]=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT ([Id],[TenantId],[EmpresaId],[FuenteModulo],[UltimoInstanteUtc],[UltimoEventoId])
                VALUES (@Id,@TenantId,@EmpresaId,@Fuente,@Instante,@EventoId);
            """;
        cmd.Parameters.AddWithValue("@Id", Guid.NewGuid());
        cmd.Parameters.AddWithValue("@TenantId", checkpoint.TenantId);
        cmd.Parameters.AddWithValue("@EmpresaId", checkpoint.EmpresaId);
        cmd.Parameters.AddWithValue("@Fuente", checkpoint.FuenteModulo);
        cmd.Parameters.AddWithValue("@Instante", checkpoint.UltimoInstanteUtc);
        cmd.Parameters.AddWithValue("@EventoId", (object?)checkpoint.UltimoEventoId ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertarEnlaceAsync(
        SqlConnection conexion, SqlTransaction transaccion, Guid tenantId, Guid empresaId, EnlaceVista360 enlace,
        CancellationToken cancellationToken)
    {
        await using var cmd = conexion.CreateCommand();
        cmd.Transaction = transaccion;
        cmd.CommandText = """
            IF NOT EXISTS (SELECT 1 FROM [busqueda].[EnlaceVista360]
                WHERE [DocumentoOrigenId]=@Origen AND [DocumentoRelacionadoId]=@Relacionado AND [TipoRelacion]=@Tipo)
            INSERT INTO [busqueda].[EnlaceVista360]
                ([Id],[TenantId],[EmpresaId],[DocumentoOrigenId],[DocumentoRelacionadoId],[TipoRelacion])
            VALUES (@Id,@TenantId,@EmpresaId,@Origen,@Relacionado,@Tipo);
            """;
        cmd.Parameters.AddWithValue("@Id", enlace.Id);
        cmd.Parameters.AddWithValue("@TenantId", tenantId);
        cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
        cmd.Parameters.AddWithValue("@Origen", enlace.DocumentoOrigenId);
        cmd.Parameters.AddWithValue("@Relacionado", enlace.DocumentoRelacionadoId);
        cmd.Parameters.AddWithValue("@Tipo", enlace.TipoRelacion);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
