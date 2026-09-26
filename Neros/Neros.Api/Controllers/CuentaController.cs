using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Neros.Api.Seguridad;
using Neros.Application.Cuenta;
using Neros.Contracts.Autenticacion;
using Neros.Contracts.Cuenta;

namespace Neros.Api.Controllers;

[ApiController]
[Route("api/cuenta")]
public sealed class CuentaController(IServicioCuenta cuenta) : ControllerBase
{
    [HttpPost("clave"), PermitirConClaveTemporal, EnableRateLimiting("cuenta")]
    public async Task<ActionResult<AccesoConcedido>> CambiarClaveAsync(SolicitudCambioClave solicitud, CancellationToken cancellationToken)
    {
        var resultado = await cuenta.CambiarClaveAsync(User.UsuarioId(), User.SesionId(), solicitud, HttpContext.ContextoCliente(), cancellationToken);
        return resultado.Acceso is null ? BadRequest(resultado.Error) : Ok(resultado.Acceso);
    }

    [HttpGet("sesiones")]
    public Task<IReadOnlyList<SesionActiva>> ListarSesionesAsync(CancellationToken cancellationToken) =>
        cuenta.ListarSesionesAsync(User.UsuarioId(), User.SesionId(), cancellationToken);

    [HttpPost("sesiones/{sesionId:guid}/cerrar")]
    public async Task<IActionResult> CerrarSesionAsync(Guid sesionId, CancellationToken cancellationToken) =>
        await cuenta.CerrarSesionAsync(User.UsuarioId(), sesionId, HttpContext.ContextoCliente(), cancellationToken) ? NoContent() : NotFound();

    [HttpPost("sesiones/cerrar-otras")]
    public async Task<SesionesCerradas> CerrarOtrasAsync(CancellationToken cancellationToken) =>
        new(await cuenta.CerrarSesionesAsync(User.UsuarioId(), User.SesionId(), HttpContext.ContextoCliente(), cancellationToken));

    [HttpPost("sesiones/cerrar-todas"), PermitirConClaveTemporal]
    public async Task<SesionesCerradas> CerrarTodasAsync(CancellationToken cancellationToken) =>
        new(await cuenta.CerrarSesionesAsync(User.UsuarioId(), null, HttpContext.ContextoCliente(), cancellationToken));
}
