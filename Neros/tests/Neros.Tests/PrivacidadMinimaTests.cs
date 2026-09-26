using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Neros.Application.Privacidad;
using Neros.Application.Seguridad;
using Neros.Contracts.Autenticacion;
using Neros.Contracts.Cuenta;
using Neros.Contracts.Privacidad;
using Neros.Persistence;
using Xunit;

namespace Neros.Tests;

public sealed class PrivacidadMinimaTests(EntornoPruebas entorno) : IClassFixture<EntornoPruebas>
{
    [Fact]
    public async Task CookiesPublicasListanDefinicionesAsync()
    {
        var cookies = await entorno.Http.GetFromJsonAsync<List<DefinicionCookiePublica>>("api/privacidad/cookies");
        Assert.NotNull(cookies);
        Assert.NotEmpty(cookies!);
        Assert.Contains(cookies, c => c.Esencial && c.Nombre == "Neros.Sesion");
    }

    [Fact]
    public async Task ApiBloqueaEmpresasSinAceptarLegalesAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        var acceso = await entorno.EntrarAsync(cuenta, aceptarLegales: false);
        using var cliente = Cliente(acceso);
        using var respuesta = await cliente.GetAsync("api/empresas");
        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorOperacion>();
        Assert.Equal(CodigosPrivacidad.DocumentosPendientes, error!.Codigo);
    }

    [Fact]
    public async Task AceptacionLegalDirectaEnServicioAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        var acceso = await entorno.EntrarAsync(cuenta, aceptarLegales: false);
        await using var scope = entorno.Api.Services.CreateAsyncScope();
        var privacidad = scope.ServiceProvider.GetRequiredService<IServicioPrivacidad>();
        var pendientes = await privacidad.ListarPendientesAsync(cuenta.Id, CancellationToken.None);
        Assert.Equal(2, pendientes.Count);
        foreach (var pendiente in pendientes)
        {
            var (correcto, error) = await privacidad.AceptarAsync(cuenta.Id, pendiente.Codigo,
                new SolicitudAceptacionLegal(pendiente.VersionId, pendiente.HashContenido), new ContextoCliente("127.0.0.1", "test", "corr"), CancellationToken.None);
            Assert.True(correcto, error);
        }
    }

    [Fact]
    public async Task AceptacionLegalPersisteHashYDesbloqueaAccesoAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        var acceso = await entorno.EntrarAsync(cuenta, aceptarLegales: false);
        using var cliente = Cliente(acceso);
        var pendientes = (await cliente.GetFromJsonAsync<List<DocumentoLegalPendiente>>("api/privacidad/documentos/pendientes"))!;
        Assert.NotEmpty(pendientes);
        foreach (var pendiente in pendientes)
        {
            using var aceptar = await cliente.PostAsJsonAsync($"api/privacidad/documentos/{pendiente.Codigo}/aceptar",
                new SolicitudAceptacionLegal(pendiente.VersionId, pendiente.HashContenido));
            aceptar.EnsureSuccessStatusCode();
        }
        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync("api/empresas")).StatusCode);

        await using var scope = entorno.Api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NerosDbContext>();
        var aceptaciones = await db.AceptacionesLegales.AsNoTracking().Where(a => a.UsuarioId == cuenta.Id).ToListAsync();
        Assert.Equal(pendientes.Count, aceptaciones.Count);
        Assert.All(aceptaciones, a => Assert.False(string.IsNullOrWhiteSpace(a.HashContenido)));
    }

    [Fact]
    [Trait("Categoria", "Navegador")]
    public async Task PaginaCookiesEsPublicaEnBlazorAsync()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true, Channel = OperatingSystem.IsWindows() ? "msedge" : null
        });
        var page = await browser.NewPageAsync(new BrowserNewPageOptions { BaseURL = entorno.Web.BaseAddress!.ToString(), Locale = "es-ES" });
        await page.GotoAsync("/cookies");
        await page.WaitForSelectorAsync("h1");
        var titulo = await page.TextContentAsync("h1");
        Assert.Contains("cookies", titulo!, StringComparison.OrdinalIgnoreCase);
        Assert.True(await page.Locator(".n-table tbody tr").CountAsync() > 0);
    }

    private HttpClient Cliente(AccesoConcedido acceso)
    {
        var cliente = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = entorno.Http.BaseAddress };
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", acceso.Token);
        return cliente;
    }
}
