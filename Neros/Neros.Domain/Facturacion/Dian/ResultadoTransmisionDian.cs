namespace Neros.Domain.Facturacion.Dian;

public sealed record ResultadoTransmisionDian(
    bool Exito,
    string? TrackId,
    string? Cufe,
    string? CodigoRespuesta,
    string? MensajeAutoridad);

public interface IEnvioDocumentoElectronicoDian
{
    Task<ResultadoTransmisionDian> EnviarAsync(
        AmbienteDian ambiente,
        string hashXml,
        string idempotencyKey,
        CancellationToken cancellationToken = default);
}
