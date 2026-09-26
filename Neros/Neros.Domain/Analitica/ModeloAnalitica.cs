namespace Neros.Domain.Analitica;

public enum ResultadoIngesta
{
    Ingerido,
    DuplicadoIgnorado,
    RechazadoTardio
}

public sealed record MarcaAguaIngesta(
    Guid TenantId,
    Guid EmpresaId,
    string FuenteModulo,
    DateTime UltimoInstanteUtc,
    Guid? UltimoEventoId);

public sealed record EventoParaIngesta(
    Guid MessageId,
    Guid TenantId,
    Guid EmpresaId,
    string FuenteModulo,
    string TipoEvento,
    string AggregateId,
    long VersionAgregado,
    DateTimeOffset OcurrioEnUtc,
    Guid CorrelationId,
    Guid? CausationId,
    string PayloadJson);

public sealed record EventoIngestaRegistrado(
    Guid Id,
    EventoParaIngesta Evento,
    bool EsTardio);

public sealed record HechoOperativo(
    Guid Id,
    Guid TenantId,
    Guid EmpresaId,
    string TipoHecho,
    DateOnly PeriodoNegocio,
    decimal ImporteMonedaFuncional,
    decimal? Cantidad,
    string? DimensionClave,
    Guid EventoIngestaId);

public sealed record DefinicionIndicador(
    Guid Id,
    string Codigo,
    string TipoIndicador,
    bool Activo);

public sealed record ValorIndicadorCalculado(
    string CodigoIndicador,
    DateOnly PeriodoNegocio,
    decimal Valor);
