using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Neros.Contracts.Organizacion;
using Neros.Organization.Persistence;
using Neros.ServiceDefaults;

namespace Neros.Organization.Api;

public sealed record ContextoAutorizado(
    string ActorId, Guid TenantId, Guid? CompanyId, Guid? BranchId, ConfiguracionRegionalEmpresa? Regional);

public static class OrganizationHost
{
    public const string Audience = OrganizacionAudiencias.Api;
    public const string ContextPermission = PermisosOrganizacion.ContextoLeer;

    public static WebApplication Crear(WebApplicationBuilder builder)
    {
        if (!Uri.TryCreate(builder.Configuration["Identity:Authority"], UriKind.Absolute, out var authority))
            throw new InvalidOperationException("Configurar Identity:Authority con el emisor HTTPS autorizado.");
        var conexion = builder.Configuration.GetConnectionString("Organization")
            ?? throw new InvalidOperationException("Configurar ConnectionStrings:Organization.");
        builder.AddNerosTelemetry("Neros.Organization.Api");
        builder.Services.AddNerosHttp();
        builder.Services.AddDbContext<OrganizationDbContext>(options => options.UseSqlServer(conexion));
        builder.Services.AddScoped<ServicioCorrespondencia>();
        builder.Services.AddNerosServiceAuthentication(authority, Audience);
        builder.Services.AddValidacionPuenteJwt(builder.Configuration);
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(ContextPermission, policy => policy
                .RequireNerosPermission("organization.read", ContextPermission));
            options.AddPolicy(PermisosOrganizacion.CorrespondenciaLeer, policy => policy
                .RequireNerosPermission("organization.read", PermisosOrganizacion.CorrespondenciaLeer));
        });
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddConcurrencyLimiter("organization", limiter =>
            {
                limiter.PermitLimit = 32;
                limiter.QueueLimit = 0;
                limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
            options.AddFixedWindowLimiter("correspondencia", limiter =>
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
        app.MapGet("/api/v1/organization/correspondencia/{companyId:guid}", async (Guid companyId, ServicioCorrespondencia servicio,
            CancellationToken cancellationToken) =>
            await servicio.ObtenerAsync(companyId, cancellationToken) is { } correspondencia ? Results.Ok(correspondencia) : Results.NotFound())
            .AllowAnonymous().RequireRateLimiting("correspondencia");
        app.MapGet("/api/v1/organization/context", async (HttpContext context, TimeProvider clock, ServicioCorrespondencia servicio,
            CancellationToken cancellationToken) =>
        {
            if (!SecurityContext.TryFromAuthenticatedPrincipal(context.User, clock, out var security))
                return Results.Forbid();
            ConfiguracionRegionalEmpresa? regional = null;
            if (security!.Tenant.CompanyId is { } companyId)
            {
                regional = (await servicio.ObtenerAsync(companyId, cancellationToken))?.Regional;
            }
            return Results.Ok(new ContextoAutorizado(security.ActorId, security.Tenant.TenantId, security.Tenant.CompanyId,
                security.Tenant.BranchId, regional));
        }).RequireAuthorization(ContextPermission).RequireRateLimiting("organization");
        return app;
    }
}
