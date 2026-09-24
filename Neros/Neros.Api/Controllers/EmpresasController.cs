using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Neros.Application.Autenticacion;
using Neros.Contracts.Autenticacion;

namespace Neros.Api.Controllers;

[ApiController]
[Route("api/empresas")]
public sealed class EmpresasController(ServicioEmpresas empresas) : ControllerBase
{
    private string UsuarioId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public Task<IReadOnlyList<EmpresaDisponible>> ListarAsync(CancellationToken cancellationToken) => empresas.ListarAsync(UsuarioId, cancellationToken);

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
}