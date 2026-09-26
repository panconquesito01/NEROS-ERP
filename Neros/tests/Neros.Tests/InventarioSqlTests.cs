using Xunit;

namespace Neros.Tests;

public sealed class InventarioSqlTests
{
    [Fact]
    public async Task ModuloInventarioDespliegaYValidaAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "inventario");
        await BaseDatosPruebas.EliminarAsync(conexion);
    }
}
