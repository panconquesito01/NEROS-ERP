namespace Neros.Organization.Persistence;

public sealed class Tenant
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public sealed class GrupoEmpresarial
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public sealed class EmpresaOrganizacion
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? GrupoEmpresarialId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public bool Activa { get; set; }
    public string Pais { get; set; } = string.Empty;
    public string MonedaFuncional { get; set; } = string.Empty;
    public string ZonaHoraria { get; set; } = string.Empty;
    public string CulturaFormato { get; set; } = string.Empty;
    public string MarcoContable { get; set; } = string.Empty;
    public Tenant Tenant { get; set; } = null!;
}

public sealed class Sucursal
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EmpresaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activa { get; set; }
}
