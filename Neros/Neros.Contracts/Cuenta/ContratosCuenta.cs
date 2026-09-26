using System.ComponentModel.DataAnnotations;

namespace Neros.Contracts.Cuenta;

public sealed class SolicitudCambioClave
{
    [Required, StringLength(128, MinimumLength = 1)]
    public string ClaveActual { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 12)]
    public string ClaveNueva { get; set; } = string.Empty;

    [Required, Compare(nameof(ClaveNueva))]
    public string Confirmacion { get; set; } = string.Empty;
}

public sealed record SesionActiva(
    Guid Id, DateTime InicioUtc, DateTime UltimaActividadUtc, DateTimeOffset Expira, string? Ip, string? AgenteUsuario, bool Actual);

public sealed record SesionesCerradas(int Cantidad);

public sealed record UsuarioAdministrado(
    string Id, string Nombre, string Correo, bool Activo, bool Bloqueado, bool DebeCambiarClave, bool AdministradorGlobal, DateTime? UltimoAccesoUtc);

public sealed record PaginaUsuarios(IReadOnlyList<UsuarioAdministrado> Usuarios, int Total, int Pagina, int Tamano);

public sealed record ClaveTemporal(string Clave);

public sealed record MembresiaUsuarioPlataforma(
    Guid EmpresaId,
    string EmpresaCodigo,
    string EmpresaNombre,
    string Rol,
    IReadOnlyList<string> ModulosHabilitados);

public sealed class MembresiaUsuarioPlataformaSolicitud
{
    public Guid EmpresaId { get; set; }

    [Required, StringLength(30)]
    public string Rol { get; set; } = "Operador";

    public IReadOnlyList<string> ModulosHabilitados { get; set; } = [];
}

public sealed record DetalleUsuarioPlataforma(
    string Id,
    string Correo,
    bool Activo,
    bool Bloqueado,
    bool DebeCambiarClave,
    bool AdministradorGlobal,
    Neros.Contracts.Administracion.PerfilPersonaUsuario Perfil,
    IReadOnlyList<MembresiaUsuarioPlataforma> Membresias);

public sealed class SolicitudActualizarUsuarioPlataforma
{
    [Required]
    public Neros.Contracts.Administracion.PerfilPersonaUsuario Perfil { get; set; } = new();

    public bool AdministradorGlobal { get; set; }

    public IReadOnlyList<MembresiaUsuarioPlataformaSolicitud> Membresias { get; set; } = [];
}

public sealed class SolicitudEstadoUsuarioPlataforma
{
    public bool Activo { get; set; }
}

/// <summary>Error de negocio devuelto con 400. <c>Codigo</c> es estable; <c>Detalles</c> lista reglas incumplidas.</summary>
public sealed record ErrorOperacion(string Codigo, IReadOnlyList<string>? Detalles = null);

public static class CodigosCuenta
{
    public const string ClaveActual = "clave_actual";
    public const string MismaClave = "misma_clave";
    public const string Politica = "politica";
    public const string Bloqueado = "bloqueado";
    public const string CambioClaveRequerido = "cambio_clave_requerido";
    public const string PropiaCuenta = "propia_cuenta";
}
