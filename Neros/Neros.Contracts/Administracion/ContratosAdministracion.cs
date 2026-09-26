using System.ComponentModel.DataAnnotations;

namespace Neros.Contracts.Administracion;

public sealed record EmpresaAdministrada(
    Guid Id, string Codigo, string Nombre, string Identificacion, bool Activa, bool TieneLogo);

public sealed record PaginaEmpresas(IReadOnlyList<EmpresaAdministrada> Empresas, int Total, int Pagina, int Tamano);

public sealed class SolicitudCrearEmpresa
{
    [Required, StringLength(20, MinimumLength = 2)]
    public string Codigo { get; set; } = string.Empty;

    [Required, StringLength(160, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(30, MinimumLength = 3)]
    public string Identificacion { get; set; } = string.Empty;
}

public sealed class SolicitudAsignarEmpresa
{
    [Required]
    public Guid EmpresaId { get; set; }

    [Required, StringLength(30)]
    public string Rol { get; set; } = "Operador";
}

public sealed class SolicitudCrearMiembroEmpresa
{
    [Required, EmailAddress, StringLength(256)]
    public string Correo { get; set; } = string.Empty;

    [Required]
    public PerfilPersonaUsuario Perfil { get; set; } = new();

    [Required, StringLength(30)]
    public string Rol { get; set; } = "Operador";

    public IReadOnlyList<string> ModulosHabilitados { get; set; } = [];
}

public sealed class SolicitudCrearUsuarioPlataforma
{
    [Required, EmailAddress, StringLength(256)]
    public string Correo { get; set; } = string.Empty;

    [Required]
    public PerfilPersonaUsuario Perfil { get; set; } = new();

    public SolicitudAsignarEmpresa? MembresiaInicial { get; set; }

    public IReadOnlyList<string> ModulosHabilitados { get; set; } = [];

    public bool AdministradorGlobal { get; set; }
}

public sealed record UsuarioEmpresaAdministrado(
    string Id, string Nombre, string Correo, string Rol, bool Activo, bool Bloqueado, bool DebeCambiarClave, DateTime? UltimoAccesoUtc);

public sealed record PaginaMiembrosEmpresa(IReadOnlyList<UsuarioEmpresaAdministrado> Miembros, int Total, int Pagina, int Tamano);

public sealed record UsuarioCreado(string Id, string ClaveTemporal);

public static class CodigosAdministracion
{
    public const string EmpresaDuplicada = "empresa_duplicada";
    public const string CorreoDuplicado = "correo_duplicado";
    public const string RolInvalido = "rol_invalido";
    public const string MembresiaDuplicada = "membresia_duplicada";
    public const string LogoInvalido = "logo_invalido";
    public const string FueraEmpresa = "fuera_empresa";
}
