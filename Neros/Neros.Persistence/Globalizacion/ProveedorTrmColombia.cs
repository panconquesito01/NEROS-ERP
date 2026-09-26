using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Neros.Application.Globalizacion;

namespace Neros.Persistence.Globalizacion;

/// <summary>TRM oficial USD→COP desde datos abiertos (cuando pais=CO).</summary>
public sealed class ProveedorTrmColombia(IHttpClientFactory fabrica) : IProveedorTasasMercado
{
    public string NombreFuente => "DatosAbiertosColombia.TRM";

    public async Task<IReadOnlyDictionary<string, decimal>> ObtenerTasasHaciaAsync(
        string monedaDestino, DateOnly fecha, IReadOnlyList<string> monedasOrigen, string? pais, CancellationToken cancellationToken)
    {
        if (!string.Equals(pais, "CO", StringComparison.OrdinalIgnoreCase)
            || !monedaDestino.Equals("COP", StringComparison.OrdinalIgnoreCase)
            || !monedasOrigen.Any(m => m.Equals("USD", StringComparison.OrdinalIgnoreCase)))
            return new Dictionary<string, decimal>();

        var cliente = fabrica.CreateClient("tasas-trm-co");
        var fechaTexto = fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var url = $"https://www.datos.gov.co/resource/32sa-8pi3.json?vigenciahasta={fechaTexto}&$limit=1&$order=vigenciahasta DESC";
        using var respuesta = await cliente.GetAsync(url, cancellationToken);
        if (!respuesta.IsSuccessStatusCode) return new Dictionary<string, decimal>();
        var filas = await respuesta.Content.ReadFromJsonAsync<List<FilaTrm>>(cancellationToken);
        var valor = filas?.FirstOrDefault()?.Valor;
        if (valor is null or <= 0) return new Dictionary<string, decimal>();
        return new Dictionary<string, decimal>(StringComparer.Ordinal) { ["USD"] = valor.Value };
    }

    private sealed class FilaTrm
    {
        [JsonPropertyName("valor")]
        public decimal? Valor { get; init; }
    }
}
