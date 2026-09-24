using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Neros.Messaging.Abstractions;

public sealed record IntegrationEnvelope
{
    public Guid MessageId { get; }
    public string Type { get; }
    public string Producer { get; }
    public Guid TenantId { get; }
    public Guid? CompanyId { get; }
    public string AggregateId { get; }
    public long AggregateVersion { get; }
    public DateTimeOffset OccurredAt { get; }
    public string? ActorId { get; }
    public Guid CorrelationId { get; }
    public Guid? CausationId { get; }
    public string? TraceParent { get; }
    public JsonElement Data { get; }

    public IntegrationEnvelope(Guid messageId, string type, string producer, Guid tenantId, Guid? companyId,
        string aggregateId, long aggregateVersion, DateTimeOffset occurredAt, string? actorId,
        Guid correlationId, Guid? causationId, string? traceParent, JsonElement data)
    {
        if (messageId == Guid.Empty || tenantId == Guid.Empty || correlationId == Guid.Empty
            || companyId == Guid.Empty || causationId == Guid.Empty)
            throw new ArgumentException("Los identificadores del mensaje no pueden estar vacios.");
        if (type is null || !Regex.IsMatch(type, "^[A-Z][A-Za-z0-9]{0,100}\\.v[1-9][0-9]{0,2}$", RegexOptions.CultureInvariant))
            throw new ArgumentException("El contrato debe indicar un tipo y version explicita.", nameof(type));
        if (string.IsNullOrWhiteSpace(producer) || producer.Length > 100
            || string.IsNullOrWhiteSpace(aggregateId) || aggregateId.Length > 200 || aggregateVersion <= 0)
            throw new ArgumentException("Productor, agregado y version son obligatorios.");
        if (occurredAt == default || occurredAt.Offset != TimeSpan.Zero)
            throw new ArgumentException("El evento requiere fecha UTC.", nameof(occurredAt));
        if (actorId is not null && (string.IsNullOrWhiteSpace(actorId) || actorId.Length > 450))
            throw new ArgumentException("Actor invalido.", nameof(actorId));
        if (traceParent is not null && !ActivityContext.TryParse(traceParent, null, out _))
            throw new ArgumentException("Contexto W3C invalido.", nameof(traceParent));
        if (data.ValueKind != JsonValueKind.Object || System.Text.Encoding.UTF8.GetByteCount(data.GetRawText()) > 65536)
            throw new ArgumentException("El payload debe ser un objeto JSON de hasta 64 KiB.", nameof(data));
        MessageId = messageId;
        Type = type;
        Producer = producer;
        TenantId = tenantId;
        CompanyId = companyId;
        AggregateId = aggregateId;
        AggregateVersion = aggregateVersion;
        OccurredAt = occurredAt;
        ActorId = actorId;
        CorrelationId = correlationId;
        CausationId = causationId;
        TraceParent = traceParent;
        Data = data.Clone();
    }
}