using System.Text.Json;
using Microsoft.Data.SqlClient;
using Neros.Domain.Integracion;
using Neros.Messaging.Abstractions;
using Neros.Messaging.Sql;
using Xunit;

namespace Neros.Tests;

public sealed class IntegracionE2ETests
{
    [Fact]
    public async Task Cadena_OutboxDespachoConsumidorInventario_IdempotenteAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "inventario");
        var bus = new BusIntegracionPrueba();
        var outbox = new AlmacenOutboxSql(conexion);
        var consumidor = RegistroConsumidoresIntegracion.Crear(
            MotorEnrutamientoIntegracion.ConsumidorInventarioReservas, conexion);

        bus.Suscribir(MotorEnrutamientoIntegracion.TipoPedidoConfirmado, async (sobre, _) =>
        {
            await consumidor.ProcesarAsync(sobre);
        });

        var producto = Guid.NewGuid();
        var bodega = Guid.NewGuid();
        var pedidoId = Guid.NewGuid();
        using var payload = JsonDocument.Parse($$"""
            {
              "pedidoId":"{{pedidoId}}",
              "tenantId":"{{Guid.NewGuid()}}",
              "empresaId":"{{Guid.NewGuid()}}",
              "versionAgregado":1,
              "lineas":[{"productoReferenciaId":"{{producto}}","bodegaId":"{{bodega}}","cantidad":2}]
            }
            """);
        var mensaje = new IntegrationEnvelope(
            Guid.NewGuid(), MotorEnrutamientoIntegracion.TipoPedidoConfirmado, "Ventas",
            Guid.NewGuid(), Guid.NewGuid(), pedidoId.ToString(), 1, DateTimeOffset.UtcNow,
            null, Guid.NewGuid(), null, null, payload.RootElement);

        await using (var sql = new SqlConnection(conexion))
        {
            await sql.OpenAsync();
            await using var tx = (SqlTransaction)await sql.BeginTransactionAsync();
            await outbox.AgregarAsync(mensaje, sql, tx);
            await tx.CommitAsync();
        }
        var despachador = new DespachadorOutbox(outbox, bus);
        Assert.Equal(1, await despachador.DespacharLoteAsync("e2e", 10));
        Assert.Equal(1, bus.Entregados);

        var (segundo, efectos) = await consumidor.ProcesarAsync(mensaje);
        Assert.Equal(ResultadoProcesamientoIntegracion.DuplicadoIgnorado, segundo);
        Assert.Empty(efectos);

        await BaseDatosPruebas.EliminarAsync(conexion);
    }

    [Fact]
    public async Task Recepcion_DosConsumidores_InventarioYContabilidadAsync()
    {
        var conexionInv = BaseDatosPruebas.NuevaConexion();
        var conexionCont = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexionInv, "inventario");
        await BaseDatosPruebas.DesplegarAsync(conexionCont, "contabilidad");

        var inventario = RegistroConsumidoresIntegracion.Crear(
            MotorEnrutamientoIntegracion.ConsumidorInventarioEntradas, conexionInv);
        var contabilidad = RegistroConsumidoresIntegracion.Crear(
            MotorEnrutamientoIntegracion.ConsumidorContabilidad, conexionCont);

        var recepcionId = Guid.NewGuid();
        using var payload = JsonDocument.Parse($$"""
            {
              "recepcionId":"{{recepcionId}}",
              "ordenCompraId":"{{Guid.NewGuid()}}",
              "tenantId":"{{Guid.NewGuid()}}",
              "empresaId":"{{Guid.NewGuid()}}",
              "versionAgregado":1,
              "lineas":[{"productoReferenciaId":"{{Guid.NewGuid()}}","bodegaId":"{{Guid.NewGuid()}}",
                "cantidadRecibida":5,"costoUnitario":1000}]
            }
            """);
        var sobre = new IntegrationEnvelope(
            Guid.NewGuid(), MotorEnrutamientoIntegracion.TipoRecepcionCompra, "Compras",
            Guid.NewGuid(), Guid.NewGuid(), recepcionId.ToString(), 1, DateTimeOffset.UtcNow,
            null, Guid.NewGuid(), null, null, payload.RootElement);

        var (rInv, eInv) = await inventario.ProcesarAsync(sobre);
        var (rCont, eCont) = await contabilidad.ProcesarAsync(sobre);

        Assert.Equal(ResultadoProcesamientoIntegracion.Procesado, rInv);
        Assert.Equal(ClaseEfectoIntegracion.MovimientoInventario, eInv[0].Clase);
        Assert.Equal(ResultadoProcesamientoIntegracion.Procesado, rCont);
        Assert.Equal(ClaseEfectoIntegracion.ReglasContables, eCont[0].Clase);

        await BaseDatosPruebas.EliminarAsync(conexionInv);
        await BaseDatosPruebas.EliminarAsync(conexionCont);
    }

    [Fact]
    public async Task Procesador_RechazaDesordenAntesDeInboxAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "inventario");
        var consumidor = RegistroConsumidoresIntegracion.Crear(
            MotorEnrutamientoIntegracion.ConsumidorInventarioReservas, conexion);

        var pedidoId = Guid.NewGuid();
        using var payload = JsonDocument.Parse($$"""
            {"pedidoId":"{{pedidoId}}","tenantId":"{{Guid.NewGuid()}}","empresaId":"{{Guid.NewGuid()}}",
             "versionAgregado":2,"lineas":[{"productoReferenciaId":"{{Guid.NewGuid()}}",
             "bodegaId":"{{Guid.NewGuid()}}","cantidad":1}]}
            """);
        var v2 = CrearPedido(pedidoId, 2, payload);
        var v1 = CrearPedido(pedidoId, 1, payload);

        Assert.Equal(ResultadoProcesamientoIntegracion.Procesado, (await consumidor.ProcesarAsync(v2)).Resultado);
        Assert.Equal(ResultadoProcesamientoIntegracion.DesordenIgnorado, (await consumidor.ProcesarAsync(v1)).Resultado);

        await BaseDatosPruebas.EliminarAsync(conexion);
    }

    private static IntegrationEnvelope CrearPedido(Guid pedidoId, long version, JsonDocument payload) =>
        new(Guid.NewGuid(), MotorEnrutamientoIntegracion.TipoPedidoConfirmado, "Ventas",
            Guid.NewGuid(), Guid.NewGuid(), pedidoId.ToString(), version, DateTimeOffset.UtcNow,
            null, Guid.NewGuid(), null, null, payload.RootElement);

    private sealed class BusIntegracionPrueba : IEventBus
    {
        private readonly List<(string RoutingKey, Func<IntegrationEnvelope, CancellationToken, Task> Handler)> _handlers = [];

        public int Entregados { get; private set; }

        public void Suscribir(string routingKey, Func<IntegrationEnvelope, CancellationToken, Task> handler)
            => _handlers.Add((routingKey, handler));

        public async Task PublishAsync(IntegrationEnvelope message, CancellationToken cancellationToken)
        {
            foreach (var (key, handler) in _handlers)
            {
                if (key != message.Type) continue;
                await handler(message, cancellationToken);
                Entregados++;
            }
        }
    }
}
