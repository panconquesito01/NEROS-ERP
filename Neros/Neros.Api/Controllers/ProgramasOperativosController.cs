using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neros.Application.Autenticacion;
using Neros.Application.Operacion;
using PermisosPrograma = Neros.Application.Operacion.PermisosProgramaOperativo;
using Neros.Application.Seguridad;
using Neros.Contracts.Operacion;
using Neros.Contracts.Seguridad;

namespace Neros.Api.Controllers;

[ApiController]
[Route("api/empresas/{empresaId:guid}/programas")]
public sealed class ProgramasOperativosController(
    IServicioProgramasOperativos programas,
    ServicioEmpresas empresas,
    IRepositorioEmpresas membresias) : ControllerBase
{
    private string UsuarioId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("{programa}")]
    public async Task<ActionResult<PaginaProgramaOperativo>> ListarAsync(
        Guid empresaId, string programa, [FromQuery] int pagina = 1, [FromQuery] int tamano = 25, CancellationToken cancellationToken = default)
    {
        if (!await AutorizadoAsync(empresaId, programa, consultar: true, cancellationToken))
            return Forbid();

        var clave = NormalizarPrograma(programa);
        return Ok(await programas.ListarAsync(empresaId, clave, Math.Max(1, pagina), Math.Clamp(tamano, 1, 100), cancellationToken));
    }

    [HttpPost("{programa}/borrador")]
    public async Task<ActionResult<BorradorProgramaCreado>> CrearBorradorAsync(
        Guid empresaId, string programa, CancellationToken cancellationToken)
    {
        if (!await AutorizadoAsync(empresaId, programa, consultar: false, cancellationToken))
            return Forbid();

        var clave = NormalizarPrograma(programa);
        var (creado, error) = await programas.CrearBorradorAsync(empresaId, clave, cancellationToken);
        return error switch
        {
            CodigosProgramaOperativo.ModuloNoConfigurado => StatusCode(StatusCodes.Status503ServiceUnavailable),
            CodigosProgramaOperativo.ProgramaDesconocido => BadRequest(),
            _ => Ok(creado)
        };
    }

    [HttpGet("{programa}/{id:guid}")]
    public async Task<ActionResult<DetalleProgramaOperativo>> ObtenerDetalleAsync(
        Guid empresaId, string programa, Guid id, CancellationToken cancellationToken)
    {
        if (!await AutorizadoAsync(empresaId, programa, consultar: true, cancellationToken))
            return Forbid();

        var clave = NormalizarPrograma(programa);
        var (detalle, error) = await programas.ObtenerDetalleAsync(empresaId, clave, id, cancellationToken);
        return error switch
        {
            CodigosProgramaOperativo.NoEncontrado => NotFound(),
            CodigosProgramaOperativo.ModuloNoConfigurado => StatusCode(StatusCodes.Status503ServiceUnavailable),
            CodigosProgramaOperativo.ProgramaDesconocido => BadRequest(),
            _ => Ok(detalle)
        };
    }

    [HttpPut("{programa}/{id:guid}")]
    public async Task<IActionResult> GuardarAsync(
        Guid empresaId, string programa, Guid id, [FromBody] SolicitudGuardarPrograma solicitud, CancellationToken cancellationToken)
    {
        if (!await AutorizadoAsync(empresaId, programa, consultar: false, cancellationToken))
            return Forbid();

        var clave = NormalizarPrograma(programa);
        var error = await programas.GuardarAsync(empresaId, clave, id, solicitud, cancellationToken);
        return error switch
        {
            CodigosProgramaOperativo.NoEncontrado => NotFound(),
            CodigosProgramaOperativo.NoEditable => BadRequest(),
            CodigosProgramaOperativo.ModuloNoConfigurado => StatusCode(StatusCodes.Status503ServiceUnavailable),
            CodigosProgramaOperativo.ProgramaDesconocido => BadRequest(),
            null => NoContent(),
            _ => BadRequest()
        };
    }

    [HttpPost("{programa}/{id:guid}/lineas")]
    public async Task<ActionResult<LineaProgramaOperativo>> AgregarLineaAsync(
        Guid empresaId, string programa, Guid id, [FromBody] SolicitudLineaPrograma solicitud, CancellationToken cancellationToken)
    {
        if (!await AutorizadoAsync(empresaId, programa, consultar: false, cancellationToken))
            return Forbid();

        var clave = NormalizarPrograma(programa);
        var (linea, error) = await programas.AgregarLineaAsync(empresaId, clave, id, solicitud, cancellationToken);
        return error switch
        {
            CodigosProgramaOperativo.NoEditable => BadRequest(),
            CodigosProgramaOperativo.ModuloNoConfigurado => StatusCode(StatusCodes.Status503ServiceUnavailable),
            CodigosProgramaOperativo.ProgramaDesconocido => BadRequest(),
            _ => Ok(linea)
        };
    }

    [HttpPost("{programa}/{id:guid}/acciones/{accion}")]
    public async Task<IActionResult> EjecutarAccionAsync(
        Guid empresaId, string programa, Guid id, string accion, CancellationToken cancellationToken)
    {
        if (!await AutorizadoAsync(empresaId, programa, consultar: false, cancellationToken))
            return Forbid();

        var clave = NormalizarPrograma(programa);
        var error = await programas.EjecutarAccionAsync(empresaId, clave, id, accion, cancellationToken);
        return error switch
        {
            CodigosProgramaOperativo.NoEncontrado => NotFound(),
            CodigosProgramaOperativo.NoEditable => BadRequest(),
            CodigosProgramaOperativo.ModuloNoConfigurado => StatusCode(StatusCodes.Status503ServiceUnavailable),
            CodigosProgramaOperativo.ProgramaDesconocido => BadRequest(),
            null => NoContent(),
            _ => BadRequest()
        };
    }

    private async Task<bool> AutorizadoAsync(Guid empresaId, string programa, bool consultar, CancellationToken cancellationToken)
    {
        if (await empresas.ConsultarAutorizadaAsync(UsuarioId, empresaId, cancellationToken) is null)
            return false;
        var clave = NormalizarPrograma(programa);
        if (ModulosEmpresa.ClaveModuloDesdePrograma(clave) is { } modulo
            && !await membresias.ModuloPermitidoAsync(UsuarioId, empresaId, modulo, cancellationToken))
            return false;
        var permiso = consultar ? PermisosPrograma.Consultar(clave) : PermisosPrograma.Gestionar(clave);
        return permiso is null || User.HasClaim(CodigosPermiso.Claim, permiso);
    }

    private static string NormalizarPrograma(string programa)
    {
        var limpio = programa.Trim().ToLowerInvariant();
        var separador = limpio.IndexOf('-');
        return separador < 0 ? limpio : $"{limpio[..separador]}/{limpio[(separador + 1)..]}";
    }
}
