using Xunit;

namespace Neros.Tests;

public sealed class TercerosSqlTests
{
    [Fact]
    public async Task ModuloTercerosDespliegaYValidaAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "terceros");
        await BaseDatosPruebas.EliminarAsync(conexion);
    }
}
