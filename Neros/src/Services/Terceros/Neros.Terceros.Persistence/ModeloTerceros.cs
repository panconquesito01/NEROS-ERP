namespace Neros.Terceros.Persistence;

public sealed class TerceroEntidad
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Tipo { get; set; } = "";
    public string RazonSocial { get; set; } = "";
    public string? NombreComercial { get; set; }
    public bool Activo { get; set; } = true;
    public ICollection<IdentificacionEntidad> Identificaciones { get; set; } = [];
    public ICollection<RolEntidad> Roles { get; set; } = [];
}

public sealed class IdentificacionEntidad
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TerceroId { get; set; }
    public string Pais { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Numero { get; set; } = "";
    public string? DigitoVerificacion { get; set; }
    public bool EsPrincipal { get; set; }
    public TerceroEntidad? Tercero { get; set; }
}

public sealed class RolEntidad
{
    public Guid TerceroId { get; set; }
    public string Rol { get; set; } = "";
    public TerceroEntidad? Tercero { get; set; }
}
