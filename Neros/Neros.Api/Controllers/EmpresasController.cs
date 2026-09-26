using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Neros.Application.Autenticacion;
using Neros.Contracts.Autenticacion;
using Neros.Persistence;

namespace Neros.Api.Controllers;

[ApiController]
[Route("api/empresas")]
public sealed class EmpresasController(ServicioEmpresas empresas, NerosDbContext database) : ControllerBase
{
    private string UsuarioId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public Task<IReadOnlyList<EmpresaDisponible>> ListarAsync(CancellationToken cancellationToken) => empresas.ListarAsync(UsuarioId, cancellationToken);

    [HttpGet("{empresaId:guid}")]
    public async Task<ActionResult<EmpresaDisponible>> ConsultarAsync(Guid empresaId, CancellationToken cancellationToken)
    {
        var empresa = await empresas.ConsultarAutorizadaAsync(UsuarioId, empresaId, cancellationToken);
        return empresa is null ? NotFound() : Ok(empresa);
    }

    [HttpPost("{empresaId:guid}/seleccionar")]
    public async Task<ActionResult<EmpresaDisponible>> SeleccionarAsync(Guid empresaId, CancellationToken cancellationToken)
    {
        var empresa = await empresas.SeleccionarAsync(UsuarioId, empresaId, cancellationToken);
        return empresa is null ? NotFound() : Ok(empresa);
    }

    [HttpGet("{empresaId:guid}/inicio")]
    public async Task<ActionResult<InicioEmpresa>> InicioAsync(Guid empresaId, CancellationToken cancellationToken)
    {
        var inicio = await empresas.ObtenerInicioAsync(UsuarioId, empresaId, cancellationToken);
        return inicio is null ? NotFound() : Ok(inicio);
    }

    [HttpGet("{empresaId:guid}/logo")]
    public async Task<IActionResult> LogoAsync(Guid empresaId, CancellationToken cancellationToken)
    {
        if (await empresas.ConsultarAutorizadaAsync(UsuarioId, empresaId, cancellationToken) is null)
            return NotFound();
        var logo = await database.Empresas.AsNoTracking()
            .Where(e => e.Id == empresaId && e.ImagenLogo != null)
            .Select(e => new { e.ImagenLogo, e.ImagenLogoContentType })
            .SingleOrDefaultAsync(cancellationToken);
        if (logo?.ImagenLogo is null || logo.ImagenLogoContentType is null)
            return NotFound();
        return File(logo.ImagenLogo, logo.ImagenLogoContentType);
    }
}