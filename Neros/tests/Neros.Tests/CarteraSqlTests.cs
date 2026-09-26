using Xunit;

namespace Neros.Tests;

public sealed class CarteraSqlTests
{
    [Fact]
    public async Task ModuloCarteraDespliegaYValidaAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "cartera");
        await BaseDatosPruebas.EliminarAsync(conexion);
    }
}
