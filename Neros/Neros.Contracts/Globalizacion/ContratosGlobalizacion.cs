namespace Neros.Contracts.Globalizacion;

public sealed record CatalogoPais(string Codigo, string Nombre);
public sealed record CatalogoMoneda(string Codigo, string Nombre, byte DecimalesIso4217);
public sealed record CatalogoZonaHoraria(string Id, string Nombre);
public sealed record CatalogoUnidadMedida(string Codigo, string Nombre, string? Simbolo);
public sealed record CatalogoTipoIdentificacion(string Pais, string Codigo, string Nombre);

public sealed record CatalogosTransversales(
    IReadOnlyList<CatalogoPais> Paises,
    IReadOnlyList<CatalogoMoneda> Monedas,
    IReadOnlyList<CatalogoZonaHoraria> ZonasHorarias,
    IReadOnlyList<CatalogoUnidadMedida> UnidadesMedida,
    IReadOnlyList<CatalogoTipoIdentificacion> TiposIdentificacion);
