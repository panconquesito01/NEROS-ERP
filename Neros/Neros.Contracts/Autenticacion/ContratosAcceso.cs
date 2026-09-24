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

public sealed record UsuarioActual(string Id, string Nombre, string Correo);
public sealed record AccesoConcedido(string Token, DateTimeOffset Expira, UsuarioActual Usuario);
public sealed record EmpresaDisponible(Guid Id, string Codigo, string Nombre, string Identificacion, string Rol);
public sealed record ActividadAcceso(DateTimeOffset Fecha, string Accion);
public sealed record InicioEmpresa(EmpresaDisponible Empresa, IReadOnlyList<ActividadAcceso> Actividad);