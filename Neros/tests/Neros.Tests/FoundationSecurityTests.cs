using System.Security.Claims;
using Neros.ServiceDefaults;
using Xunit;

namespace Neros.Tests;

public sealed class FoundationSecurityTests
{
    [Fact]
    public void Contexto_NiegaAnonimoClaimsDuplicadosYExpiracion()
    {
        var tenant = Guid.NewGuid();
        var claims = Claims(tenant).ToList();
        Assert.False(SecurityContext.TryFromAuthenticatedPrincipal(new ClaimsPrincipal(new ClaimsIdentity(claims)), TimeProvider.System, out _));
        claims.Add(new Claim("tenant_id", Guid.NewGuid().ToString("D")));
        Assert.False(SecurityContext.TryFromAuthenticatedPrincipal(Principal(claims), TimeProvider.System, out _));
        claims = Claims(tenant).Where(claim => claim.Type != "exp").Append(new Claim("exp", "1")).ToList();
        Assert.False(SecurityContext.TryFromAuthenticatedPrincipal(Principal(claims), TimeProvider.System, out _));
    }

    [Fact]
    public void Contexto_RecursoDeOtroTenantOEmpresaNoSeAutoriza()
    {
        var tenant = Guid.NewGuid();
        var company = Guid.NewGuid();
        var claims = Claims(tenant).Append(new Claim("company_id", company.ToString("D")))
            .Append(new Claim("permission", "Organization.Company.Read"));
        Assert.True(SecurityContext.TryFromAuthenticatedPrincipal(Principal(claims), TimeProvider.System, out var context));
        Assert.True(context!.Allows("Organization.Company.Read", new TenantContext(tenant, company), TimeProvider.System));
        Assert.False(context.Allows("Organization.Company.Read", new TenantContext(Guid.NewGuid(), company), TimeProvider.System));
        Assert.False(context.Allows("Organization.Company.Read", new TenantContext(tenant, Guid.NewGuid()), TimeProvider.System));
        Assert.False(context.Allows("Organization.Company.Write", new TenantContext(tenant, company), TimeProvider.System));
    }

    [Fact]
    public void Contexto_RolGlobalNoOtorgaPermisosYSucursalRequiereEmpresa()
    {
        var tenant = Guid.NewGuid();
        var claims = Claims(tenant).Append(new Claim(ClaimTypes.Role, "PlatformAdministrator"));
        Assert.True(SecurityContext.TryFromAuthenticatedPrincipal(Principal(claims), TimeProvider.System, out var context));
        Assert.False(context!.Allows("Organization.Company.Read", new TenantContext(tenant), TimeProvider.System));
        Assert.Throws<ArgumentException>(() => new TenantContext(tenant, branchId: Guid.NewGuid()));
        Assert.Throws<ArgumentException>(() => new TenantContext(Guid.Empty));
    }

    private static ClaimsPrincipal Principal(IEnumerable<Claim> claims) => new(new ClaimsIdentity(claims, "validated-test-identity"));

    private static IEnumerable<Claim> Claims(Guid tenant) =>
    [
        new Claim("sub", "test-subject"),
        new Claim("tenant_id", tenant.ToString("D")),
        new Claim("exp", DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture))
    ];
}