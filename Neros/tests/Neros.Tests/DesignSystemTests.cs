using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Neros.Persistence.Seguridad;
using Xunit;

namespace Neros.Tests;

public sealed class DesignSystemTests(EntornoPruebas entorno) : IClassFixture<EntornoPruebas>
{
    [Fact]
    [Trait("Categoria", "Navegador")]
    public async Task DashboardCapturasEnTresIdiomasDosTemasYDosTamanosAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        await entorno.AceptarLegalesAsync(await entorno.EntrarAsync(cuenta, aceptarLegales: false));
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Channel = OperatingSystem.IsWindows() ? "msedge" : null
        });
        foreach (var (cultura, locale, entrar, empresa, modulo, claro, oscuro) in Idiomas())
        {
            await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                BaseURL = entorno.Web.BaseAddress!.ToString(),
                Locale = locale,
                ViewportSize = new ViewportSize { Width = 1440, Height = 960 }
            });
            var page = await context.NewPageAsync();
            await page.GotoAsync("/login");
            await CambiarIdiomaAsync(page, cultura);
            await page.GetByLabel(entrar.Correo, new() { Exact = true }).FillAsync(cuenta.Correo);
            await page.GetByLabel(entrar.Clave, new() { Exact = true }).FillAsync(cuenta.Clave);
            await page.GetByRole(AriaRole.Button, new() { Name = entrar.Boton, Exact = true }).ClickAsync();
            await page.WaitForURLAsync("**/empresas", new() { Timeout = 30_000 });
            await page.GetByRole(AriaRole.Button, new() { Name = empresa, Exact = true }).ClickAsync();
            await page.WaitForURLAsync("**/home", new() { Timeout = 30_000 });
            await Assertions.Expect(page.Locator("[data-module-grid]")).ToBeVisibleAsync();
            await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = modulo })).ToBeVisibleAsync();
            await AccesoMultiempresaTests.CapturarAsync(page, $"inicio-{cultura}", (claro, oscuro));
        }
    }

    [Fact]
    [Trait("Categoria", "Navegador")]
    public async Task NerosDataGridCapturasAdministracionUsuariosAsync()
    {
        var admin = await CrearAdministradorAsync();
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Channel = OperatingSystem.IsWindows() ? "msedge" : null
        });
        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = entorno.Web.BaseAddress!.ToString(),
            Locale = "es-ES",
            ViewportSize = new ViewportSize { Width = 1440, Height = 960 }
        });
        var page = await context.NewPageAsync();
        await NavegacionPruebas.EntrarAsync(page, admin.Correo, admin.Clave);
        await page.WaitForURLAsync("**/empresas", new() { Timeout = 30_000 });
        await page.GetByRole(AriaRole.Button, new() { Name = "Entrar a Neros Distribucion (prueba)", Exact = true }).ClickAsync();
        await page.WaitForURLAsync("**/home", new() { Timeout = 30_000 });
        await page.GotoAsync("/administracion/usuarios");
        await Assertions.Expect(page.Locator(".n-data-grid")).ToBeVisibleAsync();
        await AccesoMultiempresaTests.CapturarAsync(page, "usuarios");
    }

    private async Task<CuentaPrueba> CrearAdministradorAsync()
    {
        await using var scope = entorno.Api.Services.CreateAsyncScope();
        var usuarios = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        var cuenta = await entorno.CrearCuentaAsync();
        var usuario = await usuarios.FindByIdAsync(cuenta.Id);
        Assert.NotNull(usuario);
        Assert.True((await usuarios.AddClaimAsync(usuario, new Claim(ClaimTypes.Role, "AdministradorGlobal"))).Succeeded);
        return cuenta;
    }

    private static async Task CambiarIdiomaAsync(IPage page, string cultura)
    {
        await page.Locator("[data-language-menu] summary").ClickAsync();
        var etiqueta = cultura switch
        {
            "en" => "English",
            "pt" => "Português",
            _ => "Español"
        };
        await page.GetByRole(AriaRole.Button, new() { Name = etiqueta }).ClickAsync();
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("lang", cultura == "pt" ? "pt" : cultura);
    }

    private static IEnumerable<(string Cultura, string Locale, (string Correo, string Clave, string Boton) Entrar, string Empresa, string Modulo, string Claro, string Oscuro)> Idiomas()
    {
        yield return ("es", "es-ES", ("Correo electrónico", "Contraseña", "Entrar a Neros"), "Entrar a Neros Distribucion (prueba)", "Módulos del ERP", "Tema claro", "Tema oscuro");
        yield return ("en", "en-US", ("Email", "Password", "Sign in to Neros"), "Enter Neros Distribucion (prueba)", "ERP modules", "Light theme", "Dark theme");
        yield return ("pt", "pt-BR", ("E-mail", "Senha", "Entrar no Neros"), "Entrar em Neros Distribucion (prueba)", "Módulos do ERP", "Tema claro", "Tema escuro");
    }
}
