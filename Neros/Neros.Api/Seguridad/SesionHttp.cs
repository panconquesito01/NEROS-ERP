using System.Net;
using System.Security.Claims;
using Neros.Application.Seguridad;
using Neros.Contracts.Autenticacion;
using Neros.Contracts.Cuenta;
using Neros.Contracts.Privacidad;
using Neros.ServiceDefaults;

namespace Neros.Api.Seguridad;

public static class SesionHttp
{
    public const string SesionIdClaim = "neros:sesion-id";
    public const string DebeCambiarClaveClaim = "neros:debe-cambiar-clave";
    public const string LegalPendienteClaim = "neros:legal-pendiente";

    public static string UsuarioId(this ClaimsPrincipal usuario) => usuario.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public static Guid SesionId(this ClaimsPrincipal usuario) => Guid.Parse(usuario.FindFirstValue(SesionIdClaim)!);

    public static UsuarioActual UsuarioActual(this ClaimsPrincipal usuario) => new(
        usuario.UsuarioId(), usuario.Identity!.Name!, usuario.FindFirstValue(ClaimTypes.Email)!,
        usuario.HasClaim(DebeCambiarClaveClaim, "true"),
        usuario.FindAll(Permisos.Claim).Select(permiso => permiso.Value).ToList(),
        usuario.FindAll("neros:legal-doc").Select(doc => doc.Value).ToList());

    public static ContextoCliente ContextoCliente(this HttpContext context)
    {
        var informada = context.Request.Headers[CabecerasCliente.Ip].ToString();
        var ip = IPAddress.TryParse(informada, out var direccion) ? direccion.ToString() : context.Connection.RemoteIpAddress?.ToString();
        var agente = context.Request.Headers[CabecerasCliente.Agente].ToString() is { Length: > 0 } valor ? valor : context.Request.Headers.UserAgent.ToString();
        return new ContextoCliente(Recortar(ip, 45), Recortar(agente, 256), Recortar(FoundationHttp.GetCorrelationId(context), 64));
    }

    /// <summary>Con clave temporal solo se admiten los endpoints marcados con <see cref="PermitirConClaveTemporalAttribute"/>.</summary>
    public static IApplicationBuilder UseCambioClaveObligatorio(this IApplicationBuilder app) => app.Use(async (context, next) =>
    {
        if (context.User.HasClaim(DebeCambiarClaveClaim, "true")
            && context.GetEndpoint()?.Metadata.GetMetadata<PermitirConClaveTemporalAttribute>() is null)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new ErrorOperacion(CodigosCuenta.CambioClaveRequerido));
            return;
        }
        await next(context);
    });

    /// <summary>Con documentos legales pendientes solo se admiten endpoints marcados con <see cref="PermitirConDocumentosPendientesAttribute"/>.</summary>
    public static IApplicationBuilder UseDocumentosLegalesPendientes(this IApplicationBuilder app) => app.Use(async (context, next) =>
    {
        if (context.User.HasClaim(LegalPendienteClaim, "true")
            && context.GetEndpoint()?.Metadata.GetMetadata<PermitirConDocumentosPendientesAttribute>() is null)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new ErrorOperacion(CodigosPrivacidad.DocumentosPendientes));
            return;
        }
        await next(context);
    });

    private static string? Recortar(string? valor, int maximo) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Length <= maximo ? valor : valor[..maximo];
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class PermitirConClaveTemporalAttribute : Attribute;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class PermitirConDocumentosPendientesAttribute : Attribute;
