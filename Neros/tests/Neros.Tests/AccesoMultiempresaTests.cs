using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Neros.Contracts.Autenticacion;
using Neros.Persistence;
using Neros.Persistence.Seguridad;
using Xunit;

namespace Neros.Tests;

public sealed class AccesoMultiempresaTests(EntornoPruebas entorno) : IClassFixture<EntornoPruebas>
{
    [Fact]
    public async Task AdministradorGlobalRespetaEstadoYRevocacionAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        using var cliente = ClienteAutorizado(await entorno.EntrarAsync(cuenta));
        await using var scope = entorno.Api.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<NerosDbContext>();
        var usuarios = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        var usuario = (await usuarios.FindByIdAsync(cuenta.Id))!;
        var permiso = new Claim(ClaimTypes.Role, "AdministradorGlobal");
        Assert.True((await usuarios.AddClaimAsync(usuario, permiso)).Succeeded);
        var empresa = new Empresa { Codigo = Guid.NewGuid().ToString("N")[..20], Nombre = "Empresa nueva (prueba)" };
        database.Empresas.Add(empresa);
        await database.SaveChangesAsync();
        try
        {
            var empresas = await cliente.GetFromJsonAsync<List<EmpresaDisponible>>("api/empresas");
            Assert.Contains(empresas!, disponible => disponible.Id == empresa.Id && disponible.Rol == "Administrador global");
            using var seleccion = await cliente.PostAsync($"api/empresas/{empresa.Id}/seleccionar", null);
            Assert.Equal(HttpStatusCode.OK, seleccion.StatusCode);
            var inicio = await cliente.GetFromJsonAsync<InicioEmpresa>($"api/empresas/{empresa.Id}/inicio");
            Assert.Single(inicio!.Actividad);
            empresa.Activa = false;
            await database.SaveChangesAsync();
            using var inactiva = await cliente.GetAsync($"api/empresas/{empresa.Id}/inicio");
            Assert.Equal(HttpStatusCode.NotFound, inactiva.StatusCode);
            empresa.Activa = true;
            await database.SaveChangesAsync();
            usuario.Activo = false;
            await database.SaveChangesAsync();
            using var bloqueado = await cliente.GetAsync($"api/empresas/{empresa.Id}/inicio");
            Assert.Equal(HttpStatusCode.Unauthorized, bloqueado.StatusCode);
            usuario.Activo = true;
            await database.SaveChangesAsync();
            Assert.True((await usuarios.RemoveClaimAsync(usuario, permiso)).Succeeded);
            using var revocada = await cliente.PostAsync($"api/empresas/{empresa.Id}/seleccionar", null);
            Assert.Equal(HttpStatusCode.NotFound, revocada.StatusCode);
        }
        finally
        {
            await database.EventosAcceso.Where(evento => evento.EmpresaId == empresa.Id).ExecuteDeleteAsync();
            await database.Empresas.Where(entidad => entidad.Id == empresa.Id).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task SoloEmpresasAutorizadasAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        var acceso = await entorno.EntrarAsync(cuenta);
        using var cliente = ClienteAutorizado(acceso);
        var empresas = await cliente.GetFromJsonAsync<List<EmpresaDisponible>>("api/empresas");
        Assert.Equal(2, empresas!.Count);
        Assert.DoesNotContain(empresas, empresa => empresa.Id == entorno.EmpresaAjena);
        using var propia = await cliente.GetAsync($"api/empresas/{entorno.EmpresaPrimera}/inicio");
        Assert.Equal(HttpStatusCode.OK, propia.StatusCode);
        using var ajena = await cliente.GetAsync($"api/empresas/{entorno.EmpresaAjena}/inicio");
        Assert.Equal(HttpStatusCode.NotFound, ajena.StatusCode);
        using var seleccionAjena = await cliente.PostAsync($"api/empresas/{entorno.EmpresaAjena}/seleccionar", null);
        Assert.Equal(HttpStatusCode.NotFound, seleccionAjena.StatusCode);
    }

    [Fact]
    public async Task AccesoAnonimoYDatosInvalidosAsync()
    {
        using var respuesta = await entorno.Http.GetAsync("api/empresas");
        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        using var invalida = await entorno.Http.PostAsJsonAsync("api/acceso/login", new SolicitudAcceso());
        Assert.Equal(HttpStatusCode.BadRequest, invalida.StatusCode);
        using var navegadorAnonimo = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = entorno.Web.BaseAddress };
        using var inicio = await navegadorAnonimo.GetAsync("home");
        Assert.Equal(HttpStatusCode.Redirect, inicio.StatusCode);
        Assert.Contains("/login", inicio.Headers.Location!.ToString());
    }

    [Fact]
    public async Task RevocacionDeMembresiaImpideAccesoAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        using var cliente = ClienteAutorizado(await entorno.EntrarAsync(cuenta));
        await using var scope = entorno.Api.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<NerosDbContext>();
        var membresia = await database.UsuariosEmpresas.FindAsync(cuenta.Id, entorno.EmpresaPrimera);
        membresia!.Activo = false;
        await database.SaveChangesAsync();
        using var respuesta = await cliente.GetAsync($"api/empresas/{entorno.EmpresaPrimera}/inicio");
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Single((await cliente.GetFromJsonAsync<List<EmpresaDisponible>>("api/empresas"))!);
    }

    [Fact]
    public async Task EmpresaInactivaNoEsAccesibleAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        using var cliente = ClienteAutorizado(await entorno.EntrarAsync(cuenta));
        await using var scope = entorno.Api.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<NerosDbContext>();
        var empresa = new Empresa { Codigo = Guid.NewGuid().ToString("N")[..20], Nombre = "Inactiva", Activa = false };
        database.Empresas.Add(empresa);
        database.UsuariosEmpresas.Add(new UsuarioEmpresa { UsuarioId = cuenta.Id, EmpresaId = empresa.Id });
        await database.SaveChangesAsync();
        using var respuesta = await cliente.GetAsync($"api/empresas/{empresa.Id}/inicio");
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task LogoutRevocaElTokenAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        using var cliente = ClienteAutorizado(await entorno.EntrarAsync(cuenta));
        using var salida = await cliente.PostAsync("api/acceso/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, salida.StatusCode);
        using var respuesta = await cliente.GetAsync("api/acceso/yo");
        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task CambioDeSelloYExpiracionRevocanSesionAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        var acceso = await entorno.EntrarAsync(cuenta, persistente: true);
        Assert.InRange(acceso.Expira, DateTimeOffset.UtcNow.AddDays(6), DateTimeOffset.UtcNow.AddDays(8));
        using var cliente = ClienteAutorizado(acceso);
        await using var scope = entorno.Api.Services.CreateAsyncScope();
        var usuarios = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        await usuarios.UpdateSecurityStampAsync((await usuarios.FindByIdAsync(cuenta.Id))!);
        using var revocada = await cliente.GetAsync("api/acceso/yo");
        Assert.Equal(HttpStatusCode.Unauthorized, revocada.StatusCode);
        using var nuevoCliente = ClienteAutorizado(await entorno.EntrarAsync(cuenta));
        var database = scope.ServiceProvider.GetRequiredService<NerosDbContext>();
        await database.Sesiones.Where(sesion => sesion.UsuarioId == cuenta.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(sesion => sesion.Expira, DateTimeOffset.UtcNow.AddMinutes(-1)));
        using var vencida = await nuevoCliente.GetAsync("api/acceso/yo");
        Assert.Equal(HttpStatusCode.Unauthorized, vencida.StatusCode);
    }

    [Fact]
    public async Task BloqueoTrasCincoIntentosAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        for (var intento = 0; intento < 5; intento++)
        {
            using var fallido = await entorno.Http.PostAsJsonAsync("api/acceso/login", new SolicitudAcceso { Correo = cuenta.Correo, Clave = "incorrecta" });
            Assert.Equal(HttpStatusCode.Unauthorized, fallido.StatusCode);
        }
        using var bloqueada = await entorno.Http.PostAsJsonAsync("api/acceso/login", new SolicitudAcceso { Correo = cuenta.Correo, Clave = cuenta.Clave });
        Assert.Equal(HttpStatusCode.Unauthorized, bloqueada.StatusCode);
    }

    [Fact]
    public async Task ActividadEsPersonalYPorEmpresaAsync()
    {
        var primera = await entorno.CrearCuentaAsync();
        var segunda = await entorno.CrearCuentaAsync();
        using var clientePrimero = ClienteAutorizado(await entorno.EntrarAsync(primera));
        using var clienteSegundo = ClienteAutorizado(await entorno.EntrarAsync(segunda));
        using var seleccion = await clientePrimero.PostAsync($"api/empresas/{entorno.EmpresaPrimera}/seleccionar", null);
        Assert.Equal(HttpStatusCode.OK, seleccion.StatusCode);
        var propia = await clientePrimero.GetFromJsonAsync<InicioEmpresa>($"api/empresas/{entorno.EmpresaPrimera}/inicio");
        Assert.Single(propia!.Actividad);
        var otraCuenta = await clienteSegundo.GetFromJsonAsync<InicioEmpresa>($"api/empresas/{entorno.EmpresaPrimera}/inicio");
        Assert.Empty(otraCuenta!.Actividad);
        var otraEmpresa = await clientePrimero.GetFromJsonAsync<InicioEmpresa>($"api/empresas/{entorno.EmpresaSegunda}/inicio");
        Assert.Empty(otraEmpresa!.Actividad);
    }

    [Fact]
    [Trait("Categoria", "Navegador")]
    public async Task NavegadorLoginEmpresasTemaYSalidaAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true, Channel = OperatingSystem.IsWindows() ? "msedge" : null
        });
        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = entorno.Web.BaseAddress!.ToString(), Locale = "es-ES", ViewportSize = new ViewportSize { Width = 1440, Height = 960 }, ColorScheme = ColorScheme.Dark
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync("/login");
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-theme", "dark");
        await CapturarAsync(page, "login");
        await page.SetViewportSizeAsync(1440, 960);
        await page.EvaluateAsync("() => document.documentElement.dataset.documentoPrueba = 'persistente'");
        var urlAcceso = page.Url;
        foreach (var correo in new[] { $"inexistente-{Guid.NewGuid():N}@example.invalid", cuenta.Correo })
        {
            await page.GetByLabel("Correo electrónico", new() { Exact = true }).FillAsync(correo);
            await page.GetByLabel("Contraseña", new() { Exact = true }).FillAsync(Guid.NewGuid().ToString("N"));
            var rechazo = await page.RunAndWaitForResponseAsync(
                () => page.GetByRole(AriaRole.Button, new() { Name = "Entrar a Neros" }).ClickAsync(),
                respuesta => respuesta.Url.EndsWith("/sesion/entrar", StringComparison.Ordinal) && respuesta.Request.Method == "POST");
            Assert.Equal(401, rechazo.Status);
            await Assertions.Expect(page.GetByRole(AriaRole.Alert)).ToContainTextAsync("El correo o la contraseña son incorrectos.");
            await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Entrar a Neros" })).ToBeEnabledAsync();
            await Assertions.Expect(page.Locator("#correo")).ToHaveValueAsync(correo);
            Assert.Equal(urlAcceso, page.Url);
            Assert.Equal("persistente", await page.EvaluateAsync<string>("() => document.documentElement.dataset.documentoPrueba"));
        }
        await CapturarAsync(page, "login-error");
        await page.SetViewportSizeAsync(1440, 960);
        await page.RouteAsync("**/sesion/entrar", ruta => ruta.AbortAsync());
        await page.GetByRole(AriaRole.Button, new() { Name = "Entrar a Neros" }).ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Alert)).ToContainTextAsync("Comprueba tu conexión");
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Entrar a Neros" })).ToBeEnabledAsync();
        Assert.Equal("persistente", await page.EvaluateAsync<string>("() => document.documentElement.dataset.documentoPrueba"));
        await page.UnrouteAsync("**/sesion/entrar");

        var csrfAcceso = await context.APIRequest.PostAsync("/sesion/entrar", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { ["Accept"] = "application/json" }
        });
        Assert.Equal(400, csrfAcceso.Status);
        Assert.Contains("\"solicitud\"", await csrfAcceso.TextAsync());
        await page.GetByLabel("Correo electrónico", new() { Exact = true }).FillAsync(cuenta.Correo);
        await page.GetByLabel("Contraseña", new() { Exact = true }).FillAsync(cuenta.Clave);
        await page.GetByRole(AriaRole.Button, new() { Name = "Mostrar contraseña", Exact = true }).ClickAsync();
        await Assertions.Expect(page.Locator("#clave")).ToHaveAttributeAsync("type", "text");
        await page.GetByRole(AriaRole.Button, new() { Name = "Ocultar contraseña", Exact = true }).ClickAsync();
        await page.GetByLabel("Mantener sesión", new() { Exact = true }).CheckAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Entrar a Neros" }).ClickAsync();
        await page.WaitForURLAsync("**/empresas");
        await Assertions.Expect(page.Locator("[data-company-row]")).ToHaveCountAsync(2);
        await Assertions.Expect(page.GetByRole(AriaRole.Searchbox)).ToHaveCountAsync(0);
        await CapturarAsync(page, "empresas");
        var csrf = await context.APIRequest.PostAsync("/sesion/empresa", new APIRequestContextOptions
        {
            Data = await new FormUrlEncodedContent(new Dictionary<string, string> { ["EmpresaId"] = entorno.EmpresaPrimera.ToString() }).ReadAsStringAsync(),
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/x-www-form-urlencoded" }
        });
        Assert.Equal(400, csrf.Status);
        await page.GetByRole(AriaRole.Button, new() { Name = "Entrar a Neros Distribucion (prueba)", Exact = true }).ClickAsync();
        await page.WaitForURLAsync("**/home");
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Neros Distribucion (prueba)", Exact = true })).ToBeVisibleAsync();
        await CapturarAsync(page, "inicio");
        await page.GetByRole(AriaRole.Link, new() { Name = "Cambiar empresa", Exact = true }).ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Continuar en Neros Distribucion (prueba), empresa activa", Exact = true })).ToBeVisibleAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Entrar a Neros Servicios (prueba)", Exact = true }).ClickAsync();
        await page.WaitForURLAsync("**/home");
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Neros Servicios (prueba)", Exact = true })).ToBeVisibleAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Tema del sistema", Exact = true }).ClickAsync();
        await page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Light, ReducedMotion = ReducedMotion.Reduce });
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-theme", "light");
        await page.GetByRole(AriaRole.Button, new() { Name = "Cerrar sesión", Exact = true }).ClickAsync();
        await page.WaitForURLAsync("**/login?estado=salida");
        await page.GotoAsync("/home");
        Assert.Contains("/login", page.Url);

        await using var contextoBasico = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = entorno.Web.BaseAddress!.ToString(), Locale = "es-ES", JavaScriptEnabled = false
        });
        var paginaBasica = await contextoBasico.NewPageAsync();
        await paginaBasica.GotoAsync("/login");
        await paginaBasica.GetByLabel("Correo electrónico", new() { Exact = true }).FillAsync($"inexistente-{Guid.NewGuid():N}@example.invalid");
        await paginaBasica.GetByLabel("Contraseña", new() { Exact = true }).FillAsync(Guid.NewGuid().ToString("N"));
        await paginaBasica.GetByRole(AriaRole.Button, new() { Name = "Entrar a Neros" }).ClickAsync();
        await paginaBasica.WaitForURLAsync("**/login?estado=credenciales");
        await Assertions.Expect(paginaBasica.GetByRole(AriaRole.Alert)).ToContainTextAsync("El correo o la contraseña son incorrectos.");

        var limiteAlcanzado = false;
        for (var intento = 0; intento < 11; intento++)
        {
            var respuestaLimite = await context.APIRequest.PostAsync("/sesion/entrar", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string> { ["Accept"] = "application/json" }, MaxRedirects = 0
            });
            if (respuestaLimite.Status == 429)
            {
                Assert.Contains("\"intentos\"", await respuestaLimite.TextAsync());
                Assert.False(respuestaLimite.Headers.ContainsKey("location"));
                limiteAlcanzado = true;
                break;
            }
            Assert.Equal(400, respuestaLimite.Status);
        }
        Assert.True(limiteAlcanzado);
    }

    [Fact]
    [Trait("Categoria", "Navegador")]
    public async Task NavegadorEligeYConservaIdiomaAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true, Channel = OperatingSystem.IsWindows() ? "msedge" : null
        });
        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = entorno.Web.BaseAddress!.ToString(), Locale = "en-US", ViewportSize = new ViewportSize { Width = 1440, Height = 960 }
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync("/login?ReturnUrl=%2Fempresas");
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("lang", "en");
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync("Welcome to Neros.");
        await page.GetByRole(AriaRole.Button, new() { Name = "Show password", Exact = true }).ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Hide password", Exact = true })).ToBeVisibleAsync();
        await CapturarAsync(page, "login-en", ("Light theme", "Dark theme"));

        await page.SetViewportSizeAsync(1440, 960);
        await page.Locator("[data-language-menu] summary").ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "English" })).ToHaveAttributeAsync("aria-current", "true");
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(page.Locator("[data-language-menu]")).Not.ToHaveAttributeAsync("open", "");
        await page.Locator("[data-language-menu] summary").ClickAsync();
        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(CarpetaCapturas(), "login-en-idiomas.png"), Animations = ScreenshotAnimations.Disabled
        });
        await page.GetByRole(AriaRole.Button, new() { Name = "Português" }).ClickAsync();
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("lang", "pt");
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync("Bem-vindo ao Neros.");
        Assert.EndsWith("/login?ReturnUrl=%2Fempresas", page.Url);

        await page.Locator("[data-language-menu] summary").ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Español" }).ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync("Bienvenido a Neros.");
        await page.GetByLabel("Correo electrónico", new() { Exact = true }).FillAsync(cuenta.Correo);
        await page.GetByLabel("Contraseña", new() { Exact = true }).FillAsync(cuenta.Clave);
        await page.GetByRole(AriaRole.Button, new() { Name = "Entrar a Neros" }).ClickAsync();
        await page.WaitForURLAsync("**/empresas");
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Mis empresas", Level = 1 })).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Entrar a Neros Distribucion (prueba)", Exact = true })).ToBeVisibleAsync();

        var idiomaSinToken = await context.APIRequest.PostAsync("/idioma", new APIRequestContextOptions
        {
            Data = "Cultura=en&Volver=%2Flogin",
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/x-www-form-urlencoded" }
        });
        Assert.Equal(400, idiomaSinToken.Status);
    }

    private HttpClient ClienteAutorizado(AccesoConcedido acceso)
    {
        var cliente = entorno.Api.CreateClient();
        cliente.BaseAddress = entorno.Http.BaseAddress;
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", acceso.Token);
        return cliente;
    }

    private static string CarpetaCapturas()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "Neros.slnx"))) root = root.Parent;
        return Directory.CreateDirectory(Path.Combine(root!.FullName, ".impeccable", "review")).FullName;
    }

    private static async Task CapturarAsync(IPage page, string vista, (string Claro, string Oscuro)? temas = null)
    {
        var (temaClaro, temaOscuro) = temas ?? ("Tema claro", "Tema oscuro");
        var carpeta = CarpetaCapturas();
        foreach (var ancho in new[] { 1440, 390 })
        {
            await page.SetViewportSizeAsync(ancho, ancho == 1440 ? 960 : 844);
            foreach (var (tema, etiqueta) in new[] { ("claro", temaClaro), ("oscuro", temaOscuro) })
            {
                await page.GetByRole(AriaRole.Button, new() { Name = etiqueta, Exact = true }).ClickAsync();
                await page.EvaluateAsync("() => document.fonts.ready");
                Assert.False(await page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > innerWidth"));
                var icono = await page.Locator(".n-icon").First.EvaluateAsync<string>("element => getComputedStyle(element).maskImage");
                Assert.Contains("/icons/", icono);
                await page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = Path.Combine(carpeta, $"{vista}-{ancho}-{tema}.png"), FullPage = true, Animations = ScreenshotAnimations.Disabled
                });
            }
        }
    }
}