using System.Collections.Frozen;
using System.Security.Claims;

namespace Neros.ServiceDefaults;

public sealed record TenantContext
{
    public Guid TenantId { get; }
    public Guid? CompanyId { get; }
    public Guid? BranchId { get; }

    public TenantContext(Guid tenantId, Guid? companyId = null, Guid? branchId = null)
    {
        if (tenantId == Guid.Empty || companyId == Guid.Empty || branchId == Guid.Empty)
            throw new ArgumentException("El ambito no admite identificadores vacios.");
        if (branchId.HasValue && !companyId.HasValue)
            throw new ArgumentException("La sucursal requiere una empresa.");
        TenantId = tenantId;
        CompanyId = companyId;
        BranchId = branchId;
    }
}

public sealed class SecurityContext
{
    public string ActorId { get; }
    public TenantContext Tenant { get; }
    public DateTimeOffset ExpiresAt { get; }
    public IReadOnlySet<string> Permissions { get; }

    private SecurityContext(string actorId, TenantContext tenant, DateTimeOffset expiresAt, IEnumerable<string> permissions)
    {
        ActorId = actorId;
        Tenant = tenant;
        ExpiresAt = expiresAt;
        Permissions = permissions.ToFrozenSet(StringComparer.Ordinal);
    }

    public static bool TryFromAuthenticatedPrincipal(ClaimsPrincipal principal, TimeProvider clock, out SecurityContext? context)
    {
        context = null;
        var identities = principal.Identities.Where(identity => identity.IsAuthenticated).ToArray();
        if (identities.Length != 1) return false;
        var identity = identities[0];
        string? SingleClaim(string type)
        {
            var claims = identity.FindAll(type).ToArray();
            return claims.Length == 1 ? claims[0].Value : null;
        }

        var actor = SingleClaim("sub");
        if (string.IsNullOrWhiteSpace(actor) || actor.Length > 450
            || !Guid.TryParseExact(SingleClaim("tenant_id"), "D", out var tenantId) || tenantId == Guid.Empty
            || !long.TryParse(SingleClaim("exp"), System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out var expires)
            || expires <= clock.GetUtcNow().ToUnixTimeSeconds() || expires > 253402300799)
            return false;

        Guid? companyId = null;
        Guid? branchId = null;
        foreach (var type in new[] { "company_id", "branch_id" })
        {
            if (!identity.HasClaim(claim => claim.Type == type)) continue;
            if (!Guid.TryParseExact(SingleClaim(type), "D", out var value) || value == Guid.Empty) return false;
            if (type == "company_id") companyId = value;
            else branchId = value;
        }
        if (branchId.HasValue && !companyId.HasValue) return false;
        context = new SecurityContext(actor, new TenantContext(tenantId, companyId, branchId),
            DateTimeOffset.FromUnixTimeSeconds(expires), identity.FindAll("permission").Select(claim => claim.Value));
        return true;
    }

    public bool Allows(string permission, TenantContext resource, TimeProvider clock) =>
        ExpiresAt > clock.GetUtcNow()
        && Tenant.TenantId == resource.TenantId
        && (!Tenant.CompanyId.HasValue || Tenant.CompanyId == resource.CompanyId)
        && (!Tenant.BranchId.HasValue || Tenant.BranchId == resource.BranchId)
        && Permissions.Contains(permission);
}