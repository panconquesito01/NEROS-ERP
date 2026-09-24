using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Neros.Contracts.Autenticacion;

namespace Neros.Blazor.Servicios;

public static class EndpointsSesion
{
    public const string TokenClaim = "neros:sesion";
    public const string EmpresaClaim = "neros:empresa";
    public const string NombreEmpresaClaim = "neros:empresa-nombre";

    public static void MapearSesion(this WebApplication app)
    {
        app.MapPost("/sesion/entrar", EntrarAsync).AllowAnonymous().RequireRateLimiting("login");
        app.MapPost("/sesion/empresa", SeleccionarAsync).RequireAuthorization();
        app.MapPost("/sesion/salir", SalirAsync).RequireAuthorization();
    }

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
            var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, acceso.Usuario.Id),
                new Claim(ClaimTypes.Name, acceso.Usuario.Nombre),
                new Claim(ClaimTypes.Email, acceso.Usuario.Correo),
                new Claim(TokenClaim, acceso.Token)
            ], CookieAuthenticationDefaults.AuthenticationScheme));
            await context.SignInAsync(principal, new AuthenticationProperties
            {
                IsPersistent = solicitud.Recordarme, ExpiresUtc = acceso.Expira, AllowRefresh = false
            });
            return SolicitaJson(context) ? Results.NoContent() : Results.LocalRedirect("/empresas");
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

    internal static IResult RechazarAcceso(HttpContext context, string estado, int codigoHttp)
    {
        if (SolicitaJson(context)) return Results.Json(new { estado }, statusCode: codigoHttp);
        return estado == "solicitud"
            ? Results.BadRequest("La solicitud vencio. Recarga la pagina e intentalo de nuevo.")
            : Results.LocalRedirect($"/login?estado={estado}");
    }

    private static bool SolicitaJson(HttpContext context) =>
        context.Request.GetTypedHeaders().Accept?.Any(tipo => tipo.MediaType == "application/json") == true;

    private static async Task<IResult> SeleccionarAsync(HttpContext context, IAntiforgery antiforgery, ClienteNeros cliente)
    {
        if (!await ValidarFormularioAsync(context, antiforgery))
        {
            return Results.BadRequest("La solicitud vencio. Recarga la pagina e intentalo de nuevo.");
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
            var claims = context.User.Claims.Where(claim => claim.Type != EmpresaClaim && claim.Type != NombreEmpresaClaim).ToList();
            claims.Add(new Claim(EmpresaClaim, empresa.Id.ToString()));
            claims.Add(new Claim(NombreEmpresaClaim, empresa.Nombre));
            await context.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)), autenticacion.Properties);
            return Results.LocalRedirect("/home");
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
            return Results.BadRequest("La solicitud vencio. Recarga la pagina e intentalo de nuevo.");
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
}