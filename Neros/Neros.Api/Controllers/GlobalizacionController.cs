using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Neros.Application.Globalizacion;
using Neros.Application.Seguridad;
using Neros.Contracts.Cuenta;
using Neros.Contracts.Globalizacion;

namespace Neros.Api.Controllers;

[ApiController]
[Route("api/globalizacion")]
public sealed class GlobalizacionController(
    IServicioCatalogosGlobalizacion catalogos,
    IServicioTasasCambio tasas,
    IServicioSincronizacionTasasReferencia sincronizacion,
    IOptions<OpcionesTasasCambio> opcionesTasas,
    TimeProvider reloj) : ControllerBase
{
    [HttpGet("catalogos")]
    public Task<CatalogosTransversales> CatalogosAsync([FromQuery] string? pais, CancellationToken cancellationToken) =>
        catalogos.ObtenerAsync(pais, cancellationToken);

    [HttpGet("tasas"), Authorize(Policy = Permisos.EmpresaConfiguracionConsultar)]
    public async Task<PaginaTasasCambio> TasasAsync(
        [FromQuery] string monedaDestino, [FromQuery] DateOnly? fecha, [FromQuery] string? pais,
        CancellationToken cancellationToken)
    {
        var pagina = await tasas.ListarAsync(monedaDestino, fecha, cancellationToken);
        var config = opcionesTasas.Value;
        var hoy = DateOnly.FromDateTime(reloj.GetUtcNow().UtcDateTime);
        var dia = fecha ?? hoy;
        if (config.Automatico && config.SincronizarAlConsultarSiFaltaHoy && pagina.Tasas.Count == 0 && dia == hoy)
        {
            await sincronizacion.SincronizarParAsync(monedaDestino, pais, dia, cancellationToken);
            pagina = await tasas.ListarAsync(monedaDestino, fecha, cancellationToken);
        }
        return pagina;
    }

    [HttpPost("tasas/sincronizar"), Authorize(Policy = Permisos.EmpresaConfiguracionModificar)]
    public async Task<ActionResult<ResultadoSincronizacionTasas>> SincronizarTasasAsync(
        [FromBody] SolicitudSincronizarTasas solicitud, CancellationToken cancellationToken)
    {
        var (resultado, error) = await tasas.SincronizarMercadoAsync(solicitud, cancellationToken);
        return error switch
        {
            "moneda_invalida" or "proveedor_sin_datos" => BadRequest(new ErrorOperacion(error)),
            _ => resultado is null ? BadRequest() : Ok(resultado)
        };
    }
}
