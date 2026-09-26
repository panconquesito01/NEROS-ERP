using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neros.Api.Seguridad;
using Neros.Application.Administracion;
using Neros.Application.Seguridad;
using Neros.Contracts.Administracion;
using Neros.Contracts.Cuenta;

namespace Neros.Api.Controllers;

[ApiController]
[Route("api/admin/plataforma")]
public sealed class AdministracionPlataformaController(IServicioAdministracionPlataforma plataforma) : ControllerBase
{
    [HttpGet("empresas"), Authorize(Policy = Permisos.PlataformaEmpresaConsultar)]
    public Task<PaginaEmpresas> ListarEmpresasAsync([FromQuery] string? buscar, [FromQuery] int pagina = 1, [FromQuery] int tamano = 20,
        CancellationToken cancellationToken = default) =>
        plataforma.ListarEmpresasAsync(buscar, pagina, tamano, cancellationToken);

    [HttpPost("empresas"), Authorize(Policy = Permisos.PlataformaEmpresaCrear)]
    public async Task<ActionResult<EmpresaAdministrada>> CrearEmpresaAsync(
        [FromBody] SolicitudCrearEmpresa solicitud, CancellationToken cancellationToken)
    {
        var (empresa, error) = await plataforma.CrearEmpresaAsync(User.UsuarioId(), solicitud, HttpContext.ContextoCliente(), cancellationToken);
        if (error == CodigosAdministracion.EmpresaDuplicada)
            return Conflict(new ErrorOperacion(error));
        return empresa is null ? BadRequest() : Ok(empresa);
    }

    [HttpPut("empresas/{empresaId:guid}/logo"), Authorize(Policy = Permisos.PlataformaEmpresaModificar)]
    [RequestSizeLimit(600_000)]
    public async Task<IActionResult> ActualizarLogoAsync(Guid empresaId, IFormFile archivo, CancellationToken cancellationToken)
    {
        if (archivo.Length == 0) return BadRequest(new ErrorOperacion(CodigosAdministracion.LogoInvalido));
        await using var stream = archivo.OpenReadStream();
        using var memoria = new MemoryStream();
        await stream.CopyToAsync(memoria, cancellationToken);
        var (ok, error) = await plataforma.ActualizarLogoAsync(
            empresaId, memoria.ToArray(), archivo.ContentType, HttpContext.ContextoCliente(), cancellationToken);
        if (error == CodigosAdministracion.LogoInvalido) return BadRequest(new ErrorOperacion(error));
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("usuarios"), Authorize(Policy = Permisos.UsuarioCrear)]
    public async Task<ActionResult<UsuarioCreado>> CrearUsuarioAsync(
        [FromBody] SolicitudCrearUsuarioPlataforma solicitud, CancellationToken cancellationToken)
    {
        var (usuario, error) = await plataforma.CrearUsuarioAsync(
            User.UsuarioId(), solicitud, HttpContext.ContextoCliente(), cancellationToken);
        return error switch
        {
            CodigosAdministracion.CorreoDuplicado => Conflict(new ErrorOperacion(error)),
            CodigosAdministracion.RolInvalido or CodigosAdministracion.FueraEmpresa => BadRequest(new ErrorOperacion(error!)),
            _ => usuario is null ? BadRequest() : Ok(usuario)
        };
    }
}
