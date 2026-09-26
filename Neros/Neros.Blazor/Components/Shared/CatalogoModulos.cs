namespace Neros.Blazor.Components.Shared;

/// <summary>Los textos de cada modulo viven en TextosComunes bajo Modulo.{Clave}.Nombre, .Lema y .Alcance.</summary>
public sealed record ModuloErp(string Clave, string Icono, bool Disponible)
{
    public string ClaveNombre => $"Modulo.{Clave}.Nombre";
    public string ClaveLema => $"Modulo.{Clave}.Lema";
    public string ClaveAlcance => $"Modulo.{Clave}.Alcance";
}

public static class CatalogoModulos
{
    public static IReadOnlyList<ModuloErp> Todos { get; } =
    [
        new("Multiempresa", "building-2", true),
        new("Ventas", "receipt-text", false),
        new("Inventarios", "package", false),
        new("Compras", "arrow-left-right", false),
        new("Finanzas", "wallet", false),
        new("Informes", "chart-no-axes-combined", false)
    ];

    public static IEnumerable<ModuloErp> EnPreparacion => Todos.Where(modulo => !modulo.Disponible);
}
