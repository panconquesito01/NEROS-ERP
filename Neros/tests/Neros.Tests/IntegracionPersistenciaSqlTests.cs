using System.Text.Json;
using Microsoft.Data.SqlClient;
using Neros.Domain.Contabilidad;
using Neros.Domain.Integracion;
using Neros.Messaging.Abstractions;
using Neros.Messaging.Sql;
using Xunit;

namespace Neros.Tests;

public sealed class IntegracionPersistenciaSqlTests
{
    [Fact]
    public async Task Consumidor_PersisteReservaInventarioAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "inventario");
        var tenant = Guid.NewGuid();
        var empresa = Guid.NewGuid();
        var producto = Guid.NewGuid();
        var bodega = Guid.NewGuid();
        await SemillarProductoBodegaAsync(conexion, tenant, empresa, producto, bodega);

        var cadenas = new Dictionary<string, string> { ["Inventario"] = conexion };
        var consumidor = RegistroConsumidoresIntegracion.Crear(
            MotorEnrutamientoIntegracion.ConsumidorInventarioReservas,
            conexion,
            RegistroConsumidoresIntegracion.ResolverEscritor(
                MotorEnrutamientoIntegracion.ConsumidorInventarioReservas, cadenas));

        var pedidoId = Guid.NewGuid();
        using var payload = JsonDocument.Parse($$"""
            {"pedidoId":"{{pedidoId}}","tenantId":"{{tenant}}","empresaId":"{{empresa}}","versionAgregado":1,
             "lineas":[{"productoReferenciaId":"{{producto}}","bodegaId":"{{bodega}}","cantidad":4}]}
            """);
        var sobre = Envelope(MotorEnrutamientoIntegracion.TipoPedidoConfirmado, tenant, empresa, pedidoId.ToString(), payload);
        var (resultado, _) = await consumidor.ProcesarAsync(sobre);
        Assert.Equal(ResultadoProcesamientoIntegracion.Procesado, resultado);

        await using var verificar = new SqlConnection(conexion);
        await verificar.OpenAsync();
        await using var cmd = verificar.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM [inventario].[ReservaInventario] WHERE [PedidoId]=@PedidoId";
        cmd.Parameters.AddWithValue("@PedidoId", pedidoId);
        Assert.Equal(1, (int)(await cmd.ExecuteScalarAsync())!);

        await BaseDatosPruebas.EliminarAsync(conexion);
    }

    [Fact]
    public async Task Consumidor_PersisteMovimientoYContabilidadAsync()
    {
        var conexionInv = BaseDatosPruebas.NuevaConexion();
        var conexionCont = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexionInv, "inventario");
        await BaseDatosPruebas.DesplegarAsync(conexionCont, "contabilidad");
        var tenant = Guid.NewGuid();
        var empresa = Guid.NewGuid();
        await SemillarContabilidadIntegracionAsync(conexionCont, tenant, empresa);
        var producto = Guid.NewGuid();
        var bodega = Guid.NewGuid();
        await SemillarProductoBodegaAsync(conexionInv, tenant, empresa, producto, bodega);

        var cadenas = new Dictionary<string, string> { ["Inventario"] = conexionInv, ["Contabilidad"] = conexionCont };
        var inventario = RegistroConsumidoresIntegracion.Crear(
            MotorEnrutamientoIntegracion.ConsumidorInventarioEntradas, conexionInv,
            RegistroConsumidoresIntegracion.ResolverEscritor(
                MotorEnrutamientoIntegracion.ConsumidorInventarioEntradas, cadenas));
        var contabilidad = RegistroConsumidoresIntegracion.Crear(
            MotorEnrutamientoIntegracion.ConsumidorContabilidad, conexionCont,
            RegistroConsumidoresIntegracion.ResolverEscritor(
                MotorEnrutamientoIntegracion.ConsumidorContabilidad, cadenas));

        var recepcionId = Guid.NewGuid();
        using var payload = JsonDocument.Parse($$"""
            {"recepcionId":"{{recepcionId}}","ordenCompraId":"{{Guid.NewGuid()}}","tenantId":"{{tenant}}",
             "empresaId":"{{empresa}}","versionAgregado":1,
             "lineas":[{"productoReferenciaId":"{{producto}}","bodegaId":"{{bodega}}",
             "cantidadRecibida":10,"costoUnitario":1000}]}
            """);
        var sobre = Envelope(MotorEnrutamientoIntegracion.TipoRecepcionCompra, tenant, empresa, recepcionId.ToString(), payload);
        Assert.Equal(ResultadoProcesamientoIntegracion.Procesado, (await inventario.ProcesarAsync(sobre)).Resultado);
        Assert.Equal(ResultadoProcesamientoIntegracion.Procesado, (await contabilidad.ProcesarAsync(sobre)).Resultado);

        await using (var verificar = new SqlConnection(conexionInv))
        {
            await verificar.OpenAsync();
            await using var cmd = verificar.CreateCommand();
            cmd.CommandText = "SELECT COUNT(1) FROM [inventario].[MovimientoInventario] WHERE [DocumentoOrigenId]=@Id";
            cmd.Parameters.AddWithValue("@Id", recepcionId);
            Assert.Equal(1, (int)(await cmd.ExecuteScalarAsync())!);
        }
        await using (var verificar = new SqlConnection(conexionCont))
        {
            await verificar.OpenAsync();
            await using var cmd = verificar.CreateCommand();
            cmd.CommandText = """
                SELECT COUNT(1) FROM [contabilidad].[SolicitudContabilizacionIntegracion]
                WHERE [OrigenAgregadoId]=@Id AND [Estado]='Contabilizado' AND [ComprobanteId] IS NOT NULL;
                """;
            cmd.Parameters.AddWithValue("@Id", recepcionId.ToString());
            Assert.Equal(1, (int)(await cmd.ExecuteScalarAsync())!);
            await using var cmd2 = verificar.CreateCommand();
            cmd2.CommandText = "SELECT COUNT(1) FROM [contabilidad].[Comprobante] WHERE [ModuloOrigen]='Compras'";
            Assert.Equal(1, (int)(await cmd2.ExecuteScalarAsync())!);
        }

        await BaseDatosPruebas.EliminarAsync(conexionInv);
        await BaseDatosPruebas.EliminarAsync(conexionCont);
    }

    private static async Task SemillarContabilidadIntegracionAsync(string conexion, Guid tenant, Guid empresa)
    {
        var ejercicio = Guid.NewGuid();
        var periodo = Guid.NewGuid();
        var tipo = Guid.NewGuid();
        var cuentaInv = Guid.NewGuid();
        var cuentaIva = Guid.NewGuid();
        var cuentaProv = Guid.NewGuid();
        await using var sql = new SqlConnection(conexion);
        await sql.OpenAsync();
        await using var cmd = sql.CreateCommand();
        cmd.CommandText = """
            INSERT INTO [contabilidad].[Ejercicio] ([Id],[TenantId],[EmpresaId],[Anio]) VALUES (@Ej,@T,@E,2026);
            INSERT INTO [contabilidad].[Periodo] ([Id],[EjercicioId],[Numero],[FechaInicio],[FechaFin],[Estado])
                VALUES (@Pe,@Ej,1,'2026-01-01','2026-12-31','Abierto');
            INSERT INTO [contabilidad].[TipoComprobante] ([Id],[TenantId],[EmpresaId],[Codigo],[Nombre]) VALUES (@Tipo,@T,@E,'INT',N'Integracion');
            INSERT INTO [contabilidad].[CuentaContable]
                ([Id],[TenantId],[EmpresaId],[Codigo],[Nombre],[Naturaleza],[Tipo],[Nivel],[AdmiteMovimiento])
            VALUES
                (@CInv,@T,@E,'1405',N'Inventario','D','Activo',1,1),
                (@CIva,@T,@E,'2408',N'IVA descontable','D','Activo',1,1),
                (@CProv,@T,@E,'2205',N'Proveedores','C','Pasivo',1,1);
            INSERT INTO [contabilidad].[MapeoRolContable] ([TenantId],[EmpresaId],[Rol],[CuentaContableId]) VALUES
                (@T,@E,@RolInv,@CInv),(@T,@E,@RolIva,@CIva),(@T,@E,@RolProv,@CProv);
            """;
        cmd.Parameters.AddWithValue("@Ej", ejercicio);
        cmd.Parameters.AddWithValue("@Pe", periodo);
        cmd.Parameters.AddWithValue("@Tipo", tipo);
        cmd.Parameters.AddWithValue("@T", tenant);
        cmd.Parameters.AddWithValue("@E", empresa);
        cmd.Parameters.AddWithValue("@CInv", cuentaInv);
        cmd.Parameters.AddWithValue("@CIva", cuentaIva);
        cmd.Parameters.AddWithValue("@CProv", cuentaProv);
        cmd.Parameters.AddWithValue("@RolInv", RolContable.CuentaInventario.ToString());
        cmd.Parameters.AddWithValue("@RolIva", RolContable.CuentaIvaDescontable.ToString());
        cmd.Parameters.AddWithValue("@RolProv", RolContable.CuentaProveedor.ToString());
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task SemillarProductoBodegaAsync(
        string conexion, Guid tenant, Guid empresa, Guid producto, Guid bodega)
    {
        await using var sql = new SqlConnection(conexion);
        await sql.OpenAsync();
        await using var cmd = sql.CreateCommand();
        cmd.CommandText = """
            INSERT INTO [inventario].[Bodega] ([Id],[TenantId],[EmpresaId],[Codigo],[Nombre]) VALUES (@B,@T,@E,'B1',N'Bodega');
            INSERT INTO [inventario].[Producto] ([Id],[TenantId],[EmpresaId],[Codigo],[Nombre]) VALUES (@P,@T,@E,'P1',N'Producto');
            """;
        cmd.Parameters.AddWithValue("@B", bodega);
        cmd.Parameters.AddWithValue("@P", producto);
        cmd.Parameters.AddWithValue("@T", tenant);
        cmd.Parameters.AddWithValue("@E", empresa);
        await cmd.ExecuteNonQueryAsync();
    }

    private static IntegrationEnvelope Envelope(
        string tipo, Guid tenant, Guid empresa, string agregado, JsonDocument payload) =>
        new(Guid.NewGuid(), tipo, "Test", tenant, empresa, agregado, 1, DateTimeOffset.UtcNow,
            null, Guid.NewGuid(), null, null, payload.RootElement);
}
