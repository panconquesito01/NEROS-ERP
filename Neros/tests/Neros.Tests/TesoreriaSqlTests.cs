using Xunit;

namespace Neros.Tests;

public sealed class TesoreriaSqlTests
{
    [Fact]
    public async Task ModuloTesoreriaDespliegaYValidaAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "tesoreria");
        await BaseDatosPruebas.EliminarAsync(conexion);
    }
}
