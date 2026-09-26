using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neros.Api.Seguridad;
using Neros.Application.Globalizacion;
using Neros.Application.Organizacion;
using Neros.Application.Seguridad;
using Neros.Contracts.Cuenta;
using Neros.Contracts.Organizacion;

namespace Neros.Api.Controllers;

[ApiController]
[Route("api/empresas/{empresaId:guid}/configuracion-regional")]
public sealed class EmpresaConfiguracionController(
    IServicioConfiguracionRegional configuracion,
    IServicioSincronizacionTasasReferencia sincronizacionTasas) : ControllerBase
{
    [HttpGet, Authorize(Policy = Permisos.EmpresaConfiguracionConsultar)]
    public async Task<ActionResult<ConfiguracionRegionalEmpresa>> ObtenerAsync(Guid empresaId, CancellationToken cancellationToken)
    {
        if (!HttpContext.CoincideEmpresaActiva(empresaId)) return Forbid();
        var regional = await configuracion.ObtenerAsync(User.UsuarioId(), empresaId, cancellationToken);
        return regional is null ? NotFound() : Ok(regional);
    }

    [HttpPut, Authorize(Policy = Permisos.EmpresaConfiguracionModificar)]
    public async Task<ActionResult<ConfiguracionRegionalEmpresa>> ActualizarAsync(
        Guid empresaId, [FromBody] ConfiguracionRegionalEmpresa solicitud, CancellationToken cancellationToken)
    {
        if (!HttpContext.CoincideEmpresaActiva(empresaId)) return Forbid();
        var (actualizada, error) = await configuracion.ActualizarAsync(User.UsuarioId(), empresaId, solicitud, cancellationToken);
        if (error is not null)
        {
            return error switch
            {
                "sin_acceso" or "sin_organizacion" => NotFound(new ErrorOperacion(error)),
                "pais_invalido" or "moneda_invalida" or "zona_invalida" or "cultura_invalida" or "marco_invalido"
                    => BadRequest(new ErrorOperacion(error)),
                _ => BadRequest(new ErrorOperacion(error))
            };
        }
        if (actualizada is null) return BadRequest();
        await sincronizacionTasas.SincronizarParAsync(actualizada.MonedaFuncional, actualizada.Pais, fecha: null, cancellationToken);
        return Ok(actualizada);
    }
}
