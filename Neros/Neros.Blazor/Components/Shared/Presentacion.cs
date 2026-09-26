namespace Neros.Blazor.Components.Shared;

public static class Presentacion
{
    public static string Iniciales(string? texto)
    {
        var palabras = (texto ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(palabra => new string(palabra.Where(char.IsLetterOrDigit).ToArray()))
            .Where(palabra => palabra.Length > 0)
            .ToArray();
        return palabras.Length switch
        {
            0 => "N",
            1 => palabras[0][..Math.Min(2, palabras[0].Length)].ToUpperInvariant(),
            _ => string.Concat(char.ToUpperInvariant(palabras[0][0]), char.ToUpperInvariant(palabras[1][0]))
        };
    }

    public static string Monograma(string codigo) => codigo[..Math.Min(2, codigo.Length)];

    public static string PrimerNombre(string? nombre) =>
        (nombre ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? string.Empty;

    /// <summary>Devuelve la clave de TextosComunes de la seccion actual, o null si la ruta no tiene seccion.</summary>
    public static string? ClaveSeccion(string rutaRelativa) =>
        rutaRelativa.Split('?', '#')[0].Trim('/').ToLowerInvariant() switch
        {
            "home" => "Seccion.Inicio",
            "empresas" => "Seccion.Empresas",
            _ => null
        };
}
