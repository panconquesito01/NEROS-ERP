using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Neros.Contracts.Terceros;
using Neros.Domain.Terceros;
using Neros.Terceros.Persistence;
using Xunit;

namespace Neros.Tests;

public sealed class TercerosServicioTests
{
    [Fact]
    public async Task ServicioPersisteTerceroEnSqlAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "terceros");
        var services = new ServiceCollection();
        services.AddDbContext<TercerosDbContext>(options => options.UseSqlServer(conexion));
        services.AddScoped<ServicioTerceros>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var servicio = scope.ServiceProvider.GetRequiredService<ServicioTerceros>();
        var tenant = Guid.NewGuid();
        var nit = "890903938";
        var dv = ValidadorIdentificacionColombia.CalcularDigitoVerificacionNit(nit);
        var (detalle, error) = await servicio.CrearAsync(tenant, new SolicitudCrearTercero(
            "Organizacion", "Prueba SA", null,
            [new IdentificacionTercero("CO", "NIT", nit, dv, true)],
            ["Cliente"]), CancellationToken.None);
        Assert.Null(error);
        Assert.NotNull(detalle);
        var (actualizado, errorActualizar) = await servicio.ActualizarAsync(tenant, detalle!.Id, new SolicitudActualizarTercero(
            "Prueba SA Modificada", null, true,
            [new IdentificacionTercero("CO", "NIT", nit, dv, true)],
            ["Cliente"]), CancellationToken.None);
        Assert.Null(errorActualizar);
        Assert.NotNull(actualizado);
        var historial = await servicio.HistorialAsync(tenant, detalle.Id, CancellationToken.None);
        Assert.True(historial.Count >= 2);
        await BaseDatosPruebas.EliminarAsync(conexion);
    }
}
