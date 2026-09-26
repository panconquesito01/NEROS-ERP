using System.Text.Json;

namespace Neros.Messaging.Abstractions;

public static class IntegrationEnvelopeSerde
{
    private static readonly JsonSerializerOptions Opciones = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static byte[] Serializar(IntegrationEnvelope mensaje)
        => JsonSerializer.SerializeToUtf8Bytes(mensaje, Opciones);

    public static IntegrationEnvelope Deserializar(byte[] utf8)
    {
        using var doc = JsonDocument.Parse(utf8);
        var root = doc.RootElement;
        return new IntegrationEnvelope(
            root.GetProperty("messageId").GetGuid(),
            root.GetProperty("type").GetString()!,
            root.GetProperty("producer").GetString()!,
            root.GetProperty("tenantId").GetGuid(),
            root.TryGetProperty("companyId", out var company) && company.ValueKind != JsonValueKind.Null
                ? company.GetGuid() : null,
            root.GetProperty("aggregateId").GetString()!,
            root.GetProperty("aggregateVersion").GetInt64(),
            root.GetProperty("occurredAt").GetDateTimeOffset(),
            root.TryGetProperty("actorId", out var actor) && actor.ValueKind == JsonValueKind.String
                ? actor.GetString() : null,
            root.GetProperty("correlationId").GetGuid(),
            root.TryGetProperty("causationId", out var causa) && causa.ValueKind != JsonValueKind.Null
                ? causa.GetGuid() : null,
            root.TryGetProperty("traceParent", out var trace) && trace.ValueKind == JsonValueKind.String
                ? trace.GetString() : null,
            root.GetProperty("data").Clone());
    }
}
