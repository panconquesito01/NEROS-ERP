using System.Security.Claims;
using Neros.Blazor.Servicios;

namespace Neros.Blazor.Components.Shared;

/// <summary>Los textos de cada modulo viven en TextosComunes bajo Modulo.{Clave}.Nombre, .Lema y .Alcance.</summary>
public sealed record ModuloErp(string Clave, string Icono, bool Disponible, string? Ruta = null)
{
    public string ClaveNombre => $"Modulo.{Clave}.Nombre";
    public string ClaveLema => $"Modulo.{Clave}.Lema";
    public string ClaveAlcance => $"Modulo.{Clave}.Alcance";
}

public static class CatalogoModulos
{
    public static IReadOnlyList<ModuloErp> Todos { get; } =
    [
        new("Multiempresa", "building-2", true, "/empresas"),
        new("Ventas", "receipt-text", true, "/ventas"),
        new("Inventarios", "package", true, "/inventarios"),
        new("Compras", "arrow-left-right", true, "/compras"),
        new("Finanzas", "wallet", true, "/finanzas"),
        new("Informes", "chart-no-axes-combined", true, "/informes")
    ];

    public static IEnumerable<ModuloErp> EnPreparacion => Todos.Where(modulo => !modulo.Disponible);

    public static IEnumerable<ModuloErp> ParaUsuario(ClaimsPrincipal usuario) =>
        Todos.Where(m => !m.Disponible || UsuarioTieneModulo(usuario, m.Clave));

    public static bool UsuarioTieneModulo(ClaimsPrincipal usuario, string claveModulo)
    {
        if (claveModulo.Equals("Multiempresa", StringComparison.OrdinalIgnoreCase))
            return true;
        var modulos = usuario.FindAll(EndpointsSesion.ModuloClaim).Select(c => c.Value).ToList();
        if (modulos.Count == 0)
            return Todos.Any(m => m.Clave.Equals(claveModulo, StringComparison.OrdinalIgnoreCase) && m.Disponible);
        return modulos.Contains(claveModulo, StringComparer.OrdinalIgnoreCase);
    }
}
