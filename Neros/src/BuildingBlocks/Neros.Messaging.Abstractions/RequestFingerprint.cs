using System.Security.Cryptography;
using System.Text.Json;
using System.Globalization;

namespace Neros.Messaging.Abstractions;

public static class RequestFingerprint
{
    public static string Create<T>(T normalizedRequest) where T : notnull
    {
        var element = JsonSerializer.SerializeToElement(normalizedRequest);
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer)) WriteCanonical(writer, element);
        return Convert.ToHexString(SHA256.HashData(buffer.ToArray()));
    }

    private static void WriteCanonical(Utf8JsonWriter writer, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                var names = new HashSet<string>(StringComparer.Ordinal);
                foreach (var property in element.EnumerateObject().OrderBy(property => property.Name, StringComparer.Ordinal))
                {
                    if (!names.Add(property.Name)) throw new ArgumentException("Propiedad duplicada en la solicitud.");
                    writer.WritePropertyName(property.Name);
                    WriteCanonical(writer, property.Value);
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray()) WriteCanonical(writer, item);
                writer.WriteEndArray();
                break;
            case JsonValueKind.Number:
                if (!element.TryGetDecimal(out var number)
                    || !JsonElement.DeepEquals(element, JsonSerializer.SerializeToElement(number)))
                    throw new ArgumentException("Numero fuera de precision admitida.");
                writer.WriteRawValue(number.ToString("G29", CultureInfo.InvariantCulture));
                break;
            default:
                element.WriteTo(writer);
                break;
        }
    }
}