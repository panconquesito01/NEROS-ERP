using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
        var resultado = await identidad.IniciarSesionAsync(solicitud, cancellationToken);
        return resultado is null ? Unauthorized() : Ok(resultado);
    }

    [HttpGet("yo")]
    public ActionResult<UsuarioActual> Consultar() => new UsuarioActual(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!, User.Identity!.Name!, User.FindFirstValue(ClaimTypes.Email)!);

    [HttpPost("logout")]
    public async Task<IActionResult> CerrarAsync(CancellationToken cancellationToken)
    {
        await identidad.CerrarSesionAsync(Request.Headers.Authorization.ToString()[7..], cancellationToken);
        return NoContent();
    }
}