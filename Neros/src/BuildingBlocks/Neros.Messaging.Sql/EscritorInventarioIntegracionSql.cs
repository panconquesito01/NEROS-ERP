using Microsoft.Data.SqlClient;
using Neros.Domain.Inventario;
using Neros.Domain.Integracion;

namespace Neros.Messaging.Sql;

public sealed class EscritorInventarioIntegracionSql(string cadenaConexion) : IEscritorIntegracion
{
    public Task PersistirAsync(
        string consumidor,
        EventoIntegracionEntrada evento,
        IReadOnlyList<EfectoIntegracionProcesado> efectos,
        SqlConnection? conexion = null,
        SqlTransaction? transaccion = null,
        CancellationToken cancellationToken = default)
    {
        if (conexion is null || transaccion is null)
            return PersistirConConexionPropiaAsync(consumidor, evento, cancellationToken);
        if (consumidor == MotorEnrutamientoIntegracion.ConsumidorInventarioReservas)
            return PersistirReservasAsync(evento, conexion, transaccion, cancellationToken);
        if (consumidor == MotorEnrutamientoIntegracion.ConsumidorInventarioEntradas)
            return PersistirEntradasAsync(evento, conexion, transaccion, cancellationToken);
        return Task.CompletedTask;
    }

    private async Task PersistirConConexionPropiaAsync(
        string consumidor, EventoIntegracionEntrada evento, CancellationToken cancellationToken)
    {
        await using var conexion = new SqlConnection(cadenaConexion);
        await conexion.OpenAsync(cancellationToken);
        await using var tx = (SqlTransaction)await conexion.BeginTransactionAsync(cancellationToken);
        await PersistirAsync(consumidor, evento, [], conexion, tx, cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    private static async Task PersistirReservasAsync(
        EventoIntegracionEntrada evento, SqlConnection conexion, SqlTransaction transaccion, CancellationToken cancellationToken)
    {
        if (evento.Carga is not PedidoConfirmadoIntegracion pedido)
            return;
        var reserva = FlujosIntegracion.PedidoConfirmadoAReserva(pedido);
        foreach (var linea in reserva.Lineas)
        {
            await using var cmd = conexion.CreateCommand();
            cmd.Transaction = transaccion;
            cmd.CommandText = """
                IF NOT EXISTS (SELECT 1 FROM [inventario].[ReservaInventario] WHERE [EventoIntegracionId] = @EventoId)
                AND NOT EXISTS (SELECT 1 FROM [inventario].[ReservaInventario]
                    WHERE [PedidoId]=@PedidoId AND [ProductoId]=@ProductoId AND [BodegaId]=@BodegaId)
                INSERT INTO [inventario].[ReservaInventario]
                    ([Id],[TenantId],[EmpresaId],[PedidoId],[ProductoId],[BodegaId],[Cantidad],[EventoIntegracionId])
                VALUES (@Id,@TenantId,@EmpresaId,@PedidoId,@ProductoId,@BodegaId,@Cantidad,@EventoId);
                """;
            cmd.Parameters.AddWithValue("@Id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@TenantId", reserva.TenantId);
            cmd.Parameters.AddWithValue("@EmpresaId", reserva.EmpresaId);
            cmd.Parameters.AddWithValue("@PedidoId", reserva.PedidoId);
            cmd.Parameters.AddWithValue("@ProductoId", linea.ProductoReferenciaId);
            cmd.Parameters.AddWithValue("@BodegaId", linea.BodegaId);
            cmd.Parameters.AddWithValue("@Cantidad", linea.Cantidad);
            cmd.Parameters.AddWithValue("@EventoId", evento.MessageId);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task PersistirEntradasAsync(
        EventoIntegracionEntrada evento, SqlConnection conexion, SqlTransaction transaccion, CancellationToken cancellationToken)
    {
        if (evento.Carga is not RecepcionCompraIntegracion recepcion)
            return;
        var fecha = DateOnly.FromDateTime(DateTime.UtcNow);
        var secuencia = 0;
        foreach (var linea in recepcion.Lineas)
        {
            secuencia++;
            var productoId = linea.ProductoReferenciaId;
            var bodegaId = linea.BodegaId;
            var movPendiente = new MovimientoInventarioPendiente(
                Guid.NewGuid(), fecha, secuencia, TipoMovimientoInventario.Entrada, EsEntrada: true,
                linea.CantidadRecibida, linea.CostoUnitario);
            var estado = await LeerExistenciaAsync(conexion, transaccion, productoId, bodegaId, cancellationToken);
            var politica = new PoliticaBodega(await BodegaPermiteNegativosAsync(conexion, transaccion, bodegaId, cancellationToken));
            var (nuevoEstado, procesado) = MotorPromedioPonderado.Aplicar(estado, movPendiente, politica);
            await InsertarMovimientoAsync(conexion, transaccion, recepcion, productoId, bodegaId, procesado, cancellationToken);
            await GuardarExistenciaAsync(conexion, transaccion, productoId, bodegaId, nuevoEstado, cancellationToken);
        }
    }

    private static async Task<EstadoExistencia> LeerExistenciaAsync(
        SqlConnection conexion, SqlTransaction transaccion, Guid productoId, Guid bodegaId, CancellationToken cancellationToken)
    {
        await using var cmd = conexion.CreateCommand();
        cmd.Transaction = transaccion;
        cmd.CommandText = """
            SELECT [Cantidad],[ValorTotal],[CostoPromedio] FROM [inventario].[Existencia]
            WHERE [ProductoId]=@ProductoId AND [BodegaId]=@BodegaId;
            """;
        cmd.Parameters.AddWithValue("@ProductoId", productoId);
        cmd.Parameters.AddWithValue("@BodegaId", bodegaId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return new EstadoExistencia(0, 0, 0);
        return new EstadoExistencia(reader.GetDecimal(0), reader.GetDecimal(1), reader.GetDecimal(2));
    }

    private static async Task<bool> BodegaPermiteNegativosAsync(
        SqlConnection conexion, SqlTransaction transaccion, Guid bodegaId, CancellationToken cancellationToken)
    {
        await using var cmd = conexion.CreateCommand();
        cmd.Transaction = transaccion;
        cmd.CommandText = "SELECT [PermitirExistenciasNegativas] FROM [inventario].[Bodega] WHERE [Id]=@Id;";
        cmd.Parameters.AddWithValue("@Id", bodegaId);
        var valor = await cmd.ExecuteScalarAsync(cancellationToken);
        return valor is bool b && b;
    }

    private static async Task InsertarMovimientoAsync(
        SqlConnection conexion, SqlTransaction transaccion, RecepcionCompraIntegracion recepcion, Guid productoId, Guid bodegaId,
        MovimientoInventarioProcesado mov, CancellationToken cancellationToken)
    {
        await using var cmd = conexion.CreateCommand();
        cmd.Transaction = transaccion;
        cmd.CommandText = """
            IF NOT EXISTS (SELECT 1 FROM [inventario].[MovimientoInventario] WHERE [Id]=@Id)
            INSERT INTO [inventario].[MovimientoInventario]
                ([Id],[TenantId],[EmpresaId],[ProductoId],[BodegaId],[Fecha],[Secuencia],[Tipo],[EsEntrada],
                 [Cantidad],[CostoUnitarioEntrada],[CostoUnitarioAplicado],[ValorTotal],[MarcadoAjusteNegativo],
                 [ModuloOrigen],[DocumentoOrigenId])
            VALUES (@Id,@TenantId,@EmpresaId,@ProductoId,@BodegaId,@Fecha,@Secuencia,@Tipo,1,
                 @Cantidad,@CostoEntrada,@CostoAplicado,@ValorTotal,@AjusteNeg,'Integracion',@DocumentoOrigenId);
            """;
        cmd.Parameters.AddWithValue("@Id", mov.Id);
        cmd.Parameters.AddWithValue("@TenantId", recepcion.TenantId);
        cmd.Parameters.AddWithValue("@EmpresaId", recepcion.EmpresaId);
        cmd.Parameters.AddWithValue("@ProductoId", productoId);
        cmd.Parameters.AddWithValue("@BodegaId", bodegaId);
        cmd.Parameters.AddWithValue("@Fecha", mov.Fecha);
        cmd.Parameters.AddWithValue("@Secuencia", mov.Secuencia);
        cmd.Parameters.AddWithValue("@Tipo", mov.Tipo.ToString());
        cmd.Parameters.AddWithValue("@Cantidad", mov.Cantidad);
        cmd.Parameters.AddWithValue("@CostoEntrada", mov.CostoUnitarioAplicado);
        cmd.Parameters.AddWithValue("@CostoAplicado", mov.CostoUnitarioAplicado);
        cmd.Parameters.AddWithValue("@ValorTotal", mov.ValorTotal);
        cmd.Parameters.AddWithValue("@AjusteNeg", mov.MarcadoAjusteNegativo);
        cmd.Parameters.AddWithValue("@DocumentoOrigenId", recepcion.RecepcionId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task GuardarExistenciaAsync(
        SqlConnection conexion, SqlTransaction transaccion, Guid productoId, Guid bodegaId, EstadoExistencia estado,
        CancellationToken cancellationToken)
    {
        await using var cmd = conexion.CreateCommand();
        cmd.Transaction = transaccion;
        cmd.CommandText = """
            MERGE [inventario].[Existencia] AS t
            USING (SELECT @ProductoId AS ProductoId, @BodegaId AS BodegaId) AS s
            ON t.[ProductoId]=s.[ProductoId] AND t.[BodegaId]=s.[BodegaId]
            WHEN MATCHED THEN UPDATE SET [Cantidad]=@Cantidad,[ValorTotal]=@Valor,[CostoPromedio]=@Costo
            WHEN NOT MATCHED THEN INSERT ([ProductoId],[BodegaId],[Cantidad],[ValorTotal],[CostoPromedio])
                VALUES (@ProductoId,@BodegaId,@Cantidad,@Valor,@Costo);
            """;
        cmd.Parameters.AddWithValue("@ProductoId", productoId);
        cmd.Parameters.AddWithValue("@BodegaId", bodegaId);
        cmd.Parameters.AddWithValue("@Cantidad", estado.Cantidad);
        cmd.Parameters.AddWithValue("@Valor", estado.ValorTotal);
        cmd.Parameters.AddWithValue("@Costo", estado.CostoPromedio);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
