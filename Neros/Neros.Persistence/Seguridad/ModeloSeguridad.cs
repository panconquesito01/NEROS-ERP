using Microsoft.AspNetCore.Identity;

namespace Neros.Persistence.Seguridad;

public sealed class Usuario : IdentityUser
{
    public string Nombre { get; set; } = string.Empty;
    public string? PrimerNombre { get; set; }
    public string? SegundoNombre { get; set; }
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }
    public string? TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? Ciudad { get; set; }
    public string? Direccion { get; set; }
    public bool Activo { get; set; } = true;
    public bool DebeCambiarClave { get; set; }
}

public sealed class Empresa
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;
    public byte[]? ImagenLogo { get; set; }
    public string? ImagenLogoContentType { get; set; }
}

public sealed class UsuarioEmpresa
{
    public string UsuarioId { get; set; } = string.Empty;
    public Usuario Usuario { get; set; } = null!;
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
    public string Rol { get; set; } = "Operador";
    public string? ModulosHabilitados { get; set; }
    public bool Activo { get; set; } = true;
}

public sealed class Sesion
{
    public string TokenHash { get; set; } = string.Empty;
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UsuarioId { get; set; } = string.Empty;
    public Usuario Usuario { get; set; } = null!;
    public string SelloSeguridad { get; set; } = string.Empty;
    public DateTimeOffset Expira { get; set; }
    public DateTime InicioUtc { get; set; }
    public DateTime UltimaActividadUtc { get; set; }
    public string? Ip { get; set; }
    public string? AgenteUsuario { get; set; }
    public DateTime? RevocadaEnUtc { get; set; }
    public string? MotivoRevocacion { get; set; }
}

public sealed class EventoAcceso
{
    public long Id { get; set; }
    public string? UsuarioId { get; set; }
    public Guid? EmpresaId { get; set; }
    public DateTimeOffset Fecha { get; set; }
    public string Accion { get; set; } = string.Empty;
}

/// <summary>Fila de <c>auditoria.Evento</c>, tabla ledger de solo insercion. Nunca guarda claves ni tokens.</summary>
public sealed class EventoAuditoria
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime FechaUtc { get; set; }
    public string Modulo { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string Resultado { get; set; } = string.Empty;
    public string? ActorId { get; set; }
    public Guid? EmpresaId { get; set; }
    public string? Entidad { get; set; }
    public string? EntidadId { get; set; }
    public string? Ip { get; set; }
    public string? AgenteUsuario { get; set; }
    public string? CorrelationId { get; set; }
    public string? Detalle { get; set; }
}
