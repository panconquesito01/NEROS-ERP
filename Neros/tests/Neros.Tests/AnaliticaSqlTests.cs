using Xunit;

namespace Neros.Tests;

public sealed class AnaliticaSqlTests
{
    [Fact]
    public async Task ModuloAnaliticaDespliegaYValidaAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "analitica");
        await BaseDatosPruebas.EliminarAsync(conexion);
    }
}
