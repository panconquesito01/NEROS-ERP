namespace Neros.Domain.Nomina.Electronica;

/// <summary>Estados alineados con documento electronico DIAN (plan §50).</summary>
public enum EstadoNominaElectronica
{
    Borrador,
    Generado,
    Firmado,
    Enviado,
    Validado,
    Rechazado,
    Entregado,
    Anulado
}

public enum AmbienteDianNomina
{
    Habilitacion,
    Produccion
}

public sealed record ResultadoTransmisionNominaElectronica(
    bool Exito,
    string? TrackId,
    string? Cune,
    string? CodigoRespuesta,
    string? MensajeAutoridad);

public interface IEnvioNominaElectronicaDian
{
    Task<ResultadoTransmisionNominaElectronica> EnviarAsync(
        AmbienteDianNomina ambiente,
        string hashXml,
        string idempotencyKey,
        CancellationToken cancellationToken = default);
}
