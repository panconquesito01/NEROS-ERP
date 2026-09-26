namespace Neros.Contracts.Busqueda;

public static class SearchAudiencias
{
    public const string Api = "neros.search";
}

public static class PermisosBusqueda
{
    public const string Consultar = "Search.Document.Read";
}

public sealed record DocumentoBusquedaDto(
    Guid Id,
    string TipoEntidad,
    string EntidadId,
    string Titulo,
    string? Resumen,
    DateTimeOffset IndexadoEnUtc);

public sealed record PaginaBusquedaDto(
    IReadOnlyList<DocumentoBusquedaDto> Elementos,
    string? CursorTitulo,
    Guid? CursorId,
    DateTimeOffset? UltimoIndexadoUtc,
    string? ModuloMasReciente);
