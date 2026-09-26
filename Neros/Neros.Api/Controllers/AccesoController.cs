using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Neros.Api.Seguridad;
using Neros.Application.Autenticacion;
using Neros.Contracts.Autenticacion;

namespace Neros.Api.Controllers;

[ApiController]
[Route("api/acceso")]
public sealed class AccesoController(IServicioIdentidad identidad) : ControllerBase
{
    [HttpPost("login"), AllowAnonymous, EnableRateLimiting("acceso")]
    public async Task<ActionResult<AccesoConcedido>> IniciarAsync(SolicitudAcceso solicitud, CancellationToken cancellationToken)
    {
        var resultado = await identidad.IniciarSesionAsync(solicitud, HttpContext.ContextoCliente(), cancellationToken);
        return resultado is null ? Unauthorized() : Ok(resultado);
    }

    [HttpGet("yo"), PermitirConClaveTemporal, PermitirConDocumentosPendientes]
    public ActionResult<UsuarioActual> Consultar() => User.UsuarioActual();

    [HttpPost("logout"), PermitirConClaveTemporal, PermitirConDocumentosPendientes]
    public async Task<IActionResult> CerrarAsync(CancellationToken cancellationToken)
    {
        await identidad.CerrarSesionAsync(Request.Headers.Authorization.ToString()[7..], HttpContext.ContextoCliente(), cancellationToken);
        return NoContent();
    }
}
