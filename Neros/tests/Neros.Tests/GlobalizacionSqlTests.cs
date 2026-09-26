using Microsoft.Data.SqlClient;
using Xunit;

namespace Neros.Tests;

public sealed class GlobalizacionSqlTests
{
    [Fact]
    public async Task ModuloGlobalizacionDespliegaYValidaAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "globalizacion");
        await using var sql = new SqlConnection(conexion);
        await sql.OpenAsync();
        await using var monedas = new SqlCommand("SELECT COUNT(*) FROM globalizacion.Moneda", sql);
        var totalMonedas = (int)(await monedas.ExecuteScalarAsync())!;
        Assert.True(totalMonedas >= 8);
        await using var tasa = new SqlCommand(
            "SELECT Valor FROM globalizacion.TasaCambio WHERE MonedaOrigen = 'USD' AND MonedaDestino = 'COP'", sql);
        var valor = (decimal)(await tasa.ExecuteScalarAsync())!;
        Assert.Equal(4200m, valor);
        await using var regla = new SqlCommand(
            "SELECT Estado FROM cumplimiento.ReglaNormativa WHERE Codigo = 'LEY1581'", sql);
        Assert.Equal("POR_VERIFICAR", (string)(await regla.ExecuteScalarAsync())!);
        await using var zonas = new SqlCommand("SELECT COUNT(*) FROM globalizacion.ZonaHoraria", sql);
        Assert.True((int)(await zonas.ExecuteScalarAsync())! >= 5);
        await using var nit = new SqlCommand(
            "SELECT COUNT(*) FROM globalizacion.TipoIdentificacion WHERE Pais = 'CO' AND Codigo = 'NIT'", sql);
        Assert.Equal(1, (int)(await nit.ExecuteScalarAsync())!);
        await BaseDatosPruebas.EliminarAsync(conexion);
    }
}
