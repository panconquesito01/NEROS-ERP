using Xunit;

namespace Neros.Tests;

public sealed class ContabilidadSqlTests
{
    [Fact]
    public async Task ModuloContabilidadDespliegaYValidaAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "contabilidad");
        await BaseDatosPruebas.EliminarAsync(conexion);
    }
}
