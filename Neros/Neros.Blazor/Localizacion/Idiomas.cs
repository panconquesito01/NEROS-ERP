using System.Globalization;

namespace Neros.Blazor.Localizacion;

public sealed record Idioma(string Codigo, string NombreNativo);

public static class Idiomas
{
    public const string Predeterminado = "es";

    public static IReadOnlyList<Idioma> Disponibles { get; } =
    [
        new("es", "Español"),
        new("en", "English"),
        new("pt", "Português")
    ];

    public static string[] Codigos { get; } = Disponibles.Select(idioma => idioma.Codigo).ToArray();

    public static bool EsCompatible(string? codigo) =>
        Disponibles.Any(idioma => string.Equals(idioma.Codigo, codigo, StringComparison.OrdinalIgnoreCase));

    public static Idioma Actual =>
        Disponibles.FirstOrDefault(idioma => idioma.Codigo == CultureInfo.CurrentUICulture.TwoLetterISOLanguageName) ?? Disponibles[0];
}
