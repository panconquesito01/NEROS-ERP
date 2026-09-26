using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Neros.Application.Globalizacion;

namespace Neros.Persistence.Globalizacion;

/// <summary>Tasas de referencia diarias vía Frankfurter (ECB), sin clave API.</summary>
public sealed class ProveedorTasasFrankfurter(IHttpClientFactory fabrica) : IProveedorTasasMercado
{
    public string NombreFuente => "Frankfurter.app";

    public async Task<IReadOnlyDictionary<string, decimal>> ObtenerTasasHaciaAsync(
        string monedaDestino, DateOnly fecha, IReadOnlyList<string> monedasOrigen, string? pais, CancellationToken cancellationToken)
    {
        _ = pais;
        var destino = monedaDestino.Trim().ToUpperInvariant();
        var origenes = monedasOrigen.Select(m => m.Trim().ToUpperInvariant()).Where(m => m != destino).Distinct().ToList();
        if (origenes.Count == 0) return new Dictionary<string, decimal>();

        var cliente = fabrica.CreateClient("tasas-frankfurter");
        var fechaTexto = fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var url = $"https://api.frankfurter.app/{fechaTexto}?from={destino}&to={string.Join(',', origenes)}";
        using var respuesta = await cliente.GetAsync(url, cancellationToken);
        if (!respuesta.IsSuccessStatusCode) return new Dictionary<string, decimal>();
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<RespuestaFrankfurter>(cancellationToken);
        if (cuerpo?.Rates is not { Count: > 0 }) return new Dictionary<string, decimal>();
        var invertidas = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var par in cuerpo.Rates)
        {
            if (par.Value <= 0) continue;
            invertidas[par.Key] = Math.Round(1m / par.Value, 12, MidpointRounding.AwayFromZero);
        }
        return invertidas;
    }

    private sealed class RespuestaFrankfurter
    {
        [JsonPropertyName("rates")]
        public Dictionary<string, decimal> Rates { get; init; } = [];
    }
}
