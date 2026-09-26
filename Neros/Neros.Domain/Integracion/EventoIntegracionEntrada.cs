namespace Neros.Domain.Integracion;

/// <summary>Evento ya validado en el bus; el payload tipado vive en <see cref="Carga"/>.</summary>
public sealed record EventoIntegracionEntrada(
    Guid MessageId,
    string TipoEvento,
    string AggregateId,
    long VersionAgregado,
    Guid TenantId,
    Guid? EmpresaId,
    Guid CorrelationId,
    string PayloadJson,
    object Carga);
