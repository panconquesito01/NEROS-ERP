using Microsoft.AspNetCore.Identity;

namespace Neros.Persistence.Seguridad;

public sealed class Usuario : IdentityUser
{
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

public sealed class Empresa
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;
}

public sealed class UsuarioEmpresa
{
    public string UsuarioId { get; set; } = string.Empty;
    public Usuario Usuario { get; set; } = null!;
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
    public string Rol { get; set; } = "Operador";
    public bool Activo { get; set; } = true;
}

public sealed class Sesion
{
    public string TokenHash { get; set; } = string.Empty;
    public string UsuarioId { get; set; } = string.Empty;
    public Usuario Usuario { get; set; } = null!;
    public string SelloSeguridad { get; set; } = string.Empty;
    public DateTimeOffset Expira { get; set; }
}

public sealed class EventoAcceso
{
    public long Id { get; set; }
    public string? UsuarioId { get; set; }
    public Guid? EmpresaId { get; set; }
    public DateTimeOffset Fecha { get; set; }
    public string Accion { get; set; } = string.Empty;
}