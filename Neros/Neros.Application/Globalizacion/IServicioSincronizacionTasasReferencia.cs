namespace Neros.Application.Globalizacion;

public interface IServicioSincronizacionTasasReferencia
{
    /// <summary>Sincroniza tasas para todas las monedas funcionales de empresas activas (y referencias mínimas si no hay Organization).</summary>
    Task<int> EjecutarCicloAsync(DateOnly? fecha, CancellationToken cancellationToken);

    /// <summary>Un par moneda/país; usado al consultar o tras cambiar configuración regional.</summary>
    Task<bool> SincronizarParAsync(string monedaDestino, string? pais, DateOnly? fecha, CancellationToken cancellationToken);
}
