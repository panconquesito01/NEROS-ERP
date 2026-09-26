using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Neros.Contracts.Terceros;
using Neros.ServiceDefaults;
using Neros.Terceros.Persistence;

namespace Neros.Terceros.Api;

public static class TercerosHost
{
    public const string Audience = TercerosAudiencias.Api;

    public static WebApplication Crear(WebApplicationBuilder builder)
    {
        if (!Uri.TryCreate(builder.Configuration["Identity:Authority"], UriKind.Absolute, out var authority))
            throw new InvalidOperationException("Configurar Identity:Authority con el emisor HTTPS autorizado.");
        var conexion = builder.Configuration.GetConnectionString("Terceros")
            ?? throw new InvalidOperationException("Configurar ConnectionStrings:Terceros.");
        builder.AddNerosTelemetry("Neros.Terceros.Api");
        builder.Services.AddNerosHttp();
        builder.Services.AddDbContext<TercerosDbContext>(options => options.UseSqlServer(conexion));
        builder.Services.AddScoped<ServicioTerceros>();
        builder.Services.AddNerosServiceAuthentication(authority, Audience);
        builder.Services.AddValidacionPuenteJwt(builder.Configuration, Audience);
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(PermisosTerceros.Consultar, policy => policy
                .RequireNerosPermission("terceros.read", PermisosTerceros.Consultar));
            options.AddPolicy(PermisosTerceros.Escribir, policy => policy
                .RequireNerosPermission("terceros.write", PermisosTerceros.Escribir));
        });
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter("terceros", limiter =>
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
        app.MapGet("/api/v1/terceros", async (string? q, int? pagina, int? tamano, HttpContext context, ServicioTerceros servicio,
            CancellationToken cancellationToken) =>
        {
            if (!Ambito(context, out var tenantId)) return Results.Forbid();
            return Results.Ok(await servicio.BuscarAsync(tenantId, q, pagina ?? 1, tamano ?? 20, cancellationToken));
        }).RequireAuthorization(PermisosTerceros.Consultar).RequireRateLimiting("terceros");
        app.MapGet("/api/v1/terceros/{id:guid}", async (Guid id, HttpContext context, ServicioTerceros servicio,
            CancellationToken cancellationToken) =>
        {
            if (!Ambito(context, out var tenantId)) return Results.Forbid();
            var detalle = await servicio.ObtenerAsync(tenantId, id, cancellationToken);
            return detalle is null ? Results.NotFound() : Results.Ok(detalle);
        }).RequireAuthorization(PermisosTerceros.Consultar).RequireRateLimiting("terceros");
        app.MapGet("/api/v1/terceros/{id:guid}/historial", async (Guid id, HttpContext context, ServicioTerceros servicio,
            CancellationToken cancellationToken) =>
        {
            if (!Ambito(context, out var tenantId)) return Results.Forbid();
            if (await servicio.ObtenerAsync(tenantId, id, cancellationToken) is null) return Results.NotFound();
            return Results.Ok(await servicio.HistorialAsync(tenantId, id, cancellationToken));
        }).RequireAuthorization(PermisosTerceros.Consultar).RequireRateLimiting("terceros");
        app.MapPost("/api/v1/terceros", async (SolicitudCrearTercero solicitud, HttpContext context, ServicioTerceros servicio,
            CancellationToken cancellationToken) =>
        {
            if (!Ambito(context, out var tenantId)) return Results.Forbid();
            var (detalle, error) = await servicio.CrearAsync(tenantId, solicitud, cancellationToken);
            if (error is not null) return Results.BadRequest(error);
            return Results.Created($"/api/v1/terceros/{detalle!.Id:D}", detalle);
        }).RequireAuthorization(PermisosTerceros.Escribir).RequireRateLimiting("terceros");
        app.MapPut("/api/v1/terceros/{id:guid}", async (Guid id, SolicitudActualizarTercero solicitud, HttpContext context,
            ServicioTerceros servicio, CancellationToken cancellationToken) =>
        {
            if (!Ambito(context, out var tenantId)) return Results.Forbid();
            var (detalle, error) = await servicio.ActualizarAsync(tenantId, id, solicitud, cancellationToken);
            if (error is not null) return Results.BadRequest(error);
            return detalle is null ? Results.NotFound() : Results.Ok(detalle);
        }).RequireAuthorization(PermisosTerceros.Escribir).RequireRateLimiting("terceros");
        return app;
    }

    private static bool Ambito(HttpContext context, out Guid tenantId)
    {
        tenantId = Guid.Empty;
        if (!SecurityContext.TryFromAuthenticatedPrincipal(context.User, TimeProvider.System, out var security))
            return false;
        tenantId = security!.Tenant.TenantId;
        return true;
    }
}
