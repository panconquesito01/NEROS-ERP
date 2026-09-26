namespace Neros.Domain.Tesoreria;

public static class GestorTransferenciaTesoreria
{
    public static ResultadoTransferenciaTesoreria Registrar(TransferenciaTesoreriaEntrada transferencia)
    {
        ArgumentNullException.ThrowIfNull(transferencia);
        if (transferencia.Importe <= 0) throw new ArgumentOutOfRangeException(nameof(transferencia));
        if (transferencia.CuentaOrigenId == transferencia.CuentaDestinoId)
            throw new InvalidOperationException("La transferencia requiere cuentas distintas.");
        var salida = new MovimientoTesoreriaEntrada(
            Guid.NewGuid(),
            transferencia.CuentaOrigenId,
            TipoMovimientoTesoreria.TransferenciaSalida,
            transferencia.Importe,
            transferencia.Fecha);
        var entrada = new MovimientoTesoreriaEntrada(
            Guid.NewGuid(),
            transferencia.CuentaDestinoId,
            TipoMovimientoTesoreria.TransferenciaEntrada,
            transferencia.Importe,
            transferencia.Fecha);
        return new ResultadoTransferenciaTesoreria(transferencia, salida, entrada);
    }
}
