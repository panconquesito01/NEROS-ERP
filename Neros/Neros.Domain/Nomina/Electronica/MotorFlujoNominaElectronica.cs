namespace Neros.Domain.Nomina.Electronica;

public static class MotorFlujoNominaElectronica
{
    public static void ValidarPrecondicion(EstadoLiquidacionNominaContabilizable estadoLiquidacion)
    {
        if (!MaquinaEstadosNominaElectronica.RequiereLiquidacionContabilizada(estadoLiquidacion))
            throw new InvalidOperationException("La nomina electronica exige una liquidacion contabilizada.");
    }

    public static (EstadoNominaElectronica NuevoEstado, string HashXml) Generar(EstadoNominaElectronica actual, string contenidoXml)
    {
        if (string.IsNullOrWhiteSpace(contenidoXml))
            throw new ArgumentException("Contenido XML requerido.", nameof(contenidoXml));
        var nuevo = MaquinaEstadosNominaElectronica.GenerarXml(actual);
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(contenidoXml)));
        return (nuevo, hash);
    }

    public static EstadoNominaElectronica Firmar(EstadoNominaElectronica actual) =>
        MaquinaEstadosNominaElectronica.Firmar(actual);

    public static async Task<(EstadoNominaElectronica NuevoEstado, ResultadoTransmisionNominaElectronica Resultado)> EnviarAsync(
        EstadoNominaElectronica actual,
        AmbienteDianNomina ambiente,
        string hashXml,
        string idempotencyKey,
        IEnvioNominaElectronicaDian envio,
        CancellationToken cancellationToken = default)
    {
        var enviado = MaquinaEstadosNominaElectronica.Enviar(actual);
        var resultado = await envio.EnviarAsync(ambiente, hashXml, idempotencyKey, cancellationToken).ConfigureAwait(false);
        if (!resultado.Exito)
            return (MaquinaEstadosNominaElectronica.MarcarRechazado(enviado), resultado);
        if (string.IsNullOrWhiteSpace(resultado.Cune))
            throw new InvalidOperationException("La DIAN no devolvio CUNE en respuesta exitosa.");
        return (MaquinaEstadosNominaElectronica.MarcarValidado(enviado), resultado);
    }
}
