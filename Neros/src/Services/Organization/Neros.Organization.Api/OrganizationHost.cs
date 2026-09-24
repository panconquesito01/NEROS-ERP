using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Neros.ServiceDefaults;

namespace Neros.Organization.Api;

public sealed record ContextoAutorizado(string ActorId, Guid TenantId, Guid? CompanyId, Guid? BranchId);

public static class OrganizationHost
{
    public const string Audience = "neros.organization";
    public const string ContextPermission = "Organization.Context.Read";

    public static WebApplication Crear(WebApplicationBuilder builder)
    {
        if (!Uri.TryCreate(builder.Configuration["Identity:Authority"], UriKind.Absolute, out var authority))
            throw new InvalidOperationException("Configurar Identity:Authority con el emisor HTTPS autorizado.");
        builder.AddNerosTelemetry("Neros.Organization.Api");
        builder.Services.AddNerosHttp();
        builder.Services.AddNerosServiceAuthentication(authority, Audience);
        builder.Services.AddAuthorization(options => options.AddPolicy(ContextPermission, policy =>
            policy.RequireNerosPermission("organization.read", ContextPermission)));
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddConcurrencyLimiter("organization", limiter =>
            {
                limiter.PermitLimit = 32;
                limiter.QueueLimit = 0;
                limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
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
        app.MapGet("/api/v1/organization/context", (HttpContext context, TimeProvider clock) =>
        {
            if (!SecurityContext.TryFromAuthenticatedPrincipal(context.User, clock, out var security))
                return Results.Forbid();
            return Results.Ok(new ContextoAutorizado(security!.ActorId, security.Tenant.TenantId,
                security.Tenant.CompanyId, security.Tenant.BranchId));
        }).RequireAuthorization(ContextPermission).RequireRateLimiting("organization");
        return app;
    }
}