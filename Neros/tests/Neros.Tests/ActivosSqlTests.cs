using Xunit;

namespace Neros.Tests;

public sealed class ActivosSqlTests
{
    [Fact]
    public async Task ModuloActivosDespliegaYValidaAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "activos");
        await BaseDatosPruebas.EliminarAsync(conexion);
    }
}
