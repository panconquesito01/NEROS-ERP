namespace Neros.Domain.Globalizacion;

/// <summary>Importe con moneda ISO 4217 (plan §9).</summary>
public readonly record struct Dinero(decimal Cantidad, string Moneda)
{
    public Dinero Validado()
    {
        CatalogoIso.ValidarMoneda(Moneda);
        return this;
    }

    public Dinero Redondear(PoliticaRedondeo politica, bool esTotal = false)
    {
        if (!Moneda.Equals(politica.Moneda, StringComparison.Ordinal))
            throw new InvalidOperationException("La politica de redondeo no corresponde a la moneda del importe.");
        if (politica.Momento == MomentoRedondeo.PorTotal && !esTotal)
            return this;
        return new Dinero(MotorRedondeo.Aplicar(Cantidad, politica), Moneda);
    }

    public static Dinero operator +(Dinero izquierda, Dinero derecha)
    {
        if (!izquierda.Moneda.Equals(derecha.Moneda, StringComparison.Ordinal))
            throw new InvalidOperationException("No se pueden sumar importes en monedas distintas sin conversion.");
        return new Dinero(izquierda.Cantidad + derecha.Cantidad, izquierda.Moneda);
    }
}
