namespace Neros.Domain.Facturacion;

/// <summary>Reserva de consecutivo fiscal sin huecos (plan §52).</summary>
public static class MotorNumeracionFiscal
{
    public static ResultadoReservaNumero ReservarSiguiente(RangoNumeracionFiscal rango, DateOnly fechaEmision)
    {
        ArgumentNullException.ThrowIfNull(rango);
        if (string.IsNullOrWhiteSpace(rango.Prefijo))
            throw new ArgumentException("Prefijo requerido.", nameof(rango));
        if (rango.Estado != EstadoNumeracionFiscal.Activo)
            throw new InvalidOperationException("La numeracion no esta activa.");
        if (fechaEmision < rango.VigenciaDesde || fechaEmision > rango.VigenciaHasta)
            throw new InvalidOperationException("La fecha de emision esta fuera de la vigencia de la resolucion.");
        var siguiente = rango.Actual + 1;
        if (siguiente < rango.Desde || siguiente > rango.Hasta)
            throw new InvalidOperationException("El rango de numeracion esta agotado.");
        var presentacion = FormatearNumero(rango.Prefijo, siguiente);
        var estado = siguiente >= rango.Hasta ? EstadoNumeracionFiscal.Agotado : rango.Estado;
        return new ResultadoReservaNumero(siguiente, presentacion, estado);
    }

    public static bool DebeAlertarAgotamiento(RangoNumeracionFiscal rango, int umbralRestantes = 100)
    {
        if (rango.Estado != EstadoNumeracionFiscal.Activo) return false;
        var restantes = rango.Hasta - rango.Actual;
        return restantes >= 0 && restantes <= umbralRestantes;
    }

    public static bool DebeAlertarVencimiento(RangoNumeracionFiscal rango, DateOnly hoy, int diasAnticipacion = 30) =>
        rango.Estado == EstadoNumeracionFiscal.Activo && rango.VigenciaHasta <= hoy.AddDays(diasAnticipacion);

    public static string FormatearNumero(string prefijo, long numero) => $"{prefijo.Trim()}{numero}";
}
