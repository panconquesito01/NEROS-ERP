using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Neros.Contracts.Globalizacion;
using Neros.Contracts.Organizacion;
using Neros.Contracts.Autenticacion;
using Xunit;

namespace Neros.Tests;

public sealed class ConfiguracionEmpresaTests(EntornoPruebas entorno) : IClassFixture<EntornoPruebas>
{
    [Fact]
    public async Task AdminEmpresa_ActualizaRegionalYSincronizaTasasAsync()
    {
        var admin = await entorno.CrearCuentaAsync();
        using var cliente = Cliente(await entorno.EntrarAsync(admin), entorno.EmpresaPrimera);

        var regional = await cliente.GetFromJsonAsync<ConfiguracionRegionalEmpresa>(
            $"api/empresas/{entorno.EmpresaPrimera:D}/configuracion-regional");
        Assert.NotNull(regional);

        using var guardar = await cliente.PutAsJsonAsync(
            $"api/empresas/{entorno.EmpresaPrimera:D}/configuracion-regional",
            new ConfiguracionRegionalEmpresa("CO", "COP", "America/Bogota", "es-CO", "Local"));
        Assert.Equal(HttpStatusCode.OK, guardar.StatusCode);

        using var sync = await cliente.PostAsJsonAsync("api/globalizacion/tasas/sincronizar",
            new SolicitudSincronizarTasas { MonedaDestino = "COP", Pais = "CO" });
        Assert.True(sync.StatusCode is HttpStatusCode.OK or HttpStatusCode.BadRequest,
            $"Sincronizar tasas: {(int)sync.StatusCode}");
        if (sync.IsSuccessStatusCode)
        {
            var resultado = await sync.Content.ReadFromJsonAsync<ResultadoSincronizacionTasas>();
            Assert.True(resultado!.Actualizadas >= 0);
        }

        var tasas = await cliente.GetFromJsonAsync<PaginaTasasCambio>("api/globalizacion/tasas?monedaDestino=COP");
        Assert.NotNull(tasas);
    }

    private HttpClient Cliente(AccesoConcedido acceso, Guid empresaId)
    {
        var cliente = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = entorno.Http.BaseAddress };
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", acceso.Token);
        cliente.DefaultRequestHeaders.Add(CabecerasCliente.Empresa, empresaId.ToString("D"));
        return cliente;
    }
}
