using Xunit;

namespace Neros.Tests;

public sealed class NominaSqlTests
{
    [Fact]
    public async Task ModuloNominaDespliegaYValidaAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "nomina");
        await BaseDatosPruebas.EliminarAsync(conexion);
    }
}
