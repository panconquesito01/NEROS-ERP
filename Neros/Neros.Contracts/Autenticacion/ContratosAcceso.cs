using System.ComponentModel.DataAnnotations;

namespace Neros.Contracts.Autenticacion;

public sealed class SolicitudAcceso
{
    [Required(ErrorMessage = "Escribe tu correo."), EmailAddress(ErrorMessage = "Revisa el correo."), MaxLength(256)]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe tu clave."), StringLength(128, MinimumLength = 1)]
    public string Clave { get; set; } = string.Empty;

    public bool Recordarme { get; set; }
}

public sealed record UsuarioActual(
    string Id, string Nombre, string Correo, bool DebeCambiarClave = false, IReadOnlyList<string>? Permisos = null,
    IReadOnlyList<string>? DocumentosLegalesPendientes = null);
public sealed record AccesoConcedido(string Token, DateTimeOffset Expira, UsuarioActual Usuario);
public sealed record EmpresaDisponible(
    Guid Id,
    string Codigo,
    string Nombre,
    string Identificacion,
    string Rol,
    bool TieneLogo = false,
    IReadOnlyList<string>? ModulosHabilitados = null);
public sealed record ActividadAcceso(DateTimeOffset Fecha, string Accion);
/// <summary>Cabeceras con las que el BFF informa la IP y el agente del navegador. Son informativas, no prueba de origen.</summary>
public static class CabecerasCliente
{
    public const string Ip = "X-Neros-Client-Ip";
    public const string Agente = "X-Neros-Client-Agent";
    public const string Empresa = "X-Neros-Company-Id";
}

public sealed record InicioEmpresa(EmpresaDisponible Empresa, IReadOnlyList<ActividadAcceso> Actividad);