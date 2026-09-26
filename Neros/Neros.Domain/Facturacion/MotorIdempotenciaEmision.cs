namespace Neros.Domain.Facturacion;

/// <summary>Idempotencia de emision y transmision (plan §56).</summary>
public static class MotorIdempotenciaEmision
{
    public static ResultadoIdempotencia Evaluar(
        string? claveExistente,
        string claveSolicitud,
        string hashExistente,
        string hashSolicitud,
        string? respuestaExistente)
    {
        if (string.IsNullOrWhiteSpace(claveSolicitud))
            throw new ArgumentException("Idempotency-Key requerida.", nameof(claveSolicitud));
        if (claveExistente is null)
            return new ResultadoIdempotencia(false, null);
        if (!string.Equals(claveExistente, claveSolicitud, StringComparison.Ordinal))
            throw new InvalidOperationException("Conflicto de idempotencia: clave distinta para la misma operacion.");
        if (!string.Equals(hashExistente, hashSolicitud, StringComparison.Ordinal))
            throw new InvalidOperationException("Conflicto de idempotencia: mismo key con solicitud distinta.");
        return new ResultadoIdempotencia(true, respuestaExistente);
    }
}
