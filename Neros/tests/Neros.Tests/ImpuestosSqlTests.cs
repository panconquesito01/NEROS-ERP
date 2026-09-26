using Xunit;

namespace Neros.Tests;

public sealed class ImpuestosSqlTests
{
    [Fact]
    public async Task ModuloImpuestosDespliegaYValidaAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "impuestos");
        await BaseDatosPruebas.EliminarAsync(conexion);
    }
}
