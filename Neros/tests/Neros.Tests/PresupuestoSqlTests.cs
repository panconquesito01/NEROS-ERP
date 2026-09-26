using Xunit;

namespace Neros.Tests;

public sealed class PresupuestoSqlTests
{
    [Fact]
    public async Task ModuloPresupuestoDespliegaYValidaAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "presupuesto");
        await BaseDatosPruebas.EliminarAsync(conexion);
    }
}
