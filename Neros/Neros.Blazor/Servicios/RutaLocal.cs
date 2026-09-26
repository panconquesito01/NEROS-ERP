namespace Neros.Blazor.Servicios;

public static class RutaLocal
{
    public static string? Validar(string? ruta) =>
        !string.IsNullOrEmpty(ruta) && ruta.StartsWith('/') && !ruta.StartsWith("//") && !ruta.StartsWith("/\\")
            ? ruta
            : null;
}
