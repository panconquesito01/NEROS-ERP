using Xunit;

namespace Neros.Tests;

public sealed class ComprasSqlTests
{
    [Fact]
    public async Task ModuloComprasDespliegaYValidaAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "compras");
        await BaseDatosPruebas.EliminarAsync(conexion);
    }
}
