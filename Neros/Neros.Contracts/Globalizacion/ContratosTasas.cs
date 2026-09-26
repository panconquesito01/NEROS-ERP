namespace Neros.Contracts.Globalizacion;

public sealed record TasaCambioDia(
    string MonedaOrigen, string MonedaDestino, DateOnly Fecha, decimal Valor, string Fuente);

public sealed record PaginaTasasCambio(IReadOnlyList<TasaCambioDia> Tasas, string MonedaDestino, DateOnly Fecha);

public sealed class SolicitudSincronizarTasas
{
    public string MonedaDestino { get; set; } = "COP";
    public string? Pais { get; set; }
    public DateOnly? Fecha { get; set; }
}

public sealed record ResultadoSincronizacionTasas(int Actualizadas, DateOnly Fecha, string Fuente);
