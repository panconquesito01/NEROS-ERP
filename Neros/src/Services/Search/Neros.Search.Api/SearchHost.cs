using Microsoft.AspNetCore.RateLimiting;
using Neros.Contracts.Busqueda;
using Neros.Contracts.Seguridad;
using Neros.ServiceDefaults;

namespace Neros.Search.Api;

public static class SearchHost
{
    public const string Audience = SearchAudiencias.Api;

    public static WebApplication Crear(WebApplicationBuilder builder)
    {
        if (!Uri.TryCreate(builder.Configuration["Identity:Authority"], UriKind.Absolute, out var authority))
            throw new InvalidOperationException("Configurar Identity:Authority con el emisor HTTPS autorizado.");
        var conexion = builder.Configuration.GetConnectionString("Search")
            ?? throw new InvalidOperationException("Configurar ConnectionStrings:Search.");
        builder.AddNerosTelemetry("Neros.Search.Api");
        builder.Services.AddNerosHttp();
        builder.Services.AddSingleton(new ServicioBusqueda(conexion));
        builder.Services.AddNerosServiceAuthentication(authority, Audience);
        builder.Services.AddValidacionPuenteJwt(builder.Configuration, Audience);
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(PermisosBusqueda.Consultar, policy => policy
                .RequireNerosPermission("search.read", PermisosBusqueda.Consultar));
        });
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter("search", limiter =>
            {
                limiter.PermitLimit = 120;
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.QueueLimit = 0;
            });
        });
        var app = builder.Build();
        app.UseNerosHttp();
        app.Use(async (context, next) =>
        {
            if ((!app.Environment.IsDevelopment() && !context.Request.IsHttps)
                || context.Request.Headers.Keys.Any(header => header.Equals("X-Tenant-Id", StringComparison.OrdinalIgnoreCase)
                || header.Equals("X-Company-Id", StringComparison.OrdinalIgnoreCase)
                || header.Equals("X-Actor-Id", StringComparison.OrdinalIgnoreCase)
                || header.Equals("X-Permissions", StringComparison.OrdinalIgnoreCase)))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
            await next(context);
        });
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
        app.MapNerosHealth();
        app.MapGet("/api/v1/search", async (string? q, int? max, string? cursorTitulo, Guid? cursorId,
            HttpContext context, ServicioBusqueda servicio, CancellationToken cancellationToken) =>
        {
            if (!Ambito(context, out var tenantId, out var empresaId, out var permisosCodigo))
                return Results.Forbid();
            var pagina = await servicio.BuscarAsync(
                tenantId, empresaId, permisosCodigo, q, Math.Clamp(max ?? 20, 1, 50),
                cursorTitulo, cursorId, cancellationToken);
            return Results.Ok(pagina);
        }).RequireAuthorization(PermisosBusqueda.Consultar).RequireRateLimiting("search");
        return app;
    }

    private static bool Ambito(
        HttpContext context,
        out Guid tenantId,
        out Guid empresaId,
        out IReadOnlySet<string> permisosCodigoEmpresa)
    {
        tenantId = Guid.Empty;
        empresaId = Guid.Empty;
        permisosCodigoEmpresa = new HashSet<string>(StringComparer.Ordinal);
        if (!SecurityContext.TryFromAuthenticatedPrincipal(context.User, TimeProvider.System, out var security))
            return false;
        if (security!.Tenant.CompanyId is not { } companyId)
            return false;
        tenantId = security.Tenant.TenantId;
        empresaId = companyId;
        permisosCodigoEmpresa = security.Permissions
            .Where(EsCodigoPermisoEmpresa)
            .ToHashSet(StringComparer.Ordinal);
        return permisosCodigoEmpresa.Count > 0;
    }

    private static bool EsCodigoPermisoEmpresa(string permiso) =>
        permiso.Count(c => c == '.') == 2
        && permiso.Split('.') is [var modulo, _, _]
        && modulo != "Search"
        && modulo != "Terceros"
        && modulo != "Platform";
}
