using Neros.Contracts.Operacion;

namespace Neros.Application.Operacion;

public interface IServicioProgramasOperativos
{
    Task<PaginaProgramaOperativo> ListarAsync(Guid empresaId, string programa, int pagina, int tamano, CancellationToken cancellationToken);
    Task<(BorradorProgramaCreado? Creado, string? Error)> CrearBorradorAsync(Guid empresaId, string programa, CancellationToken cancellationToken);
    Task<(DetalleProgramaOperativo? Detalle, string? Error)> ObtenerDetalleAsync(Guid empresaId, string programa, Guid id, CancellationToken cancellationToken);
    Task<string?> GuardarAsync(Guid empresaId, string programa, Guid id, SolicitudGuardarPrograma solicitud, CancellationToken cancellationToken);
    Task<(LineaProgramaOperativo? Linea, string? Error)> AgregarLineaAsync(Guid empresaId, string programa, Guid id, SolicitudLineaPrograma solicitud, CancellationToken cancellationToken);
    Task<string?> EjecutarAccionAsync(Guid empresaId, string programa, Guid id, string accion, CancellationToken cancellationToken);
}
