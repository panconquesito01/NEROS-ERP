using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Localization;
using Neros.Blazor.Localizacion;
using Neros.Contracts.Autenticacion;
using Neros.Contracts.Cuenta;
using Neros.Contracts.Privacidad;
using Neros.Application.Seguridad;
using Neros.Contracts.Seguridad;

namespace Neros.Blazor.Servicios;

public static class EndpointsSesion
{
    public const string TokenClaim = "neros:sesion";
    public const string EmpresaClaim = "neros:empresa";
    public const string NombreEmpresaClaim = "neros:empresa-nombre";
    public const string EmpresaLogoClaim = "neros:empresa-tiene-logo";
    public const string ModuloClaim = "neros:modulo";
    public const string RolEmpresaClaim = "neros:empresa-rol";
    public const string DebeCambiarClaveClaim = "neros:debe-cambiar-clave";
    public const string RutaCambioClave = "/cuenta/clave";
    public const string LegalPendienteClaim = "neros:legal-pendiente";
    public const string LegalDocClaim = "neros:legal-doc";
    public const string RutaAceptacionLegal = "/legal/aceptar";

    public static bool TieneEmpresaSeleccionada(ClaimsPrincipal usuario) =>
        Guid.TryParse(usuario.FindFirstValue(EmpresaClaim), out _);

    public static void MapearSesion(this WebApplication app)
    {
        app.MapPost("/sesion/entrar", EntrarAsync).AllowAnonymous().RequireRateLimiting("login");
        app.MapPost("/sesion/empresa", SeleccionarAsync).RequireAuthorization();
        app.MapPost("/sesion/salir", SalirAsync).RequireAuthorization();
        app.MapPost("/sesion/clave", CambiarClaveAsync).RequireAuthorization();
        app.MapPost("/sesion/sesiones/cerrar", CerrarSesionAsync).RequireAuthorization();
        app.MapPost("/sesion/sesiones/cerrar-otras", CerrarOtrasAsync).RequireAuthorization();
        app.MapPost("/sesion/sesiones/cerrar-todas", CerrarTodasAsync).RequireAuthorization();
        app.MapPost("/sesion/legal/aceptar", AceptarLegalesAsync).RequireAuthorization();
        app.MapGet("/empresas/{empresaId:guid}/logo", LogoEmpresaAsync).RequireAuthorization();
    }

    /// <summary>Claims derivados del usuario. Se recalculan en cada peticion desde <c>api/acceso/yo</c>.</summary>
    public static IEnumerable<Claim> ClaimsUsuario(UsuarioActual usuario)
    {
        yield return new Claim(ClaimTypes.NameIdentifier, usuario.Id);
        yield return new Claim(ClaimTypes.Name, usuario.Nombre);
        yield return new Claim(ClaimTypes.Email, usuario.Correo);
        if (usuario.DebeCambiarClave) yield return new Claim(DebeCambiarClaveClaim, "true");
        if (usuario.DocumentosLegalesPendientes is { Count: > 0 })
        {
            yield return new Claim(LegalPendienteClaim, "true");
            foreach (var codigo in usuario.DocumentosLegalesPendientes) yield return new Claim(LegalDocClaim, codigo);
        }
        foreach (var permiso in usuario.Permisos ?? []) yield return new Claim(CodigosPermiso.Claim, permiso);
    }

    public static ClaimsPrincipal ActualizarPrincipal(ClaimsPrincipal actual, UsuarioActual usuario)
    {
        var claims = actual.Claims.Where(claim => claim.Type is TokenClaim or EmpresaClaim or NombreEmpresaClaim or EmpresaLogoClaim or ModuloClaim or RolEmpresaClaim).ToList();
        claims.AddRange(ClaimsUsuario(usuario));
        var rolEmpresa = actual.FindFirstValue(RolEmpresaClaim);
        if (!string.IsNullOrWhiteSpace(rolEmpresa))
        {
            claims = Permisos.ActualizarClaimsEmpresa(claims, rolEmpresa);
        }
        else if (actual.HasClaim(c => c.Type == EmpresaClaim))
        {
            var plataforma = Permisos.Plataforma.ToHashSet(StringComparer.Ordinal);
            var presentes = claims.Where(c => c.Type == CodigosPermiso.Claim).Select(c => c.Value).ToHashSet(StringComparer.Ordinal);
            foreach (var permiso in actual.Claims.Where(c => c.Type == CodigosPermiso.Claim && !plataforma.Contains(c.Value)))
            {
                if (presentes.Add(permiso.Value))
                    claims.Add(permiso);
            }
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    /// <summary>Con clave temporal solo quedan disponibles el cambio de clave, la salida y los recursos estaticos.</summary>
    public static IApplicationBuilder UseCambioClaveObligatorio(this IApplicationBuilder app) => app.Use(async (context, next) =>
    {
        if (context.User.HasClaim(DebeCambiarClaveClaim, "true") && !PermitidaConClaveTemporal(context.Request.Path))
        {
            context.Response.Redirect(RutaCambioClave);
            return;
        }
        await next(context);
    });

    public static IApplicationBuilder UseDocumentosLegalesObligatorios(this IApplicationBuilder app) => app.Use(async (context, next) =>
    {
        if (context.User.HasClaim(DebeCambiarClaveClaim, "true")) { await next(context); return; }
        if (context.User.HasClaim(LegalPendienteClaim, "true") && !PermitidaConLegalPendiente(context.Request.Path))
        {
            context.Response.Redirect(RutaAceptacionLegal);
            return;
        }
        await next(context);
    });

    private static readonly string[] RutasConClaveTemporal =
        [RutaCambioClave, "/sesion/clave", "/sesion/salir", "/sesion/sesiones/cerrar-todas", "/idioma", "/cookies", "/not-found", "/Error", "/_framework", "/_blazor", "/_content"];

    private static readonly string[] RutasConLegalPendiente =
        [RutaAceptacionLegal, "/sesion/legal/aceptar", "/sesion/salir", "/idioma", "/cookies", "/not-found", "/Error", "/_framework", "/_blazor", "/_content"];

    private static bool PermitidaConClaveTemporal(PathString ruta) =>
        Path.HasExtension(ruta.Value) || RutasConClaveTemporal.Any(permitida => ruta.StartsWithSegments(permitida, StringComparison.OrdinalIgnoreCase));

    private static bool PermitidaConLegalPendiente(PathString ruta) =>
        Path.HasExtension(ruta.Value) || RutasConLegalPendiente.Any(permitida => ruta.StartsWithSegments(permitida, StringComparison.OrdinalIgnoreCase));

    private static async Task<IResult> EntrarAsync(HttpContext context, IAntiforgery antiforgery, ClienteNeros cliente, ILogger<ClienteNeros> logger)
    {
        if (!await ValidarFormularioAsync(context, antiforgery))
        {
            return RechazarAcceso(context, "solicitud", StatusCodes.Status400BadRequest);
        }
        var form = await context.Request.ReadFormAsync(context.RequestAborted);
        var solicitud = new SolicitudAcceso
        {
            Correo = form["Correo"].ToString().Trim(),
            Clave = form["Clave"].ToString(),
            Recordarme = form["Recordarme"] == "true"
        };
        if (!Validator.TryValidateObject(solicitud, new ValidationContext(solicitud), null, true))
        {
            return RechazarAcceso(context, "datos", StatusCodes.Status400BadRequest);
        }
        try
        {
            var acceso = await cliente.IniciarAsync(solicitud, context.RequestAborted);
            if (acceso is null)
            {
                return RechazarAcceso(context, "credenciales", StatusCodes.Status401Unauthorized);
            }
            List<Claim> claims = [.. ClaimsUsuario(acceso.Usuario), new Claim(TokenClaim, acceso.Token)];
            var tieneLegales = acceso.Usuario.DocumentosLegalesPendientes is { Count: > 0 };
            var empresaUnica = acceso.Usuario.DebeCambiarClave || tieneLegales
                ? null
                : await SeleccionarEmpresaUnicaAsync(cliente, acceso.Token, logger, context.RequestAborted);
            if (empresaUnica is not null)
            {
                claims = Permisos.ActualizarClaimsEmpresa(claims, empresaUnica.Rol);
                AgregarClaimsEmpresa(claims, empresaUnica);
            }
            await context.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)), new AuthenticationProperties
            {
                IsPersistent = solicitud.Recordarme, ExpiresUtc = acceso.Expira, AllowRefresh = false
            });
            var destino = acceso.Usuario.DebeCambiarClave ? RutaCambioClave
                : tieneLegales ? RutaAceptacionLegal
                : empresaUnica is null ? "/empresas" : DestinoLocal(form["Destino"]) ?? "/home";
            return SolicitaJson(context) ? Results.Json(new { destino }) : Results.LocalRedirect(destino);
        }
        catch (HttpRequestException error)
        {
            logger.LogWarning("Acceso: fallo de comunicacion con API. Tipo {Tipo}; estado HTTP {Estado}",
                error.HttpRequestError, error.StatusCode);
            if (error.HttpRequestError == HttpRequestError.ConnectionError)
            {
                return RechazarAcceso(context, "conexion", StatusCodes.Status503ServiceUnavailable);
            }
            return error.StatusCode == HttpStatusCode.TooManyRequests
                ? RechazarAcceso(context, "intentos", StatusCodes.Status429TooManyRequests)
                : RechazarAcceso(context, "servicio", StatusCodes.Status503ServiceUnavailable);
        }
        catch (TaskCanceledException) when (!context.RequestAborted.IsCancellationRequested)
        {
            logger.LogWarning("Acceso: la API excedio el tiempo de respuesta permitido");
            return RechazarAcceso(context, "servicio", StatusCodes.Status504GatewayTimeout);
        }
    }

    private static async Task<EmpresaDisponible?> SeleccionarEmpresaUnicaAsync(ClienteNeros cliente, string token, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            var empresas = await cliente.EmpresasAsync(token, cancellationToken);
            return empresas.Count == 1 ? await cliente.SeleccionarAsync(token, empresas[0].Id, cancellationToken) : null;
        }
        catch (HttpRequestException error)
        {
            logger.LogWarning("Acceso: no se pudo preseleccionar la empresa. Estado HTTP {Estado}", error.StatusCode);
            return null;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Acceso: la preseleccion de empresa excedio el tiempo de respuesta");
            return null;
        }
    }

    private static string? DestinoLocal(string? destino) =>
        RutaLocal.Validar(destino) is { Length: > 1 } ruta
            && !ruta.StartsWith("/sesion", StringComparison.OrdinalIgnoreCase) && !ruta.StartsWith("/login", StringComparison.OrdinalIgnoreCase)
            && !ruta.StartsWith("/idioma", StringComparison.OrdinalIgnoreCase)
            ? ruta
            : null;

    private static IResult SolicitudVencida(HttpContext context) =>
        Results.BadRequest(context.RequestServices.GetRequiredService<IStringLocalizer<TextosComunes>>()["Solicitud.Vencida"].Value);

    internal static IResult RechazarAcceso(HttpContext context, string estado, int codigoHttp)
    {
        if (SolicitaJson(context)) return Results.Json(new { estado }, statusCode: codigoHttp);
        return estado == "solicitud"
            ? SolicitudVencida(context)
            : Results.LocalRedirect($"/login?estado={estado}");
    }

    private static bool SolicitaJson(HttpContext context) =>
        context.Request.GetTypedHeaders().Accept?.Any(tipo => tipo.MediaType == "application/json") == true;

    private static async Task<IResult> SeleccionarAsync(HttpContext context, IAntiforgery antiforgery, ClienteNeros cliente)
    {
        if (!await ValidarFormularioAsync(context, antiforgery))
        {
            return SolicitudVencida(context);
        }
        var form = await context.Request.ReadFormAsync(context.RequestAborted);
        if (!Guid.TryParse(form["EmpresaId"], out var empresaId))
        {
            return Results.LocalRedirect("/empresas?estado=permiso");
        }
        try
        {
            var empresa = await cliente.SeleccionarAsync(context.User.FindFirstValue(TokenClaim)!, empresaId, context.RequestAborted);
            if (empresa is null)
            {
                return Results.LocalRedirect("/empresas?estado=permiso");
            }
            var autenticacion = await context.AuthenticateAsync();
            var claims = Permisos.ActualizarClaimsEmpresa(context.User.Claims, empresa.Rol);
            claims.RemoveAll(c => c.Type is EmpresaClaim or NombreEmpresaClaim or EmpresaLogoClaim or ModuloClaim or RolEmpresaClaim);
            AgregarClaimsEmpresa(claims, empresa);
            await context.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)), autenticacion.Properties);
            var volver = DestinoLocal(form["Volver"].ToString());
            return Results.LocalRedirect(volver ?? "/home");
        }
        catch (HttpRequestException)
        {
            return Results.LocalRedirect("/empresas?estado=servicio");
        }
        catch (TaskCanceledException) when (!context.RequestAborted.IsCancellationRequested)
        {
            return Results.LocalRedirect("/empresas?estado=servicio");
        }
    }

    private static async Task<IResult> SalirAsync(HttpContext context, IAntiforgery antiforgery, ClienteNeros cliente)
    {
        if (!await ValidarFormularioAsync(context, antiforgery))
        {
            return SolicitudVencida(context);
        }
        try
        {
            await cliente.CerrarAsync(context.User.FindFirstValue(TokenClaim)!, context.RequestAborted);
            await context.SignOutAsync();
            return Results.LocalRedirect("/login?estado=salida");
        }
        catch (HttpRequestException)
        {
            return Results.LocalRedirect("/home?estado=salida");
        }
        catch (TaskCanceledException) when (!context.RequestAborted.IsCancellationRequested)
        {
            return Results.LocalRedirect("/home?estado=salida");
        }
    }

    private static async Task<IResult> CambiarClaveAsync(HttpContext context, IAntiforgery antiforgery, ClienteNeros cliente, ILogger<ClienteNeros> logger)
    {
        if (!await ValidarFormularioAsync(context, antiforgery))
        {
            return SolicitudVencida(context);
        }
        var form = await context.Request.ReadFormAsync(context.RequestAborted);
        var solicitud = new SolicitudCambioClave
        {
            ClaveActual = form["ClaveActual"].ToString(),
            ClaveNueva = form["ClaveNueva"].ToString(),
            Confirmacion = form["Confirmacion"].ToString()
        };
        if (solicitud.ClaveNueva != solicitud.Confirmacion)
        {
            return Results.LocalRedirect($"{RutaCambioClave}?estado=confirmacion");
        }
        if (!Validator.TryValidateObject(solicitud, new ValidationContext(solicitud), null, true))
        {
            return Results.LocalRedirect($"{RutaCambioClave}?estado={(solicitud.ClaveNueva.Length is > 0 and < 12 ? "longitud" : "datos")}");
        }
        try
        {
            var (acceso, error) = await cliente.CambiarClaveAsync(context.User.FindFirstValue(TokenClaim)!, solicitud, context.RequestAborted);
            if (acceso is null)
            {
                return Results.LocalRedirect($"{RutaCambioClave}?estado={error}");
            }
            var autenticacion = await context.AuthenticateAsync();
            var claims = context.User.Claims.Where(claim => claim.Type is EmpresaClaim or NombreEmpresaClaim).ToList();
            claims.AddRange(ClaimsUsuario(acceso.Usuario));
            claims.Add(new Claim(TokenClaim, acceso.Token));
            await context.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)), new AuthenticationProperties
            {
                IsPersistent = autenticacion.Properties?.IsPersistent ?? false, ExpiresUtc = acceso.Expira, AllowRefresh = false
            });
            return Results.LocalRedirect($"{RutaCambioClave}?estado=cambiada");
        }
        catch (HttpRequestException error)
        {
            logger.LogWarning("Cambio de clave: fallo de comunicacion con API. Estado HTTP {Estado}", error.StatusCode);
            return Results.LocalRedirect($"{RutaCambioClave}?estado={(error.StatusCode == HttpStatusCode.TooManyRequests ? "intentos" : "servicio")}");
        }
        catch (TaskCanceledException) when (!context.RequestAborted.IsCancellationRequested)
        {
            return Results.LocalRedirect($"{RutaCambioClave}?estado=servicio");
        }
    }

    private static async Task<IResult> CerrarSesionAsync(HttpContext context, IAntiforgery antiforgery, ClienteNeros cliente)
    {
        if (!await ValidarFormularioAsync(context, antiforgery))
        {
            return SolicitudVencida(context);
        }
        var form = await context.Request.ReadFormAsync(context.RequestAborted);
        if (!Guid.TryParse(form["SesionId"], out var sesionId))
        {
            return Results.LocalRedirect("/cuenta/sesiones?estado=no_encontrada");
        }
        return (await OperarSesionesAsync(context, async token =>
            await cliente.CerrarSesionAsync(token, sesionId, context.RequestAborted) ? "cerrada" : "no_encontrada"))!;
    }

    private static async Task<IResult> CerrarOtrasAsync(HttpContext context, IAntiforgery antiforgery, ClienteNeros cliente)
    {
        if (!await ValidarFormularioAsync(context, antiforgery))
        {
            return SolicitudVencida(context);
        }
        return (await OperarSesionesAsync(context, async token =>
            $"otras&cantidad={await cliente.CerrarSesionesAsync(token, false, context.RequestAborted)}"))!;
    }

    private static async Task<IResult> CerrarTodasAsync(HttpContext context, IAntiforgery antiforgery, ClienteNeros cliente)
    {
        if (!await ValidarFormularioAsync(context, antiforgery))
        {
            return SolicitudVencida(context);
        }
        var resultado = await OperarSesionesAsync(context, async token =>
        {
            await cliente.CerrarSesionesAsync(token, true, context.RequestAborted);
            return null;
        });
        if (resultado is not null) return resultado;
        await context.SignOutAsync();
        return Results.LocalRedirect("/login?estado=salida");
    }

    private static async Task<IResult?> OperarSesionesAsync(HttpContext context, Func<string, Task<string?>> operacion)
    {
        try
        {
            var estado = await operacion(context.User.FindFirstValue(TokenClaim)!);
            return estado is null ? null : Results.LocalRedirect($"/cuenta/sesiones?estado={estado}");
        }
        catch (HttpRequestException)
        {
            return Results.LocalRedirect("/cuenta/sesiones?estado=servicio");
        }
        catch (TaskCanceledException) when (!context.RequestAborted.IsCancellationRequested)
        {
            return Results.LocalRedirect("/cuenta/sesiones?estado=servicio");
        }
    }

    private static async Task<IResult> AceptarLegalesAsync(HttpContext context, IAntiforgery antiforgery, ClienteNeros cliente)
    {
        if (!await ValidarFormularioAsync(context, antiforgery))
        {
            return SolicitudVencida(context);
        }
        var token = context.User.FindFirstValue(TokenClaim)!;
        try
        {
            var pendientes = await cliente.PendientesLegalesAsync(token, context.RequestAborted);
            foreach (var pendiente in pendientes)
            {
                if (!await cliente.AceptarLegalAsync(token, pendiente.Codigo,
                        new SolicitudAceptacionLegal(pendiente.VersionId, pendiente.HashContenido), context.RequestAborted))
                {
                    return Results.LocalRedirect($"{RutaAceptacionLegal}?estado=error");
                }
            }
            var autenticacion = await context.AuthenticateAsync();
            var usuario = await cliente.ConsultarUsuarioAsync(token, context.RequestAborted);
            if (usuario is not null)
            {
                var claims = context.User.Claims.Where(claim => claim.Type is TokenClaim or EmpresaClaim or NombreEmpresaClaim).ToList();
                claims.AddRange(ClaimsUsuario(usuario));
                await context.SignInAsync(
                    new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
                    autenticacion.Properties ?? new AuthenticationProperties());
            }
            return Results.LocalRedirect("/empresas");
        }
        catch (HttpRequestException)
        {
            return Results.LocalRedirect($"{RutaAceptacionLegal}?estado=servicio");
        }
        catch (TaskCanceledException) when (!context.RequestAborted.IsCancellationRequested)
        {
            return Results.LocalRedirect($"{RutaAceptacionLegal}?estado=servicio");
        }
    }

    private static async Task<bool> ValidarFormularioAsync(HttpContext context, IAntiforgery antiforgery)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context);
            return true;
        }
        catch (AntiforgeryValidationException)
        {
            return false;
        }
    }

    private static void AgregarClaimsEmpresa(List<Claim> claims, EmpresaDisponible empresa)
    {
        claims.Add(new Claim(EmpresaClaim, empresa.Id.ToString()));
        claims.Add(new Claim(NombreEmpresaClaim, empresa.Nombre));
        if (!string.IsNullOrWhiteSpace(empresa.Rol))
            claims.Add(new Claim(RolEmpresaClaim, empresa.Rol == "Administrador global" ? "Administrador" : empresa.Rol));
        if (empresa.TieneLogo) claims.Add(new Claim(EmpresaLogoClaim, "true"));
        foreach (var modulo in empresa.ModulosHabilitados ?? [])
            claims.Add(new Claim(ModuloClaim, modulo));
    }

    private static async Task<IResult> LogoEmpresaAsync(Guid empresaId, HttpContext context, ClienteNeros cliente)
    {
        var token = context.User.FindFirstValue(TokenClaim);
        if (token is null) return Results.Unauthorized();
        try
        {
            var (contenido, tipo) = await cliente.LogoEmpresaAsync(token, empresaId, context.RequestAborted);
            if (contenido is null || tipo is null) return Results.NotFound();
            context.Response.Headers.CacheControl = "private, max-age=300";
            return Results.File(contenido, tipo);
        }
        catch (HttpRequestException)
        {
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }
}