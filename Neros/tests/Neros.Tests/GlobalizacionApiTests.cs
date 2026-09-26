using System.Net.Http.Headers;
using System.Net.Http.Json;
using Neros.Contracts.Globalizacion;
using Xunit;

namespace Neros.Tests;

public sealed class GlobalizacionApiTests(EntornoPruebas entorno) : IClassFixture<EntornoPruebas>
{
    [Fact]
    public async Task CatalogosTransversales_RequierenSesionYDevuelvenIsoAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        using var cliente = Cliente(await entorno.EntrarAsync(cuenta));
        var catalogos = await cliente.GetFromJsonAsync<CatalogosTransversales>("api/globalizacion/catalogos?pais=CO");
        Assert.NotNull(catalogos);
        Assert.Contains(catalogos!.Paises, p => p.Codigo == "CO");
        Assert.Contains(catalogos.Monedas, m => m.Codigo == "COP");
        Assert.Contains(catalogos.ZonasHorarias, z => z.Id == "America/Bogota");
        Assert.Contains(catalogos.TiposIdentificacion, t => t.Codigo == "NIT");
    }

    private HttpClient Cliente(Contracts.Autenticacion.AccesoConcedido acceso)
    {
        var cliente = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = entorno.Http.BaseAddress };
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", acceso.Token);
        return cliente;
    }
}
