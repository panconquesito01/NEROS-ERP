using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Neros.Api.Seguridad;
using Neros.Application.Administracion;
using Neros.Application.Autenticacion;
using Neros.Application.Globalizacion;
using Neros.Persistence.Administracion;
using Neros.Application.Organizacion;
using Neros.Organization.Persistence;
using Neros.Application.Operacion;
using Neros.Persistence.Globalizacion;
using Neros.Persistence.Operacion;
using Neros.Persistence.Organizacion;
using Neros.Application.Cuenta;
using Neros.Application.Privacidad;
using Neros.Application.Seguridad;
using Neros.Persistence;
using Neros.Persistence.Privacidad;
using Neros.Persistence.Seguridad;
using Neros.Api.Servicios;
using Neros.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddNerosTelemetry("Neros.Api.Compatibility");
var conexion = builder.Configuration.GetConnectionString("Neros")
    ?? throw new InvalidOperationException("Configura ConnectionStrings:Neros mediante User Secrets o variables de entorno.");
builder.Services.AddDbContext<NerosDbContext>(options => options.UseSqlServer(conexion));
var conexionOrganizacion = builder.Configuration.GetConnectionString("Organization");
if (!string.IsNullOrWhiteSpace(conexionOrganizacion))
    builder.Services.AddDbContext<OrganizationDbContext>(options => options.UseSqlServer(conexionOrganizacion));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddIdentityCore<Usuario>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 12;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
}).AddEntityFrameworkStores<NerosDbContext>().AddSignInManager();
builder.Services.AddScoped<SesionesUsuario>();
builder.Services.AddScoped<RegistroAuditoria>();
builder.Services.AddScoped<IServicioIdentidad, ServicioIdentidad>();
builder.Services.AddScoped<IServicioCuenta, ServicioCuenta>();
builder.Services.AddScoped<IServicioAdministracionUsuarios, ServicioAdministracionUsuarios>();
builder.Services.AddScoped<IServicioAdministracionPlataforma, ServicioAdministracionPlataforma>();
builder.Services.AddScoped<IServicioAdministracionEmpresa, ServicioAdministracionEmpresa>();
builder.Services.AddScoped<IServicioPrivacidad, ServicioPrivacidad>();
builder.Services.AddScoped<IRepositorioEmpresas, RepositorioEmpresas>();
builder.Services.AddScoped<ServicioEmpresas>();
builder.Services.AddScoped<IServicioCatalogosGlobalizacion, ServicioCatalogosGlobalizacion>();
builder.Services.AddScoped<IServicioProgramasOperativos, ServicioProgramasOperativos>();
builder.Services.AddScoped<IServicioTasasCambio, ServicioTasasCambio>();
builder.Services.AddScoped<IServicioSincronizacionTasasReferencia, ServicioSincronizacionTasasReferencia>();
builder.Services.Configure<OpcionesTasasCambio>(builder.Configuration.GetSection(OpcionesTasasCambio.Seccion));
var opcionesTasas = builder.Configuration.GetSection(OpcionesTasasCambio.Seccion).Get<OpcionesTasasCambio>() ?? new OpcionesTasasCambio();
if (opcionesTasas.Automatico)
    builder.Services.AddHostedService<ServicioTasasCambioProgramado>();
builder.Services.AddScoped<IProveedorTasasMercado, ProveedorTrmColombia>();
builder.Services.AddScoped<IProveedorTasasMercado, ProveedorTasasFrankfurter>();
if (!string.IsNullOrWhiteSpace(conexionOrganizacion))
    builder.Services.AddScoped<IServicioConfiguracionRegional, ServicioConfiguracionRegional>();
builder.Services.AddHttpClient("tasas-frankfurter", c => { c.BaseAddress = new Uri("https://api.frankfurter.app/"); c.Timeout = TimeSpan.FromSeconds(15); });
builder.Services.AddHttpClient("tasas-trm-co", c => { c.Timeout = TimeSpan.FromSeconds(15); });
builder.Services.AddAuthentication("Sesion").AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, AutenticacionSesion>("Sesion", null);
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    foreach (var permiso in Permisos.Todos)
        options.AddPolicy(permiso, policy => policy.RequireAuthenticatedUser().RequireClaim(Permisos.Claim, permiso));
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("acceso", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 60, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.AddPolicy("cuenta", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Request.Headers.Authorization.ToString() is { Length: > 0 } token ? SesionesUsuario.Hash(token) : "anonimo",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.AddControllers();
builder.Services.AddNerosHttp();

var app = builder.Build();
if (args.Any(argumento => argumento is "--inicializar-admin" or "--asignar-empresa"))
{
    if (!app.Environment.IsDevelopment())
    {
        throw new InvalidOperationException("Los comandos locales requieren el entorno Development.");
    }
    await ComandosAdministracion.EjecutarAsync(app.Services, args);
    return;
}

app.UseNerosHttp();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.Use(async (context, next) =>
{
    context.Response.Headers.CacheControl = "no-store";
    context.Response.Headers.XContentTypeOptions = "nosniff";
    await next(context);
});
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseCambioClaveObligatorio();
app.UseDocumentosLegalesPendientes();
app.MapControllers();
app.MapNerosHealth();
app.Run();

public partial class Program;
