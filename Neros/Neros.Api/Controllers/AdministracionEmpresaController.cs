using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neros.Api.Seguridad;
using Neros.Application.Administracion;
using Neros.Application.Cuenta;
using Neros.Application.Seguridad;
using Neros.Contracts.Administracion;
using Neros.Contracts.Cuenta;

namespace Neros.Api.Controllers;

[ApiController]
[Route("api/admin/empresa")]
public sealed class AdministracionEmpresaController(IServicioAdministracionEmpresa administracion) : ControllerBase
{
    [HttpGet("miembros"), Authorize(Policy = Permisos.EmpresaMiembroAdministrar)]
    public async Task<ActionResult<PaginaMiembrosEmpresa>> ListarMiembrosAsync(
        [FromQuery] string? buscar, [FromQuery] int pagina = 1, [FromQuery] int tamano = 20, CancellationToken cancellationToken = default)
    {
        if (!HttpContext.PuedeAdministrarEmpresa()) return Forbid();
        var empresaId = HttpContext.EmpresaActivaRequerida();
        try
        {
            return await administracion.ListarMiembrosAsync(User.UsuarioId(), empresaId, buscar, pagina, tamano, cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("miembros"), Authorize(Policy = Permisos.EmpresaMiembroAdministrar)]
    public async Task<ActionResult<UsuarioCreado>> CrearMiembroAsync(
        [FromBody] SolicitudCrearMiembroEmpresa solicitud, CancellationToken cancellationToken)
    {
        if (!HttpContext.PuedeAdministrarEmpresa()) return Forbid();
        var empresaId = HttpContext.EmpresaActivaRequerida();
        try
        {
            var (usuario, error) = await administracion.CrearMiembroAsync(
                User.UsuarioId(), empresaId, solicitud, HttpContext.ContextoCliente(), cancellationToken);
            return error switch
            {
                CodigosAdministracion.CorreoDuplicado or CodigosAdministracion.MembresiaDuplicada => Conflict(new ErrorOperacion(error)),
                CodigosAdministracion.RolInvalido => BadRequest(new ErrorOperacion(error)),
                _ => usuario is null ? BadRequest() : Ok(usuario)
            };
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("miembros/{usuarioId}/restablecer-clave"), Authorize(Policy = Permisos.EmpresaMiembroAdministrar)]
    public async Task<ActionResult<ClaveTemporal>> RestablecerClaveAsync(string usuarioId, CancellationToken cancellationToken)
    {
        if (!HttpContext.PuedeAdministrarEmpresa()) return Forbid();
        var empresaId = HttpContext.EmpresaActivaRequerida();
        try
        {
            var (estado, clave) = await administracion.RestablecerClaveAsync(
                User.UsuarioId(), empresaId, usuarioId, HttpContext.ContextoCliente(), cancellationToken);
            return estado switch
            {
                EstadoAdministracion.Correcto => Ok(new ClaveTemporal(clave!)),
                EstadoAdministracion.PropiaCuenta => BadRequest(new ErrorOperacion(CodigosCuenta.PropiaCuenta)),
                _ => NotFound()
            };
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("miembros/{usuarioId}/desbloquear"), Authorize(Policy = Permisos.EmpresaMiembroAdministrar)]
    public async Task<IActionResult> DesbloquearAsync(string usuarioId, CancellationToken cancellationToken)
    {
        if (!HttpContext.PuedeAdministrarEmpresa()) return Forbid();
        var empresaId = HttpContext.EmpresaActivaRequerida();
        try
        {
            return await administracion.DesbloquearAsync(User.UsuarioId(), empresaId, usuarioId, HttpContext.ContextoCliente(), cancellationToken)
                == EstadoAdministracion.Correcto ? NoContent() : NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
