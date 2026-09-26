namespace Neros.Domain.Busqueda;

public enum ResultadoIndexacion
{
    Indexado,
    DuplicadoIgnorado,
    VersionAntiguaIgnorada,
    TombstoneAplicado
}

public sealed record CheckpointIngesta(
    Guid TenantId,
    Guid EmpresaId,
    string FuenteModulo,
    DateTime UltimoInstanteUtc,
    Guid? UltimoEventoId);

public sealed record EventoParaIndexacion(
    Guid MessageId,
    Guid TenantId,
    Guid EmpresaId,
    string FuenteModulo,
    string TipoEvento,
    string EntidadId,
    long VersionAgregado,
    DateTimeOffset OcurrioEnUtc,
    Guid CorrelationId,
    bool EsTombstone,
    string PayloadJson);

public sealed record DocumentoIndice(
    Guid Id,
    Guid TenantId,
    Guid EmpresaId,
    string TipoEntidad,
    string EntidadId,
    long VersionIndice,
    string Titulo,
    string? Resumen,
    string TextoBusqueda,
    string PermisoRequerido,
    string OrigenModulo,
    Guid CorrelationId,
    bool Activo,
    DateTimeOffset? TombstoneEnUtc,
    DateTimeOffset IndexadoEnUtc);

public sealed record DocumentoIndiceVisible(
    Guid Id,
    string TipoEntidad,
    string EntidadId,
    string Titulo,
    string? Resumen,
    DateTimeOffset IndexadoEnUtc);

public sealed record CriterioBusqueda(
    Guid TenantId,
    Guid EmpresaId,
    string Termino,
    int MaxResultados,
    string? CursorTitulo,
    Guid? CursorId);

public sealed record FrescuraIndice(DateTimeOffset? UltimoIndexadoUtc, string? ModuloMasReciente);

public sealed record ResultadoBusqueda(
    IReadOnlyList<DocumentoIndiceVisible> Documentos,
    string? SiguienteCursorTitulo,
    Guid? SiguienteCursorId,
    FrescuraIndice Frescura);

public sealed record EnlaceVista360(
    Guid Id,
    Guid DocumentoOrigenId,
    Guid DocumentoRelacionadoId,
    string TipoRelacion);
