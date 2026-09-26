namespace Neros.Domain.Globalizacion;

/// <summary>Conversion usando tasa origen→destino (plan §31).</summary>
public static class ConversionMoneda
{
    public static Dinero Convertir(Dinero origen, string monedaDestino, decimal tasaOrigenADestino, PoliticaRedondeo politicaDestino)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tasaOrigenADestino);
        origen.Validado();
        var destino = monedaDestino.Trim().ToUpperInvariant();
        if (!destino.Equals(politicaDestino.Moneda, StringComparison.Ordinal))
            throw new ArgumentException("La politica debe corresponder a la moneda destino.", nameof(politicaDestino));
        politicaDestino.Validada();
        var convertido = origen.Cantidad * tasaOrigenADestino;
        return new Dinero(MotorRedondeo.Aplicar(convertido, politicaDestino), destino);
    }

    public static Dinero AMonedaFuncional(Dinero documento, decimal tasaDocumentoAFuncional, PoliticaRedondeo politicaFuncional) =>
        Convertir(documento, politicaFuncional.Moneda, tasaDocumentoAFuncional, politicaFuncional);
}
