namespace Neros.Contracts.Organizacion;

public sealed record ConfiguracionRegionalEmpresa(
    string Pais, string MonedaFuncional, string ZonaHoraria, string CulturaFormato, string MarcoContable);

public sealed record CorrespondenciaEmpresa(
    Guid CompanyId, Guid TenantId, string CodigoTenant, Guid? GrupoEmpresarialId, ConfiguracionRegionalEmpresa Regional);

public sealed record ContextoOrganizacion(
    string ActorId, Guid TenantId, Guid? CompanyId, Guid? BranchId, ConfiguracionRegionalEmpresa? Regional);

public static class OrganizacionAudiencias
{
    public const string Api = "neros.organization";
}

public static class PermisosOrganizacion
{
    public const string ContextoLeer = "Organization.Context.Read";
    public const string CorrespondenciaLeer = "Organization.Correspondence.Read";
}
