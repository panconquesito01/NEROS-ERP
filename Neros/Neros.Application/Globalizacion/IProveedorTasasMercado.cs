namespace Neros.Application.Globalizacion;

public interface IProveedorTasasMercado
{
    string NombreFuente { get; }
    Task<IReadOnlyDictionary<string, decimal>> ObtenerTasasHaciaAsync(
        string monedaDestino, DateOnly fecha, IReadOnlyList<string> monedasOrigen, string? pais, CancellationToken cancellationToken);
}
