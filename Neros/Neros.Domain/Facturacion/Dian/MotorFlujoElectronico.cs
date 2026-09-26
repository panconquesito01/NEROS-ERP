using Neros.Domain.Facturacion.Dian;

namespace Neros.Domain.Facturacion;

/// <summary>Orquestacion pura del flujo electronico post-emision.</summary>
public static class MotorFlujoElectronico
{
    public static (EstadoDocumentoElectronico NuevoEstado, string HashXml) Generar(EstadoDocumentoElectronico actual, string contenidoXml)
    {
        if (string.IsNullOrWhiteSpace(contenidoXml))
            throw new ArgumentException("Contenido XML requerido.", nameof(contenidoXml));
        var nuevo = MaquinaEstadosDocumentoElectronico.GenerarXml(actual);
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(contenidoXml)));
        return (nuevo, hash);
    }

    public static EstadoDocumentoElectronico Firmar(EstadoDocumentoElectronico actual) =>
        MaquinaEstadosDocumentoElectronico.Firmar(actual);

    public static async Task<(EstadoDocumentoElectronico NuevoEstado, ResultadoTransmisionDian Resultado)> EnviarAsync(
        EstadoDocumentoElectronico actual,
        AmbienteDian ambiente,
        string hashXml,
        string idempotencyKey,
        IEnvioDocumentoElectronicoDian envio,
        CancellationToken cancellationToken = default)
    {
        var enviado = MaquinaEstadosDocumentoElectronico.Enviar(actual);
        var resultado = await envio.EnviarAsync(ambiente, hashXml, idempotencyKey, cancellationToken).ConfigureAwait(false);
        if (!resultado.Exito)
            return (MaquinaEstadosDocumentoElectronico.MarcarRechazado(enviado), resultado);
        if (string.IsNullOrWhiteSpace(resultado.Cufe))
            throw new InvalidOperationException("La DIAN no devolvio CUFE en respuesta exitosa.");
        return (MaquinaEstadosDocumentoElectronico.MarcarValidado(enviado), resultado);
    }
}
