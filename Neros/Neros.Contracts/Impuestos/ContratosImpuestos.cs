namespace Neros.Contracts.Impuestos;

public sealed record PublicacionVersionImpuestos(Guid VersionId, int Numero, string HashContenido);

public sealed record ReglaImpuestoPublicada(
    string CodigoImpuesto, decimal Tarifa, int Orden, bool PrecioIncluyeImpuesto, bool EsRetencion, bool BaseAcumulaImpuestosPrevios);
