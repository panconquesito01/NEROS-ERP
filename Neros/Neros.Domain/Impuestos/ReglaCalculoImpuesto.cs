namespace Neros.Domain.Impuestos;

/// <summary>Regla congelada dentro de una version publicada inmutable (plan §38).</summary>
public sealed record ReglaCalculoImpuesto(
    string CodigoImpuesto,
    decimal Tarifa,
    int Orden,
    bool PrecioIncluyeImpuesto,
    bool EsRetencion,
    bool BaseAcumulaImpuestosPrevios)
{
    public ReglaCalculoImpuesto Validada()
    {
        if (string.IsNullOrWhiteSpace(CodigoImpuesto)) throw new ArgumentException("Codigo de impuesto requerido.");
        if (Tarifa < 0) throw new ArgumentOutOfRangeException(nameof(Tarifa));
        if (Orden < 0) throw new ArgumentOutOfRangeException(nameof(Orden));
        if (PrecioIncluyeImpuesto && EsRetencion)
            throw new ArgumentException("Una retencion no puede declararse como precio incluido.");
        return this;
    }
}

public sealed record VersionImpuestosInmutable(Guid Id, int NumeroVersion, IReadOnlyList<ReglaCalculoImpuesto> Reglas)
{
    public VersionImpuestosInmutable Validada()
    {
        if (Id == Guid.Empty) throw new ArgumentException("Version invalida.");
        if (NumeroVersion <= 0) throw new ArgumentOutOfRangeException(nameof(NumeroVersion));
        if (Reglas.Count == 0) throw new ArgumentException("La version requiere al menos una regla.");
        var ordenes = Reglas.Select(r => r.Validada().Orden).ToList();
        if (ordenes.Distinct().Count() != ordenes.Count)
            throw new ArgumentException("Orden duplicado en reglas de la version.");
        return this;
    }

    public IReadOnlyList<ReglaCalculoImpuesto> ReglasOrdenadas => Reglas.OrderBy(r => r.Orden).ToList();
}
