using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Neros.Contracts.Autenticacion;
using Neros.Contracts.Organizacion;
using Neros.Contracts.Seguridad;
using Neros.Contracts.Busqueda;
using Neros.Contracts.Terceros;
using Neros.ServiceDefaults;

namespace Neros.Gateway;

public static class PuenteSesionOrganizacion
{
    public static IServiceCollection AddPuenteSesionOrganizacion(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPuenteJwt(configuration);
        services.AddHttpClient("puente-compatibilidad", (provider, client) =>
        {
            var destino = configuration["Services:Compatibility"]
                ?? throw new InvalidOperationException("Configurar Services:Compatibility.");
            client.BaseAddress = new Uri(destino);
            client.Timeout = TimeSpan.FromSeconds(5);
        }).AddNerosResilience();
        services.AddHttpClient("puente-organizacion", (provider, client) =>
        {
            var destino = configuration["Services:Organization"]
                ?? throw new InvalidOperationException("Configurar Services:Organization.");
            client.BaseAddress = new Uri(destino);
            client.Timeout = TimeSpan.FromSeconds(5);
        }).AddNerosResilience();
        return services;
    }

    public static IApplicationBuilder UsePuenteSesionOrganizacion(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var path = context.Request.Path;
            var esOrganization = path.StartsWithSegments("/api/v1/organization", StringComparison.OrdinalIgnoreCase);
            var esTerceros = path.StartsWithSegments("/api/v1/terceros", StringComparison.OrdinalIgnoreCase);
            var esSearch = path.StartsWithSegments("/api/v1/search", StringComparison.OrdinalIgnoreCase);
            if (!esOrganization && !esTerceros && !esSearch)
            {
                await next(context);
                return;
            }
            if (esOrganization && path.StartsWithSegments("/api/v1/organization/correspondencia", StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }
            var authorization = context.Request.Headers.Authorization.ToString();
            if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }
            var token = authorization["Bearer ".Length..].Trim();
            if (token.Contains('.', StringComparison.Ordinal))
            {
                await next(context);
                return;
            }
            if (token.Length != 64 || !token.All(Uri.IsHexDigit))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
            var emisor = app.ApplicationServices.GetRequiredService<IConfiguration>()["Identity:Authority"]
                ?? throw new InvalidOperationException("Configurar Identity:Authority.");
            var emisorUri = emisor.TrimEnd('/');
            var factory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
            using var compatibilidad = factory.CreateClient("puente-compatibilidad");
            using var yo = new HttpRequestMessage(HttpMethod.Get, "api/acceso/yo");
            yo.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var respuestaYo = await compatibilidad.SendAsync(yo, context.RequestAborted);
            if (respuestaYo.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
            if (!respuestaYo.IsSuccessStatusCode)
            {
                context.Response.StatusCode = StatusCodes.Status502BadGateway;
                return;
            }
            var usuario = await respuestaYo.Content.ReadFromJsonAsync<UsuarioActual>(context.RequestAborted);
            if (usuario is null)
            {
                context.Response.StatusCode = StatusCodes.Status502BadGateway;
                return;
            }
            if (!context.Request.Headers.TryGetValue(PuenteJwtExtensions.CabeceraEmpresa, out var cabeceraEmpresa)
                || !Guid.TryParse(cabeceraEmpresa.ToString(), out var companyId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
            using var membresia = new HttpRequestMessage(HttpMethod.Get, $"api/empresas/{companyId:D}");
            membresia.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var respuestaEmpresa = await compatibilidad.SendAsync(membresia, context.RequestAborted);
            if (respuestaEmpresa.StatusCode == System.Net.HttpStatusCode.NotFound
                || respuestaEmpresa.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
            if (!respuestaEmpresa.IsSuccessStatusCode)
            {
                context.Response.StatusCode = StatusCodes.Status502BadGateway;
                return;
            }
            var empresa = await respuestaEmpresa.Content.ReadFromJsonAsync<EmpresaDisponible>(context.RequestAborted);
            if (empresa is null)
            {
                context.Response.StatusCode = StatusCodes.Status502BadGateway;
                return;
            }
            using var organizacion = factory.CreateClient("puente-organizacion");
            using var correspondencia = await organizacion.GetAsync($"api/v1/organization/correspondencia/{companyId:D}",
                context.RequestAborted);
            if (correspondencia.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
            if (!correspondencia.IsSuccessStatusCode)
            {
                context.Response.StatusCode = StatusCodes.Status502BadGateway;
                return;
            }
            var correspondenciaEmpresa = await correspondencia.Content.ReadFromJsonAsync<CorrespondenciaEmpresa>(context.RequestAborted);
            if (correspondenciaEmpresa is null)
            {
                context.Response.StatusCode = StatusCodes.Status502BadGateway;
                return;
            }
            var puente = context.RequestServices.GetRequiredService<PuenteJwtEmisor>();
            if (esOrganization)
            {
                var permisos = new List<string> { PermisosOrganizacion.ContextoLeer, PermisosOrganizacion.CorrespondenciaLeer };
                permisos.AddRange(usuario.Permisos ?? []);
                var jwt = puente.Emitir(emisorUri, OrganizacionAudiencias.Api, usuario.Id, correspondenciaEmpresa.TenantId,
                    companyId, null, permisos, "organization.read");
                context.Request.Headers.Authorization = $"Bearer {jwt}";
            }
            else if (esTerceros)
            {
                var codigosEmpresa = PermisosEmpresa.DeRol(empresa.Rol);
                var permisosJwt = PuentePermisosTerceros.DesdeCodigosEmpresa(codigosEmpresa).Distinct(StringComparer.Ordinal).ToList();
                if (permisosJwt.Count == 0)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
                var alcance = PuentePermisosTerceros.AlcancesDesdeCodigos(codigosEmpresa);
                var jwt = puente.Emitir(emisorUri, TercerosAudiencias.Api, usuario.Id, correspondenciaEmpresa.TenantId,
                    companyId, null, permisosJwt, alcance);
                context.Request.Headers.Authorization = $"Bearer {jwt}";
            }
            else
            {
                var codigosEmpresa = PermisosEmpresa.DeRol(empresa.Rol);
                if (!codigosEmpresa.Contains(CodigosPermiso.BusquedaIndiceConsultar))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
                var permisosJwt = PuentePermisosBusqueda.DesdeCodigosEmpresa(codigosEmpresa)
                    .Concat(codigosEmpresa)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
                var jwt = puente.Emitir(emisorUri, SearchAudiencias.Api, usuario.Id, correspondenciaEmpresa.TenantId,
                    companyId, null, permisosJwt, PuentePermisosBusqueda.AlcanceDesdeCodigos(codigosEmpresa));
                context.Request.Headers.Authorization = $"Bearer {jwt}";
            }
            await next(context);
        });
    }

    public static void ConfigurarValidacionPuente(this IServiceCollection services, RsaSecurityKey clave, string emisor)
    {
        services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Authority = null;
            options.ConfigurationManager = null;
            options.TokenValidationParameters.ValidIssuer = emisor.TrimEnd('/');
            options.TokenValidationParameters.ValidAudience = OrganizacionAudiencias.Api;
            options.TokenValidationParameters.IssuerSigningKey = clave;
        });
    }
}
