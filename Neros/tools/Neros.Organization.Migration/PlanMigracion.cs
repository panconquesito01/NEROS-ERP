using System.Text.Json.Serialization;

namespace Neros.Organization.Migration;

[JsonConverter(typeof(JsonStringEnumConverter<EstadoCorrespondencia>))]
public enum EstadoCorrespondencia { Pendiente, Aprobada }

public sealed record TenantMigracion(Guid TenantId, string Code);
public sealed record GrupoMigracion(Guid BusinessGroupId, Guid TenantId);
public sealed record EmpresaMigracion(Guid CompanyId, Guid? TenantId, Guid? BusinessGroupId,
    EstadoCorrespondencia MigrationStatus);
public sealed record PlanMigracion(int Version, TenantMigracion[] Tenants, GrupoMigracion[] BusinessGroups,
    EmpresaMigracion[] Companies);

public static class ValidadorMigracion
{
    public static PlanMigracion Preparar(IEnumerable<Guid> empresasOrigen) =>
        new(1, [], [], empresasOrigen.Order().Select(companyId =>
            new EmpresaMigracion(companyId, null, null, EstadoCorrespondencia.Pendiente)).ToArray());

    public static IReadOnlyList<string> Validar(PlanMigracion plan, IReadOnlyCollection<Guid> empresasOrigen)
    {
        var errores = new List<string>();
        if (plan.Version != 1) errores.Add("version_no_admitida");
        if (plan.Tenants is null || plan.BusinessGroups is null || plan.Companies is null)
            return ["coleccion_ausente"];
        if (plan.Tenants.Any(tenant => tenant is null || tenant.TenantId == Guid.Empty)
            || plan.BusinessGroups.Any(grupo => grupo is null || grupo.BusinessGroupId == Guid.Empty || grupo.TenantId == Guid.Empty)
            || plan.Companies.Any(empresa => empresa is null || empresa.CompanyId == Guid.Empty))
            return ["identificador_invalido"];
        var tenants = plan.Tenants.Select(tenant => tenant.TenantId).ToHashSet();
        if (tenants.Count != plan.Tenants.Length) errores.Add("tenant_duplicado");
        if (plan.Tenants.Any(tenant => string.IsNullOrWhiteSpace(tenant.Code) || tenant.Code.Length > 64
                || tenant.Code.Any(character => !char.IsAsciiLetterUpper(character)
                    && !char.IsAsciiDigit(character) && character != '-')))
            errores.Add("tenant_codigo_invalido");
        if (plan.Tenants.Select(tenant => tenant.Code).Distinct(StringComparer.OrdinalIgnoreCase).Count() != plan.Tenants.Length)
            errores.Add("tenant_codigo_duplicado");
        if (plan.BusinessGroups.Select(grupo => grupo.BusinessGroupId).Distinct().Count() != plan.BusinessGroups.Length)
            errores.Add("grupo_duplicado");
        if (plan.BusinessGroups.Any(grupo => !tenants.Contains(grupo.TenantId))) errores.Add("grupo_tenant_desconocido");
        var empresas = plan.Companies.Select(empresa => empresa.CompanyId).ToHashSet();
        if (empresas.Count != plan.Companies.Length) errores.Add("empresa_duplicada");
        var origen = empresasOrigen.ToHashSet();
        if (tenants.Overlaps(empresas) || tenants.Overlaps(origen)) errores.Add("tenant_reutiliza_company_id");
        if (origen.Contains(Guid.Empty) || origen.Count != empresasOrigen.Count) errores.Add("inventario_invalido");
        if (!empresas.SetEquals(origen)) errores.Add("inventario_sin_correspondencia_exacta");
        foreach (var empresa in plan.Companies)
        {
            if (empresa.MigrationStatus != EstadoCorrespondencia.Aprobada) errores.Add("empresa_pendiente");
            if (empresa.TenantId is not { } tenantId || !tenants.Contains(tenantId))
                errores.Add("empresa_tenant_no_determinado");
            if (empresa.BusinessGroupId is { } grupoId && !plan.BusinessGroups.Any(grupo =>
                    grupo.BusinessGroupId == grupoId && grupo.TenantId == empresa.TenantId))
                errores.Add("empresa_grupo_fuera_de_tenant");
        }
        return errores.Distinct(StringComparer.Ordinal).ToArray();
    }
}