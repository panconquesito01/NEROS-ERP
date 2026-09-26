using Xunit;

namespace Neros.Tests;

public sealed class ProyectosSqlTests
{
    [Fact]
    public async Task ModuloProyectosDespliegaYValidaAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "proyectos");
        await BaseDatosPruebas.EliminarAsync(conexion);
    }
}
