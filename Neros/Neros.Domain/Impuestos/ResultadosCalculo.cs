namespace Neros.Domain.Impuestos;

public sealed record ResultadoImpuesto(string CodigoImpuesto, decimal BaseGravable, decimal Tarifa, decimal Importe, bool EsRetencion);

public sealed record LineaEntrada(decimal Cantidad, decimal PrecioUnitario, decimal DescuentoLinea);

public sealed record LineaCalculada(
    decimal Bruto, decimal Descuentos, decimal BaseNeta,
    IReadOnlyList<ResultadoImpuesto> Impuestos, IReadOnlyList<ResultadoImpuesto> Retenciones)
{
    public decimal TotalImpuestos => Impuestos.Sum(i => i.Importe);
    public decimal TotalRetenciones => Retenciones.Sum(r => r.Importe);
}

public sealed record DocumentoCalculado(
    IReadOnlyList<LineaCalculada> Lineas,
    decimal Subtotal,
    decimal TotalImpuestos,
    decimal TotalRetenciones,
    decimal Total,
    decimal ValorAPagar)
{
    public static DocumentoCalculado Vacio() => new([], 0, 0, 0, 0, 0);
}
