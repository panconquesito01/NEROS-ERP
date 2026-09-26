using System.Text.Json;
using Microsoft.Data.SqlClient;
using Neros.Domain.Integracion;
using Neros.Messaging.Abstractions;
using Neros.Messaging.Sql;
using Xunit;

namespace Neros.Tests;

public sealed class IndexacionBusquedaSqlTests
{
    [Fact]
    public async Task Consumidor_IndexaPedidoConfirmadoAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "busqueda");
        var tenant = Guid.NewGuid();
        var empresa = Guid.NewGuid();
        var pedidoId = Guid.NewGuid();
        var cadenas = new Dictionary<string, string> { ["Search"] = conexion };
        var consumidor = RegistroConsumidoresIntegracion.Crear(
            MotorEnrutamientoIntegracion.ConsumidorBusqueda,
            conexion,
            RegistroConsumidoresIntegracion.ResolverEscritor(
                MotorEnrutamientoIntegracion.ConsumidorBusqueda, cadenas));

        using var payload = JsonDocument.Parse($$"""
            {"pedidoId":"{{pedidoId}}","tenantId":"{{tenant}}","empresaId":"{{empresa}}","versionAgregado":1,
             "referencia":"PED-900","clienteNombre":"Cliente Demo",
             "lineas":[{"productoReferenciaId":"{{Guid.NewGuid()}}","bodegaId":"{{Guid.NewGuid()}}","cantidad":1}]}
            """);
        var sobre = Envelope(MotorEnrutamientoIntegracion.TipoPedidoConfirmado, tenant, empresa, pedidoId.ToString(), payload);
        var (resultado, _) = await consumidor.ProcesarAsync(sobre);
        Assert.Equal(ResultadoProcesamientoIntegracion.Procesado, resultado);

        await using var verificar = new SqlConnection(conexion);
        await verificar.OpenAsync();
        await using var cmd = verificar.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(1) FROM [busqueda].[DocumentoIndice]
            WHERE [TenantId]=@TenantId AND [EntidadId]=@Entidad AND [Titulo] LIKE N'%PED-900%';
            """;
        cmd.Parameters.AddWithValue("@TenantId", tenant);
        cmd.Parameters.AddWithValue("@Entidad", pedidoId.ToString());
        Assert.Equal(1, (int)(await cmd.ExecuteScalarAsync())!);

        await BaseDatosPruebas.EliminarAsync(conexion);
    }

    private static IntegrationEnvelope Envelope(
        string tipo, Guid tenant, Guid empresa, string agregado, JsonDocument payload) =>
        new(Guid.NewGuid(), tipo, "Ventas", tenant, empresa, agregado, 1, DateTimeOffset.UtcNow,
            null, Guid.NewGuid(), null, null, payload.RootElement);
}
