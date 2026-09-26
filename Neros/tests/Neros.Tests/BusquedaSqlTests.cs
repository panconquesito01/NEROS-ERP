using Xunit;

namespace Neros.Tests;

public sealed class BusquedaSqlTests
{
    [Fact]
    public async Task ModuloBusquedaDespliegaYValidaAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "busqueda");
        await BaseDatosPruebas.EliminarAsync(conexion);
    }
}
