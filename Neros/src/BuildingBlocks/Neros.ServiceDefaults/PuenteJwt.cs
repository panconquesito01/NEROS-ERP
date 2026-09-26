using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Neros.ServiceDefaults;

/// <summary>Emisor de JWT internos de vida corta para el puente de sesion (ADR-0004, plan 19).</summary>
public sealed class PuenteJwtEmisor(TimeProvider reloj, RsaSecurityKey clave)
{
    public const int DuracionSegundos = 60;

    public string Emitir(string emisor, string audiencia, string actorId, Guid tenantId, Guid? companyId, Guid? branchId,
        IEnumerable<string> permisos, string scope)
    {
        var ahora = reloj.GetUtcNow();
        var claims = new List<Claim>
        {
            new("sub", actorId),
            new("tenant_id", tenantId.ToString("D")),
            new("scope", scope)
        };
        foreach (var permiso in permisos.Distinct(StringComparer.Ordinal)) claims.Add(new Claim("permission", permiso));
        if (companyId is { } empresa) claims.Add(new Claim("company_id", empresa.ToString("D")));
        if (branchId is { } sucursal) claims.Add(new Claim("branch_id", sucursal.ToString("D")));
        var token = new JwtSecurityToken(emisor, audiencia, claims, ahora.UtcDateTime, ahora.AddSeconds(DuracionSegundos).UtcDateTime,
            new SigningCredentials(clave, SecurityAlgorithms.RsaSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RsaSecurityKey Clave => clave;
}

public static class PuenteJwtExtensions
{
    public const string CabeceraEmpresa = "X-Neros-Company-Id";

    public static IServiceCollection AddPuenteJwt(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(_ => CrearClave(configuration));
        services.AddSingleton<PuenteJwtEmisor>();
        return services;
    }

    private static RsaSecurityKey CrearClave(IConfiguration configuration)
    {
        var pem = configuration["Identity:BridgeSigningKeyPem"];
        if (!string.IsNullOrWhiteSpace(pem))
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(pem);
            return new RsaSecurityKey(rsa) { KeyId = configuration["Identity:BridgeSigningKeyId"] ?? "neros-bridge" };
        }
        var rsaDesarrollo = RSA.Create(2048);
        return new RsaSecurityKey(rsaDesarrollo) { KeyId = "neros-bridge-dev" };
    }
}
