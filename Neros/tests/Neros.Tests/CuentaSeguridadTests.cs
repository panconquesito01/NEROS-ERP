using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Neros.Application.Seguridad;
using Neros.Contracts.Autenticacion;
using Neros.Contracts.Cuenta;
using Neros.Persistence;
using Neros.Persistence.Seguridad;
using Xunit;

namespace Neros.Tests;

public sealed class CuentaSeguridadTests(EntornoPruebas entorno) : IClassFixture<EntornoPruebas>
{
    private static string ClaveNueva() => $"Nn9#{Guid.NewGuid():N}";

    [Fact]
    public async Task CambioDeClaveRevocaOtrasSesionesYRenuevaLaActualAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        using var actual = Cliente(await entorno.EntrarAsync(cuenta));
        using var otra = Cliente(await entorno.EntrarAsync(cuenta));
        var nueva = ClaveNueva();

        using var respuesta = await actual.PostAsJsonAsync("api/cuenta/clave", Solicitud(cuenta.Clave, nueva));
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var renovada = (await respuesta.Content.ReadFromJsonAsync<AccesoConcedido>())!;
        Assert.False(renovada.Usuario.DebeCambiarClave);

        Assert.Equal(HttpStatusCode.Unauthorized, await EstadoAsync(actual, "api/acceso/yo"));
        Assert.Equal(HttpStatusCode.Unauthorized, await EstadoAsync(otra, "api/acceso/yo"));
        using var renovado = Cliente(renovada);
        Assert.Equal(HttpStatusCode.OK, await EstadoAsync(renovado, "api/acceso/yo"));
        Assert.Single((await renovado.GetFromJsonAsync<List<SesionActiva>>("api/cuenta/sesiones"))!);

        using var anterior = await entorno.Http.PostAsJsonAsync("api/acceso/login", new SolicitudAcceso { Correo = cuenta.Correo, Clave = cuenta.Clave });
        Assert.Equal(HttpStatusCode.Unauthorized, anterior.StatusCode);
        await entorno.EntrarAsync(cuenta with { Clave = nueva });

        var eventos = await EventosAsync(cuenta.Id, "Cuenta.CambiarClave");
        var evento = Assert.Single(eventos);
        Assert.Equal("Correcto", evento.Resultado);
        Assert.Equal(cuenta.Id, evento.ActorId);
        Assert.DoesNotContain(eventos, e => ContieneSecreto(e, cuenta.Clave, nueva));
    }

    [Fact]
    public async Task CambioDeClaveRechazaDatosInvalidosSinCerrarSesionAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        using var cliente = Cliente(await entorno.EntrarAsync(cuenta));

        Assert.Equal(CodigosCuenta.ClaveActual, await ErrorAsync(cliente, Solicitud("Incorrecta#123456", ClaveNueva())));
        Assert.Equal(CodigosCuenta.MismaClave, await ErrorAsync(cliente, Solicitud(cuenta.Clave, cuenta.Clave)));
        Assert.Equal(CodigosCuenta.Politica, await ErrorAsync(cliente, Solicitud(cuenta.Clave, "soloenminusculas")));
        using var confirmacion = await cliente.PostAsJsonAsync("api/cuenta/clave",
            new SolicitudCambioClave { ClaveActual = cuenta.Clave, ClaveNueva = ClaveNueva(), Confirmacion = ClaveNueva() });
        Assert.Equal(HttpStatusCode.BadRequest, confirmacion.StatusCode);

        Assert.Equal(HttpStatusCode.OK, await EstadoAsync(cliente, "api/acceso/yo"));
        var rechazos = await EventosAsync(cuenta.Id, "Cuenta.CambiarClave");
        Assert.Equal(3, rechazos.Count);
        Assert.All(rechazos, evento => Assert.Equal("Rechazado", evento.Resultado));
    }

    [Fact]
    public async Task RestablecimientoEsExclusivoDelAdministradorYObligaACambiarAsync()
    {
        var administrador = await CrearAdministradorAsync();
        var afectado = await entorno.CrearCuentaAsync();
        using var admin = Cliente(await entorno.EntrarAsync(administrador));
        using var comun = Cliente(await entorno.EntrarAsync(afectado));

        Assert.Equal(HttpStatusCode.Forbidden, await EstadoAsync(comun, "api/admin/usuarios"));
        using (var prohibido = await comun.PostAsync($"api/admin/usuarios/{administrador.Id}/restablecer-clave", null))
            Assert.Equal(HttpStatusCode.Forbidden, prohibido.StatusCode);
        using (var propia = await admin.PostAsync($"api/admin/usuarios/{administrador.Id}/restablecer-clave", null))
        {
            Assert.Equal(HttpStatusCode.BadRequest, propia.StatusCode);
            Assert.Equal(CodigosCuenta.PropiaCuenta, (await propia.Content.ReadFromJsonAsync<ErrorOperacion>())!.Codigo);
        }
        using (var inexistente = await admin.PostAsync($"api/admin/usuarios/{Guid.NewGuid()}/restablecer-clave", null))
            Assert.Equal(HttpStatusCode.NotFound, inexistente.StatusCode);

        using var restablecida = await admin.PostAsync($"api/admin/usuarios/{afectado.Id}/restablecer-clave", null);
        Assert.Equal(HttpStatusCode.OK, restablecida.StatusCode);
        var temporal = (await restablecida.Content.ReadFromJsonAsync<ClaveTemporal>())!.Clave;
        Assert.Equal(20, temporal.Length);
        Assert.Equal(HttpStatusCode.Unauthorized, await EstadoAsync(comun, "api/acceso/yo"));

        var accesoTemporal = await entorno.EntrarAsync(afectado with { Clave = temporal });
        Assert.True(accesoTemporal.Usuario.DebeCambiarClave);
        using var conTemporal = Cliente(accesoTemporal);
        using (var bloqueada = await conTemporal.GetAsync("api/empresas"))
        {
            Assert.Equal(HttpStatusCode.Forbidden, bloqueada.StatusCode);
            Assert.Equal(CodigosCuenta.CambioClaveRequerido, (await bloqueada.Content.ReadFromJsonAsync<ErrorOperacion>())!.Codigo);
        }
        Assert.Equal(HttpStatusCode.Forbidden, await EstadoAsync(conTemporal, "api/cuenta/sesiones"));
        Assert.Equal(HttpStatusCode.OK, await EstadoAsync(conTemporal, "api/acceso/yo"));

        var definitiva = ClaveNueva();
        using var cambio = await conTemporal.PostAsJsonAsync("api/cuenta/clave", Solicitud(temporal, definitiva));
        Assert.Equal(HttpStatusCode.OK, cambio.StatusCode);
        using var normal = Cliente((await cambio.Content.ReadFromJsonAsync<AccesoConcedido>())!);
        Assert.Equal(HttpStatusCode.OK, await EstadoAsync(normal, "api/empresas"));
        Assert.False((await normal.GetFromJsonAsync<UsuarioActual>("api/acceso/yo"))!.DebeCambiarClave);

        var evento = Assert.Single(await EventosAsync(administrador.Id, "Usuario.RestablecerClave"));
        Assert.Equal(afectado.Id, evento.EntidadId);
        Assert.False(ContieneSecreto(evento, temporal, definitiva));
    }

    [Fact]
    public async Task AdministracionListaBuscaYDesbloqueaAsync()
    {
        var administrador = await CrearAdministradorAsync();
        var afectado = await entorno.CrearCuentaAsync();
        using var admin = Cliente(await entorno.EntrarAsync(administrador));
        await using (var scope = entorno.Api.Services.CreateAsyncScope())
        {
            var usuarios = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
            await usuarios.SetLockoutEndDateAsync((await usuarios.FindByIdAsync(afectado.Id))!, DateTimeOffset.UtcNow.AddMinutes(15));
        }

        var pagina = (await admin.GetFromJsonAsync<PaginaUsuarios>($"api/admin/usuarios?buscar={afectado.Correo[..20]}"))!;
        var encontrado = Assert.Single(pagina.Usuarios);
        Assert.Equal(afectado.Id, encontrado.Id);
        Assert.True(encontrado.Bloqueado);
        var todos = (await admin.GetFromJsonAsync<PaginaUsuarios>("api/admin/usuarios?pagina=1&tamano=1"))!;
        Assert.Single(todos.Usuarios);
        Assert.True(todos.Total >= 2);
        Assert.Equal(100, (await admin.GetFromJsonAsync<PaginaUsuarios>("api/admin/usuarios?tamano=5000"))!.Tamano);

        using var bloqueado = await entorno.Http.PostAsJsonAsync("api/acceso/login", new SolicitudAcceso { Correo = afectado.Correo, Clave = afectado.Clave });
        Assert.Equal(HttpStatusCode.Unauthorized, bloqueado.StatusCode);
        using (var desbloqueo = await admin.PostAsync($"api/admin/usuarios/{afectado.Id}/desbloquear", null))
            Assert.Equal(HttpStatusCode.NoContent, desbloqueo.StatusCode);
        using (var inexistente = await admin.PostAsync($"api/admin/usuarios/{Guid.NewGuid()}/desbloquear", null))
            Assert.Equal(HttpStatusCode.NotFound, inexistente.StatusCode);
        await entorno.EntrarAsync(afectado);
        Assert.Single(await EventosAsync(administrador.Id, "Usuario.Desbloquear"));
    }

    [Fact]
    public async Task SesionesPropiasSeListanYCierranSinTocarAjenasAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        var ajena = await entorno.CrearCuentaAsync();
        using var actual = Cliente(await entorno.EntrarAsync(cuenta));
        using var segunda = Cliente(await entorno.EntrarAsync(cuenta));
        using var otraCuenta = Cliente(await entorno.EntrarAsync(ajena));

        var sesiones = (await actual.GetFromJsonAsync<List<SesionActiva>>("api/cuenta/sesiones"))!;
        Assert.Equal(2, sesiones.Count);
        Assert.True(sesiones[0].Actual);
        Assert.Single(sesiones, sesion => sesion.Actual);
        var sesionAjena = (await otraCuenta.GetFromJsonAsync<List<SesionActiva>>("api/cuenta/sesiones"))!.Single();

        using (var cierreAjeno = await actual.PostAsync($"api/cuenta/sesiones/{sesionAjena.Id}/cerrar", null))
            Assert.Equal(HttpStatusCode.NotFound, cierreAjeno.StatusCode);
        Assert.Equal(HttpStatusCode.OK, await EstadoAsync(otraCuenta, "api/acceso/yo"));

        using (var cierre = await actual.PostAsync($"api/cuenta/sesiones/{sesiones.Single(s => !s.Actual).Id}/cerrar", null))
            Assert.Equal(HttpStatusCode.NoContent, cierre.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, await EstadoAsync(segunda, "api/acceso/yo"));

        using var tercera = Cliente(await entorno.EntrarAsync(cuenta));
        using (var otras = await actual.PostAsync("api/cuenta/sesiones/cerrar-otras", null))
            Assert.Equal(1, (await otras.Content.ReadFromJsonAsync<SesionesCerradas>())!.Cantidad);
        Assert.Equal(HttpStatusCode.Unauthorized, await EstadoAsync(tercera, "api/acceso/yo"));
        Assert.Equal(HttpStatusCode.OK, await EstadoAsync(actual, "api/acceso/yo"));

        using (var todas = await actual.PostAsync("api/cuenta/sesiones/cerrar-todas", null))
            Assert.Equal(1, (await todas.Content.ReadFromJsonAsync<SesionesCerradas>())!.Cantidad);
        Assert.Equal(HttpStatusCode.Unauthorized, await EstadoAsync(actual, "api/acceso/yo"));
        Assert.Equal(HttpStatusCode.OK, await EstadoAsync(otraCuenta, "api/acceso/yo"));
    }

    [Fact]
    public void CatalogoDePermisosEsConsistente()
    {
        Assert.All(Permisos.Todos, permiso => Assert.Matches(new Regex("^[A-Z]+\\.[A-Z_]+\\.[A-Z_]+$"), permiso));
        Assert.Equal(Permisos.Todos.Count(), Permisos.Todos.Distinct().Count());
        Assert.Empty(Permisos.DePlataforma(false));
        Assert.Contains(Permisos.UsuarioRestablecerClave, Permisos.DePlataforma(true));
        Assert.Contains(Permisos.EmpresaMiembroAdministrar, Permisos.DeRolEmpresa("Administrador"));
        Assert.Contains(Permisos.TerceroConsultar, Permisos.DeRolEmpresa("Administrador"));
        Assert.Contains(Permisos.TerceroCrear, Permisos.DeRolEmpresa("Operador"));
        Assert.DoesNotContain(Permisos.EmpresaMiembroAdministrar, Permisos.DeRolEmpresa("Operador"));
        Assert.Equal(
            [
                Permisos.EmpresaInicioConsultar, Permisos.TerceroConsultar, Permisos.ContabilidadComprobanteConsultar,
                Permisos.ContabilidadReporteConsultar,
                Permisos.InventarioMovimientoConsultar, Permisos.VentasCotizacionConsultar, Permisos.VentasPedidoConsultar,
                Permisos.ComprasOrdenConsultar, Permisos.ComprasRecepcionConsultar,
                Permisos.CarteraCxcConsultar, Permisos.CarteraCxpConsultar,
                Permisos.TesoreriaCuentaConsultar, Permisos.TesoreriaConciliacionConsultar,
                Permisos.FacturacionDocumentoConsultar, Permisos.FacturacionElectronicoConsultar,
                Permisos.NominaEmpleadoConsultar, Permisos.NominaConceptoConsultar,
                Permisos.NominaLiquidacionConsultar,
                Permisos.NominaLegalConsultar, Permisos.NominaElectronicoConsultar,
                Permisos.ActivosActivoConsultar,
                Permisos.PresupuestoVersionConsultar, Permisos.PresupuestoEjecucionConsultar,
                Permisos.ProduccionOrdenConsultar,
                Permisos.ProyectosProyectoConsultar,
                Permisos.AnaliticaIndicadorConsultar,
                Permisos.BusquedaIndiceConsultar
            ],
            Permisos.DeRolEmpresa("Consulta"));
        Assert.Contains(Permisos.FacturacionDocumentoEmitir, Permisos.DeRolEmpresa("Operador"));
        Assert.DoesNotContain(Permisos.FacturacionNumeracionConfigurar, Permisos.DeRolEmpresa("Operador"));
        Assert.Contains(Permisos.FacturacionNumeracionConfigurar, Permisos.DeRolEmpresa("Administrador"));
        Assert.Contains(Permisos.NominaLiquidacionCalcular, Permisos.DeRolEmpresa("Operador"));
        Assert.DoesNotContain(Permisos.NominaLiquidacionContabilizar, Permisos.DeRolEmpresa("Operador"));
        Assert.Contains(Permisos.NominaLiquidacionContabilizar, Permisos.DeRolEmpresa("Administrador"));
        Assert.DoesNotContain(Permisos.NominaConceptoConfigurar, Permisos.DeRolEmpresa("Operador"));
        Assert.Contains(Permisos.NominaElectronicoTransmitir, Permisos.DeRolEmpresa("Operador"));
        Assert.DoesNotContain(Permisos.NominaLegalAprobar, Permisos.DeRolEmpresa("Operador"));
        Assert.Contains(Permisos.NominaLegalAprobar, Permisos.DeRolEmpresa("Administrador"));
        Assert.Contains(Permisos.ActivosDepreciacionCalcular, Permisos.DeRolEmpresa("Operador"));
        Assert.DoesNotContain(Permisos.ActivosDepreciacionContabilizar, Permisos.DeRolEmpresa("Operador"));
        Assert.Contains(Permisos.ActivosDepreciacionContabilizar, Permisos.DeRolEmpresa("Administrador"));
        Assert.Contains(Permisos.PresupuestoVersionGestionar, Permisos.DeRolEmpresa("Operador"));
        Assert.DoesNotContain(Permisos.PresupuestoVersionAprobar, Permisos.DeRolEmpresa("Operador"));
        Assert.Contains(Permisos.PresupuestoVersionAprobar, Permisos.DeRolEmpresa("Administrador"));
        Assert.Contains(Permisos.ProduccionMovimientoRegistrar, Permisos.DeRolEmpresa("Operador"));
        Assert.DoesNotContain(Permisos.ProduccionOrdenLiberar, Permisos.DeRolEmpresa("Operador"));
        Assert.Contains(Permisos.ProduccionOrdenLiberar, Permisos.DeRolEmpresa("Administrador"));
        Assert.Contains(Permisos.AnaliticaIndicadorConfigurar, Permisos.DeRolEmpresa("Administrador"));
        Assert.DoesNotContain(Permisos.AnaliticaIndicadorConfigurar, Permisos.DeRolEmpresa("Operador"));
        Assert.Contains(Permisos.AnaliticaIndicadorConsultar, Permisos.DeRolEmpresa("Operador"));
        Assert.Contains(Permisos.BusquedaIndiceConsultar, Permisos.DeRolEmpresa("Operador"));
        Assert.Contains(Permisos.BusquedaIndiceReconstruir, Permisos.DeRolEmpresa("Administrador"));
        Assert.DoesNotContain(Permisos.BusquedaIndiceReconstruir, Permisos.DeRolEmpresa("Operador"));
        Assert.DoesNotContain(Permisos.TerceroCrear, Permisos.DeRolEmpresa("Consulta"));
        Assert.Contains(Permisos.ContabilidadPeriodoReabrir, Permisos.DeRolEmpresa("Administrador"));
        Assert.DoesNotContain(Permisos.ContabilidadPeriodoReabrir, Permisos.DeRolEmpresa("Operador"));
        Assert.Contains(Permisos.ComprasOrdenAprobar, Permisos.DeRolEmpresa("Administrador"));
        Assert.DoesNotContain(Permisos.ComprasOrdenAprobar, Permisos.DeRolEmpresa("Operador"));
        Assert.Empty(Permisos.DeRolEmpresa("administrador"));
    }

    [Fact]
    public void ClaveTemporalCumplePoliticaYNoRepite()
    {
        var claves = Enumerable.Range(0, 200).Select(_ => ServicioAdministracionUsuarios.GenerarClaveTemporal()).ToList();
        Assert.Equal(claves.Count, claves.Distinct().Count());
        Assert.All(claves, clave =>
        {
            Assert.Equal(20, clave.Length);
            Assert.Contains(clave, char.IsUpper);
            Assert.Contains(clave, char.IsLower);
            Assert.Contains(clave, char.IsDigit);
            Assert.Contains(clave, caracter => !char.IsLetterOrDigit(caracter));
            Assert.DoesNotContain(clave, caracter => "0O1lI".Contains(caracter));
        });
    }

    [Fact]
    [Trait("Categoria", "Navegador")]
    public async Task NavegadorMenuRestablecimientoCambioObligatorioYSesionesAsync()
    {
        var administrador = await CrearAdministradorAsync();
        var afectado = await entorno.CrearCuentaAsync();
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true, Channel = OperatingSystem.IsWindows() ? "msedge" : null
        });
        var opciones = new BrowserNewContextOptions
        {
            BaseURL = entorno.Web.BaseAddress!.ToString(), Locale = "es-ES", ViewportSize = new ViewportSize { Width = 1440, Height = 960 }
        };

        await using var contextoAfectado = await browser.NewContextAsync(opciones);
        var paginaAfectado = await contextoAfectado.NewPageAsync();
        await EntrarAsync(paginaAfectado, afectado.Correo, afectado.Clave);
        await paginaAfectado.WaitForURLAsync("**/empresas");
        await paginaAfectado.GotoAsync("/administracion/usuarios");
        Assert.DoesNotContain("/administracion", new Uri(paginaAfectado.Url).AbsolutePath);

        await using var contextoAdmin = await browser.NewContextAsync(opciones);
        var page = await contextoAdmin.NewPageAsync();
        page.Dialog += async (_, dialogo) => await dialogo.AcceptAsync();
        await EntrarAsync(page, administrador.Correo, administrador.Clave);
        await page.WaitForURLAsync("**/empresas");
        await page.Locator("[data-menu] summary").ClickAsync();
        await Assertions.Expect(page.Locator(".n-user-menu-head").GetByText(administrador.Correo)).ToBeVisibleAsync();
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(page.Locator("[data-menu]")).Not.ToHaveAttributeAsync("open", "");
        await page.Locator("[data-menu] summary").ClickAsync();
        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(AccesoMultiempresaTests.CarpetaCapturas(), "menu-usuario.png"), Animations = ScreenshotAnimations.Disabled
        });
        await page.GetByRole(AriaRole.Link, new() { Name = "Administrar usuarios" }).ClickAsync();
        await page.WaitForURLAsync("**/administracion/usuarios");
        await page.GetByRole(AriaRole.Searchbox).FillAsync(afectado.Correo);
        await page.GetByRole(AriaRole.Button, new() { Name = "Buscar", Exact = true }).ClickAsync();
        await page.WaitForURLAsync("**/administracion/usuarios?buscar=*");
        await page.GetByRole(AriaRole.Button, new() { Name = "Restablecer la contraseña de Ana (prueba)" }).ClickAsync();
        var temporal = (await page.Locator("#clave-temporal").TextContentAsync())!.Trim();
        Assert.Equal(20, temporal.Length);
        await AccesoMultiempresaTests.CapturarAsync(page, "usuarios");
        await page.SetViewportSizeAsync(1440, 960);
        await page.ReloadAsync();
        await Assertions.Expect(page.Locator("#clave-temporal")).ToHaveCountAsync(0);
        Assert.Single(await EventosAsync(administrador.Id, "Usuario.RestablecerClave"));

        await paginaAfectado.GotoAsync("/empresas");
        Assert.Contains("/login", paginaAfectado.Url);
        await EntrarAsync(paginaAfectado, afectado.Correo, temporal);
        await paginaAfectado.WaitForURLAsync("**/cuenta/clave");
        await Assertions.Expect(paginaAfectado.GetByRole(AriaRole.Status)).ToContainTextAsync("Un administrador restableció tu contraseña");
        await paginaAfectado.GotoAsync("/empresas");
        await paginaAfectado.WaitForURLAsync("**/cuenta/clave");
        await AccesoMultiempresaTests.CapturarAsync(paginaAfectado, "cambio-clave");
        await paginaAfectado.SetViewportSizeAsync(1440, 960);
        var definitiva = ClaveNueva();
        await paginaAfectado.GetByLabel("Contraseña actual", new() { Exact = true }).FillAsync(temporal);
        await paginaAfectado.GetByLabel("Nueva contraseña", new() { Exact = true }).FillAsync(definitiva);
        await paginaAfectado.GetByLabel("Confirmar nueva contraseña", new() { Exact = true }).FillAsync(definitiva);
        await paginaAfectado.GetByRole(AriaRole.Button, new() { Name = "Guardar contraseña" }).ClickAsync();
        await paginaAfectado.WaitForURLAsync("**/cuenta/clave?estado=cambiada");
        await paginaAfectado.GetByRole(AriaRole.Link, new() { Name = "Continuar" }).ClickAsync();
        await paginaAfectado.WaitForURLAsync(NavegacionPruebas.DestinoTrasLogin(), new() { Timeout = 30_000 });
        await NavegacionPruebas.CompletarLegalesSiPendientesAsync(paginaAfectado);
        await paginaAfectado.WaitForURLAsync("**/empresas");

        await page.Locator("[data-menu] summary").ClickAsync();
        await page.GetByRole(AriaRole.Link, new() { Name = "Mis sesiones" }).ClickAsync();
        await page.WaitForURLAsync("**/cuenta/sesiones");
        await Assertions.Expect(page.GetByText("Esta sesión")).ToBeVisibleAsync();
        await AccesoMultiempresaTests.CapturarAsync(page, "sesiones");
        await page.SetViewportSizeAsync(1440, 960);
        await page.GetByRole(AriaRole.Button, new() { Name = "Cerrar todas" }).ClickAsync();
        await page.WaitForURLAsync("**/login?estado=salida");
    }

    private async Task<CuentaPrueba> CrearAdministradorAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        await using var scope = entorno.Api.Services.CreateAsyncScope();
        var usuarios = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        var usuario = (await usuarios.FindByIdAsync(cuenta.Id))!;
        usuario.Nombre = "Admin (prueba)";
        Assert.True((await usuarios.UpdateAsync(usuario)).Succeeded);
        Assert.True((await usuarios.AddClaimAsync(usuario, new Claim(ClaimTypes.Role, Permisos.RolAdministradorGlobal))).Succeeded);
        return cuenta;
    }

    private static Task EntrarAsync(IPage page, string correo, string clave) => NavegacionPruebas.EntrarAsync(page, correo, clave);

    private async Task<List<EventoAuditoria>> EventosAsync(string actorId, string accion)
    {
        await using var scope = entorno.Api.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<NerosDbContext>();
        return await database.EventosAuditoria.AsNoTracking().Where(evento => evento.ActorId == actorId && evento.Accion == accion).ToListAsync();
    }

    private static bool ContieneSecreto(EventoAuditoria evento, params string[] secretos) =>
        secretos.Any(secreto => (evento.Detalle ?? string.Empty).Contains(secreto, StringComparison.Ordinal)
            || (evento.EntidadId ?? string.Empty).Contains(secreto, StringComparison.Ordinal));

    private static SolicitudCambioClave Solicitud(string actual, string nueva) =>
        new() { ClaveActual = actual, ClaveNueva = nueva, Confirmacion = nueva };

    private static async Task<string?> ErrorAsync(HttpClient cliente, SolicitudCambioClave solicitud)
    {
        using var respuesta = await cliente.PostAsJsonAsync("api/cuenta/clave", solicitud);
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        return (await respuesta.Content.ReadFromJsonAsync<ErrorOperacion>())?.Codigo;
    }

    private static async Task<HttpStatusCode> EstadoAsync(HttpClient cliente, string ruta)
    {
        using var respuesta = await cliente.GetAsync(ruta);
        return respuesta.StatusCode;
    }

    private HttpClient Cliente(AccesoConcedido acceso)
    {
        var cliente = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = entorno.Http.BaseAddress };
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", acceso.Token);
        return cliente;
    }
}
