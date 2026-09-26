using System.Text.Json;
using Microsoft.Data.SqlClient;
using Neros.Messaging.Abstractions;
using Neros.Messaging.Sql;
using Xunit;

namespace Neros.Tests;

public sealed class IntegracionOutboxInboxSqlTests
{
    [Fact]
    public async Task Outbox_DespachaYMantieneIdempotenciaDeInboxAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "ventas");
        var outbox = new AlmacenOutboxSql(conexion);
        var inbox = new AlmacenInboxSql(conexion);
        var bus = new BusEventosIntegracionPrueba();
        var despachador = new DespachadorOutbox(outbox, bus);

        var mensaje = CrearMensaje();
        await using (var sql = new SqlConnection(conexion))
        {
            await sql.OpenAsync();
            await using var tx = (SqlTransaction)await sql.BeginTransactionAsync();
            await outbox.AgregarAsync(mensaje, sql, tx);
            await tx.CommitAsync();
        }

        var publicados = await despachador.DespacharLoteAsync("test", 10);
        Assert.Equal(1, publicados);
        Assert.Single(bus.Publicados);

        var publicadosSegundoIntento = await despachador.DespacharLoteAsync("test", 10);
        Assert.Equal(0, publicadosSegundoIntento);

        Assert.True(await inbox.TryRegisterAsync("consumidor.a", mensaje.MessageId, CancellationToken.None));
        Assert.False(await inbox.TryRegisterAsync("consumidor.a", mensaje.MessageId, CancellationToken.None));

        await BaseDatosPruebas.EliminarAsync(conexion);
    }

    [Fact]
    public async Task Outbox_ReprocesoTrasErrorIncrementaIntentosAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "inventario");
        var outbox = new AlmacenOutboxSql(conexion);
        var mensaje = CrearMensaje();
        await using (var sql = new SqlConnection(conexion))
        {
            await sql.OpenAsync();
            await using var tx = (SqlTransaction)await sql.BeginTransactionAsync();
            await outbox.AgregarAsync(mensaje, sql, tx);
            await tx.CommitAsync();
        }

        var busFallido = new BusEventosIntegracionPrueba { Fallar = true };
        var despachador = new DespachadorOutbox(outbox, busFallido);
        await despachador.DespacharLoteAsync("test", 10);
        await using var verificar = new SqlConnection(conexion);
        await verificar.OpenAsync();
        await using var cmd = verificar.CreateCommand();
        cmd.CommandText = "SELECT [Intentos], [PublicadoEnUtc] FROM [integracion].[MensajeSalida] WHERE [EventoId] = @Id";
        cmd.Parameters.AddWithValue("@Id", mensaje.MessageId);
        await using var lector = await cmd.ExecuteReaderAsync();
        Assert.True(await lector.ReadAsync());
        Assert.True(lector.GetInt32(0) >= 1);
        Assert.True(lector.IsDBNull(1));

        await BaseDatosPruebas.EliminarAsync(conexion);
    }

    [Fact]
    public async Task Outbox_MultiModulo_DespachaVentasYComprasAsync()
    {
        var conexionVentas = BaseDatosPruebas.NuevaConexion();
        var conexionCompras = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexionVentas, "ventas");
        await BaseDatosPruebas.DesplegarAsync(conexionCompras, "compras");
        var bus = new BusEventosIntegracionPrueba();
        var multi = new DespachadorOutboxModulos(
        [
            new DespachadorOutbox(new AlmacenOutboxSql(conexionVentas), bus),
            new DespachadorOutbox(new AlmacenOutboxSql(conexionCompras), bus)
        ]);
        var msgVentas = CrearMensaje();
        var msgCompras = CrearMensaje();
        foreach (var (conexion, mensaje) in new[] { (conexionVentas, msgVentas), (conexionCompras, msgCompras) })
        {
            await using var sql = new SqlConnection(conexion);
            await sql.OpenAsync();
            await using var tx = (SqlTransaction)await sql.BeginTransactionAsync();
            await new AlmacenOutboxSql(conexion).AgregarAsync(mensaje, sql, tx);
            await tx.CommitAsync();
        }
        Assert.Equal(2, await multi.DespacharLoteAsync("multi", 10));
        Assert.Equal(2, bus.Publicados.Count);
        await BaseDatosPruebas.EliminarAsync(conexionVentas);
        await BaseDatosPruebas.EliminarAsync(conexionCompras);
    }

    private static IntegrationEnvelope CrearMensaje()
    {
        using var doc = JsonDocument.Parse("{\"pedidoId\":\"demo\"}");
        return new IntegrationEnvelope(
            Guid.NewGuid(), "SalesOrderConfirmed.v1", "Ventas", Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid().ToString(), 1, DateTimeOffset.UtcNow, null, Guid.NewGuid(), null, null, doc.RootElement);
    }

    private sealed class BusEventosIntegracionPrueba : IEventBus
    {
        public bool Fallar { get; set; }
        public List<IntegrationEnvelope> Publicados { get; } = [];

        public Task PublishAsync(IntegrationEnvelope message, CancellationToken cancellationToken)
        {
            if (Fallar) throw new InvalidOperationException("Simulacion de caida.");
            Publicados.Add(message);
            return Task.CompletedTask;
        }
    }
}
