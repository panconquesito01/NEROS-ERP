using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neros.Api.Seguridad;
using Neros.Contracts.Seguridad;
using Neros.Application.Cuenta;
using Neros.Application.Seguridad;
using Neros.Contracts.Administracion;
using Neros.Contracts.Cuenta;

namespace Neros.Api.Controllers;

[ApiController]
[Route("api/admin/usuarios")]
public sealed class AdministracionUsuariosController(IServicioAdministracionUsuarios administracion) : ControllerBase
{
    [HttpGet, Authorize(Policy = Permisos.UsuarioConsultar)]
    public async Task<ActionResult<PaginaUsuarios>> ListarAsync([FromQuery] string? buscar, [FromQuery] int pagina = 1, [FromQuery] int tamano = 20,
        CancellationToken cancellationToken = default)
    {
        if (!HttpContext.User.HasClaim(CodigosPermiso.Claim, CodigosPermiso.UsuarioConsultar))
            return Forbid();
        return await administracion.ListarAsync(buscar, pagina, tamano, cancellationToken);
    }

    [HttpPost("{usuarioId}/restablecer-clave"), Authorize(Policy = Permisos.UsuarioRestablecerClave)]
    public async Task<ActionResult<ClaveTemporal>> RestablecerClaveAsync(string usuarioId, CancellationToken cancellationToken)
    {
        var (estado, clave) = await administracion.RestablecerClaveAsync(User.UsuarioId(), usuarioId, HttpContext.ContextoCliente(), cancellationToken);
        return estado switch
        {
            EstadoAdministracion.Correcto => Ok(new ClaveTemporal(clave!)),
            EstadoAdministracion.PropiaCuenta => BadRequest(new ErrorOperacion(CodigosCuenta.PropiaCuenta)),
            _ => NotFound()
        };
    }

    [HttpPost("{usuarioId}/desbloquear"), Authorize(Policy = Permisos.UsuarioDesbloquear)]
    public async Task<IActionResult> DesbloquearAsync(string usuarioId, CancellationToken cancellationToken) =>
        await administracion.DesbloquearAsync(User.UsuarioId(), usuarioId, HttpContext.ContextoCliente(), cancellationToken)
            == EstadoAdministracion.Correcto ? NoContent() : NotFound();

    [HttpGet("{usuarioId}"), Authorize(Policy = Permisos.UsuarioConsultar)]
    public async Task<ActionResult<DetalleUsuarioPlataforma>> ObtenerAsync(string usuarioId, CancellationToken cancellationToken)
    {
        var detalle = await administracion.ObtenerAsync(usuarioId, cancellationToken);
        return detalle is null ? NotFound() : detalle;
    }

    [HttpPut("{usuarioId}"), Authorize(Policy = Permisos.UsuarioModificar)]
    public async Task<IActionResult> ActualizarAsync(string usuarioId, [FromBody] SolicitudActualizarUsuarioPlataforma solicitud, CancellationToken cancellationToken)
    {
        var estado = await administracion.ActualizarAsync(User.UsuarioId(), usuarioId, solicitud, HttpContext.ContextoCliente(), cancellationToken);
        return estado switch
        {
            EstadoAdministracion.Correcto => NoContent(),
            EstadoAdministracion.RolInvalido => BadRequest(new ErrorOperacion(CodigosAdministracion.RolInvalido)),
            EstadoAdministracion.FueraEmpresa => BadRequest(new ErrorOperacion(CodigosAdministracion.FueraEmpresa)),
            _ => NotFound()
        };
    }

    [HttpPut("{usuarioId}/estado"), Authorize(Policy = Permisos.UsuarioModificar)]
    public async Task<IActionResult> CambiarEstadoAsync(string usuarioId, [FromBody] SolicitudEstadoUsuarioPlataforma solicitud, CancellationToken cancellationToken)
    {
        var estado = await administracion.CambiarEstadoAsync(User.UsuarioId(), usuarioId, solicitud.Activo, HttpContext.ContextoCliente(), cancellationToken);
        return estado switch
        {
            EstadoAdministracion.Correcto => NoContent(),
            EstadoAdministracion.PropiaCuenta => BadRequest(new ErrorOperacion(CodigosCuenta.PropiaCuenta)),
            _ => NotFound()
        };
    }

    [HttpDelete("{usuarioId}"), Authorize(Policy = Permisos.UsuarioEliminar)]
    public async Task<IActionResult> EliminarAsync(string usuarioId, CancellationToken cancellationToken)
    {
        var estado = await administracion.EliminarPermanenteAsync(User.UsuarioId(), usuarioId, HttpContext.ContextoCliente(), cancellationToken);
        return estado switch
        {
            EstadoAdministracion.Correcto => NoContent(),
            EstadoAdministracion.PropiaCuenta => BadRequest(new ErrorOperacion(CodigosCuenta.PropiaCuenta)),
            _ => NotFound()
        };
    }
}
