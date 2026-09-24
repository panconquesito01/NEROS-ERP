using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Neros.Api.Seguridad;
using Neros.Application.Autenticacion;
using Neros.Persistence;
using Neros.Persistence.Seguridad;
using Neros.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddNerosTelemetry("Neros.Api.Compatibility");
var conexion = builder.Configuration.GetConnectionString("Neros")
    ?? throw new InvalidOperationException("Configura ConnectionStrings:Neros mediante User Secrets o variables de entorno.");
builder.Services.AddDbContext<NerosDbContext>(options => options.UseSqlServer(conexion));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddIdentityCore<Usuario>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 12;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
}).AddEntityFrameworkStores<NerosDbContext>().AddSignInManager();
builder.Services.AddScoped<IServicioIdentidad, ServicioIdentidad>();
builder.Services.AddScoped<IRepositorioEmpresas, RepositorioEmpresas>();
builder.Services.AddScoped<ServicioEmpresas>();
builder.Services.AddAuthentication("Sesion").AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, AutenticacionSesion>("Sesion", null);
builder.Services.AddAuthorization(options => options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("acceso", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 60, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.AddControllers();
builder.Services.AddNerosHttp();

var app = builder.Build();
if (args.Any(argumento => argumento is "--generar-sql" or "--inicializar-admin" or "--asignar-empresa"))
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
app.MapControllers();
app.MapNerosHealth();
app.Run();

public partial class Program;
