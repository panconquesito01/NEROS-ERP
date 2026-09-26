using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neros.Contracts.Organizacion;

namespace Neros.ServiceDefaults;

public static class PuenteJwtValidacion
{
    public static IServiceCollection AddValidacionPuenteJwt(this IServiceCollection services, IConfiguration configuration) =>
        AddValidacionPuenteJwt(services, configuration, OrganizacionAudiencias.Api);

    public static IServiceCollection AddValidacionPuenteJwt(this IServiceCollection services, IConfiguration configuration, string audiencia)
    {
        if (string.IsNullOrWhiteSpace(configuration["Identity:BridgeSigningKeyPem"]))
        {
            return services;
        }
        services.AddPuenteJwt(configuration);
        if (!Uri.TryCreate(configuration["Identity:Authority"], UriKind.Absolute, out var authority))
        {
            throw new InvalidOperationException("Configurar Identity:Authority.");
        }
        services.AddSingleton<IConfigureOptions<JwtBearerOptions>>(provider =>
            new ConfigureNamedOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var puente = provider.GetRequiredService<PuenteJwtEmisor>();
                options.Authority = null;
                options.ConfigurationManager = null;
                options.TokenValidationParameters.ValidIssuer = authority.AbsoluteUri.TrimEnd('/');
                options.TokenValidationParameters.ValidAudience = audiencia;
                options.TokenValidationParameters.IssuerSigningKey = puente.Clave;
            }));
        return services;
    }
}
