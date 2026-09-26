using Xunit;

namespace Neros.Tests;

public sealed class ProduccionSqlTests
{
    [Fact]
    public async Task ModuloProduccionDespliegaYValidaAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "produccion");
        await BaseDatosPruebas.EliminarAsync(conexion);
    }
}
