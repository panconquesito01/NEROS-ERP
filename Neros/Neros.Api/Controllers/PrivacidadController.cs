using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neros.Api.Seguridad;
using Neros.Application.Privacidad;
using Neros.Contracts.Cuenta;
using Neros.Contracts.Privacidad;

namespace Neros.Api.Controllers;

[ApiController]
[Route("api/privacidad")]
public sealed class PrivacidadController(IServicioPrivacidad privacidad) : ControllerBase
{
    [HttpGet("cookies"), AllowAnonymous]
    public Task<IReadOnlyList<DefinicionCookiePublica>> ListarCookiesAsync(CancellationToken cancellationToken) =>
        privacidad.ListarCookiesAsync(cancellationToken);

    [HttpGet("documentos/pendientes"), PermitirConDocumentosPendientes, PermitirConClaveTemporal]
    public Task<IReadOnlyList<DocumentoLegalPendiente>> ListarPendientesAsync(CancellationToken cancellationToken) =>
        privacidad.ListarPendientesAsync(User.UsuarioId(), cancellationToken);

    [HttpGet("documentos/{codigo}"), PermitirConDocumentosPendientes, PermitirConClaveTemporal]
    public async Task<ActionResult<DocumentoLegalPublicado>> ObtenerAsync(string codigo, CancellationToken cancellationToken) =>
        await privacidad.ObtenerVigenteAsync(codigo, cancellationToken) is { } documento ? Ok(documento) : NotFound();

    [HttpPost("documentos/{codigo}/aceptar"), PermitirConDocumentosPendientes, PermitirConClaveTemporal]
    public async Task<IActionResult> AceptarAsync(string codigo, SolicitudAceptacionLegal solicitud, CancellationToken cancellationToken)
    {
        var (correcto, error) = await privacidad.AceptarAsync(User.UsuarioId(), codigo, solicitud, HttpContext.ContextoCliente(), cancellationToken);
        return correcto ? NoContent() : BadRequest(new ErrorOperacion(error!));
    }
}
