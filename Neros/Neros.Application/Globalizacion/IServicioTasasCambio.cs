using Neros.Contracts.Globalizacion;

namespace Neros.Application.Globalizacion;

public interface IServicioTasasCambio
{
    Task<PaginaTasasCambio> ListarAsync(string monedaDestino, DateOnly? fecha, CancellationToken cancellationToken);
    Task<(ResultadoSincronizacionTasas? Resultado, string? Error)> SincronizarMercadoAsync(
        SolicitudSincronizarTasas solicitud, CancellationToken cancellationToken);
    Task<TasaCambioDia?> ObtenerAsync(string origen, string destino, DateOnly fecha, CancellationToken cancellationToken);
}
