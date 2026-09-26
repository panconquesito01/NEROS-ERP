namespace Neros.Domain.Globalizacion;

/// <summary>Unico punto de redondeo monetario; prohibido Math.Round directo en importes (plan §32).</summary>
public static class MotorRedondeo
{
    public static decimal Aplicar(decimal valor, PoliticaRedondeo politica)
    {
        var intermedio = Redondear(valor, politica.PrecisionCalculo, politica.Modo);
        return Redondear(intermedio, politica.PrecisionMoneda, politica.Modo);
    }

    public static decimal Redondear(decimal valor, int decimales, ModoRedondeo modo)
    {
        if (decimales < 0) throw new ArgumentOutOfRangeException(nameof(decimales));
        return modo switch
        {
            ModoRedondeo.AwayFromZero => decimal.Round(valor, decimales, MidpointRounding.AwayFromZero),
            ModoRedondeo.ToEven => decimal.Round(valor, decimales, MidpointRounding.ToEven),
            ModoRedondeo.Truncate => Truncar(valor, decimales),
            _ => throw new ArgumentOutOfRangeException(nameof(modo))
        };
    }

    private static decimal Truncar(decimal valor, int decimales)
    {
        var factor = (decimal)Math.Pow(10, decimales);
        return valor >= 0 ? Math.Truncate(valor * factor) / factor : -Math.Truncate(-valor * factor) / factor;
    }
}
