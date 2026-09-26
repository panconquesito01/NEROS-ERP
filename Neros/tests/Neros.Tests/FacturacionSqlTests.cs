using Xunit;

namespace Neros.Tests;

public sealed class FacturacionSqlTests
{
    [Fact]
    public async Task ModuloFacturacionDespliegaYValidaAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "facturacion");
        await BaseDatosPruebas.EliminarAsync(conexion);
    }
}
