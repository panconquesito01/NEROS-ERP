using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Neros.ServiceDefaults;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;

namespace Neros.Gateway;

public static class GatewayHost
{
    public static WebApplication Crear(WebApplicationBuilder builder)
    {
        var legacy = Destino(builder, "Services:Compatibility");
        var organization = Destino(builder, "Services:Organization");
        if (!Uri.TryCreate(builder.Configuration["Identity:Authority"], UriKind.Absolute, out var authority))
            throw new InvalidOperationException("Configurar Identity:Authority con el emisor HTTPS autorizado.");
        builder.AddNerosTelemetry("Neros.Gateway");
        builder.Services.AddNerosHttp();
        builder.Services.AddNerosServiceAuthentication(authority, "neros.organization");
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = null;
            options.DefaultAuthenticateScheme = "compatibility";
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddScheme<AuthenticationSchemeOptions, CompatibilityHandler>("compatibility", _ => { });
        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser().Build();
            options.AddPolicy("organization", policy => policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireNerosPermission("organization.read", "Organization.Context.Read"));
            options.AddPolicy("Neros.Health", policy => policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser().RequireClaim("permission", "Platform.Health.Read"));
        });
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddConcurrencyLimiter("gateway", limiter => { limiter.PermitLimit = 32; limiter.QueueLimit = 0; });
        });
        RouteConfig[] routes =
        [
            Ruta("access", "compatibility", "/api/acceso/{**remainder}", "anonymous"),
            Ruta("companies", "compatibility", "/api/empresas/{**remainder}", "anonymous"),
            Ruta("organization", "organization", "/api/v1/organization/{**remainder}", "organization")
        ];
        ClusterConfig[] clusters = [Cluster("compatibility", legacy), Cluster("organization", organization)];
        builder.Services.AddReverseProxy().LoadFromMemory(routes, clusters)
            .ConfigureHttpClient((_, handler) => handler.ConnectTimeout = TimeSpan.FromSeconds(3))
            .AddTransforms(context => context.AddRequestTransform(request =>
            {
                request.ProxyRequest.Headers.Remove(FoundationHttp.CorrelationHeader);
                request.ProxyRequest.Headers.TryAddWithoutValidation(FoundationHttp.CorrelationHeader,
                    FoundationHttp.GetCorrelationId(request.HttpContext));
                return ValueTask.CompletedTask;
            }));
        var app = builder.Build();
        app.UseNerosHttp();
        app.Use(async (context, next) =>
        {
            if (!app.Environment.IsDevelopment() && !context.Request.IsHttps)
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
        app.MapReverseProxy().RequireRateLimiting("gateway");
        return app;
    }

    private sealed class CompatibilityHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync() => Task.FromResult(AuthenticateResult.NoResult());
    }

    private static string Destino(WebApplicationBuilder builder, string key)
    {
        if (!Uri.TryCreate(builder.Configuration[key], UriKind.Absolute, out var uri)
            || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query)
            || !string.IsNullOrEmpty(uri.Fragment) || uri.AbsolutePath != "/"
            || (uri.Scheme != Uri.UriSchemeHttps && !(builder.Environment.IsDevelopment() && uri.Scheme == Uri.UriSchemeHttp)))
            throw new InvalidOperationException($"Configurar {key} con un origen valido; HTTPS fuera de desarrollo.");
        return uri.AbsoluteUri;
    }

    private static RouteConfig Ruta(string id, string cluster, string path, string policy) => new()
    {
        RouteId = id,
        ClusterId = cluster,
        Match = new RouteMatch { Path = path },
        AuthorizationPolicy = policy,
        Transforms =
        [
            new Dictionary<string, string> { ["RequestHeadersAllowed"] = "Accept;Authorization;Content-Type;Content-Length;If-Match;If-None-Match;Idempotency-Key;X-Correlation-Id" },
            new Dictionary<string, string> { ["X-Forwarded"] = "Remove" }
        ]
    };

    private static ClusterConfig Cluster(string id, string address) => new()
    {
        ClusterId = id,
        Destinations = new Dictionary<string, DestinationConfig> { ["primary"] = new() { Address = address } },
        HttpClient = new HttpClientConfig { MaxConnectionsPerServer = 32 },
        HttpRequest = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(10) }
    };
}