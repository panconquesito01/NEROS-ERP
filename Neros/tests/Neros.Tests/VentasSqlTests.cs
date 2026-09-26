using Xunit;

namespace Neros.Tests;

public sealed class VentasSqlTests
{
    [Fact]
    public async Task ModuloVentasDespliegaYValidaAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "ventas");
        await BaseDatosPruebas.EliminarAsync(conexion);
    }
}
