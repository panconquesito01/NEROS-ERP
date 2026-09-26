namespace Neros.Domain.Globalizacion;

/// <summary>Politica de redondeo por moneda y tipo documental (plan §32).</summary>
public sealed record PoliticaRedondeo(
    string Moneda,
    int PrecisionCalculo,
    int PrecisionMoneda,
    ModoRedondeo Modo,
    MomentoRedondeo Momento = MomentoRedondeo.PorLinea)
{
    public PoliticaRedondeo Validada()
    {
        CatalogoIso.ValidarMoneda(Moneda);
        if (PrecisionCalculo < PrecisionMoneda)
            throw new ArgumentOutOfRangeException(nameof(PrecisionCalculo), "La precision de calculo no puede ser menor que la de moneda.");
        if (PrecisionMoneda < 0 || PrecisionCalculo < 0)
            throw new ArgumentOutOfRangeException(nameof(PrecisionMoneda));
        return this;
    }
}
