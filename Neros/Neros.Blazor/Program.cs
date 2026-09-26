using System.Net;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Neros.Blazor.Components;
using Neros.Blazor.Localizacion;
using Neros.Blazor.Servicios;
using Neros.Contracts.Seguridad;
using Neros.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddNerosTelemetry("Neros.Web.Bff");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ServicioUiModales>();
builder.Services.AgregarLocalizacionNeros();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddTransient<CabecerasClienteHandler>();
builder.Services.AddHttpClient<ClienteNeros>(http =>
{
    var direccion = builder.Configuration["Gateway:BaseUrl"] ?? builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5067/";
    if (!builder.Environment.IsDevelopment() && !direccion.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("El destino Gateway/Api debe usar HTTPS fuera de desarrollo.");
    }
    http.BaseAddress = new Uri(direccion);
    http.Timeout = TimeSpan.FromSeconds(12);
}).AddHttpMessageHandler<CabecerasClienteHandler>().AddNerosResilience();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/empresas";
    options.Cookie.Name = "Neros.Sesion";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.SlidingExpiration = false;
    options.Events.OnValidatePrincipal = async context =>
    {
        var token = context.Principal?.FindFirstValue(EndpointsSesion.TokenClaim);
        if (token is null)
        {
            context.RejectPrincipal();
            return;
        }
        try
        {
            var usuario = await context.HttpContext.RequestServices.GetRequiredService<ClienteNeros>()
                .ConsultarUsuarioAsync(token, context.HttpContext.RequestAborted);
            if (usuario is not null)
            {
                context.ReplacePrincipal(EndpointsSesion.ActualizarPrincipal(context.Principal!, usuario));
            }
        }
        catch (HttpRequestException error) when (error.StatusCode == HttpStatusCode.Unauthorized)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
        }
        catch (HttpRequestException)
        {
            context.HttpContext.Items["ApiNoDisponible"] = true;
        }
        catch (TaskCanceledException) when (!context.HttpContext.RequestAborted.IsCancellationRequested)
        {
            context.HttpContext.Items["ApiNoDisponible"] = true;
        }
    };
});
builder.Services.AddAuthorization(options =>
{
    foreach (var permiso in Neros.Application.Seguridad.Permisos.Todos)
        options.AddPolicy(permiso, policy => policy.RequireAuthenticatedUser().RequireClaim(CodigosPermiso.Claim, permiso));
});
builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, _) =>
    {
        await EndpointsSesion.RechazarAcceso(context.HttpContext, "intentos", StatusCodes.Status429TooManyRequests)
            .ExecuteAsync(context.HttpContext);
    };
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

var app = builder.Build();
app.UseNerosCorrelation();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseRequestLocalization();
app.Use(async (context, next) =>
{
    context.Response.Headers.CacheControl = "no-store";
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers.XFrameOptions = "DENY";
    context.Response.Headers["Referrer-Policy"] = "same-origin";
    await next(context);
});
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseCambioClaveObligatorio();
app.UseDocumentosLegalesObligatorios();
app.UseAntiforgery();
app.MapearSesion();
app.MapearIdioma();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
