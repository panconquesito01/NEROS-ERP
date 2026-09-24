using System.Text.Json;
using Neros.Messaging.Abstractions;
using Xunit;

namespace Neros.Tests;

public sealed class FoundationMessagingTests
{
    [Fact]
    public void Envelope_ConservaIdentidadYDatosTrasLiberarDocumentoYSerializar()
    {
        IntegrationEnvelope message;
        using (var document = JsonDocument.Parse("{\"companyId\":\"sample\"}"))
            message = Create(document.RootElement);
        var serialized = JsonSerializer.Serialize(message);
        var restored = JsonSerializer.Deserialize<IntegrationEnvelope>(serialized)!;
        Assert.Equal(message.MessageId, restored.MessageId);
        Assert.Equal(message.CorrelationId, restored.CorrelationId);
        Assert.Equal(message.TenantId, restored.TenantId);
        Assert.Equal("sample", restored.Data.GetProperty("companyId").GetString());
    }

    [Fact]
    public void Envelope_RechazaContratoSinVersionPayloadGrandeYTenantVacio()
    {
        using var data = JsonDocument.Parse("{}");
        Assert.Throws<ArgumentException>(() => Create(data.RootElement, type: "CompanyChanged"));
        Assert.Throws<ArgumentException>(() => Create(data.RootElement, tenant: Guid.Empty));
        using var large = JsonDocument.Parse(JsonSerializer.Serialize(new { text = new string('a', 65536) }));
        Assert.Throws<ArgumentException>(() => Create(large.RootElement));
    }

    [Fact]
    public void Huella_DtoNormalizadoNoDependeDeEspaciosUOrdenDeEntrada()
    {
        var first = JsonSerializer.Deserialize<Request>("{\"Code\":\"ACME\",\"Name\":\"Empresa\"}")!;
        var second = JsonSerializer.Deserialize<Request>("{ \"Name\": \"Empresa\", \"Code\": \"ACME\" }")!;
        Assert.Equal(RequestFingerprint.Create(first), RequestFingerprint.Create(second));
        Assert.NotEqual(RequestFingerprint.Create(first), RequestFingerprint.Create(first with { Name = "Otra" }));
        Assert.Equal(RequestFingerprint.Create(new Dictionary<string, decimal> { ["a"] = 1.00m, ["b"] = 2m }),
            RequestFingerprint.Create(new Dictionary<string, decimal> { ["b"] = 2.0m, ["a"] = 1m }));
    }

    [Theory]
    [InlineData("{\"amount\":1.00000000000000000000000000001}")]
    [InlineData("{\"amount\":1e-100}")]
    [InlineData("{\"amount\":1e100}")]
    public void Huella_RechazaNumerosQuePierdenPrecision(string payload)
    {
        using var document = JsonDocument.Parse(payload);
        Assert.Throws<ArgumentException>(() => RequestFingerprint.Create(document.RootElement));
    }

    private sealed record Request(string Code, string Name);

    private static IntegrationEnvelope Create(JsonElement data, string type = "CompanyChanged.v1", Guid? tenant = null) =>
        new(Guid.NewGuid(), type, "Organization", tenant ?? Guid.NewGuid(), null, "company-id", 1,
            DateTimeOffset.UtcNow, "test-subject", Guid.NewGuid(), null, null, data);
}