using System.Text.Json;

namespace Neros.Application.Seguridad;

public static class ModulosEmpresa
{
    public static readonly IReadOnlyList<string> OperativosPorDefecto =
    [
        "Ventas", "Inventarios", "Compras", "Finanzas", "Informes"
    ];

    public static IReadOnlyList<string> Resolver(string? jsonAlmacenado, bool administradorGlobal)
    {
        if (administradorGlobal)
            return OperativosPorDefecto.Concat(["Multiempresa"]).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var parsed = Deserializar(jsonAlmacenado);
        if (parsed.Count == 0)
            return OperativosPorDefecto;
        return parsed;
    }

    public static IReadOnlyList<string> Deserializar(string? jsonAlmacenado)
    {
        if (string.IsNullOrWhiteSpace(jsonAlmacenado))
            return [];
        try
        {
            return JsonSerializer.Deserialize<List<string>>(jsonAlmacenado)?
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Select(m => m.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public static bool ModuloPermitido(IReadOnlyList<string> habilitados, string claveModulo)
    {
        if (claveModulo.Equals("Multiempresa", StringComparison.OrdinalIgnoreCase))
            return true;
        if (habilitados.Count == 0)
            return OperativosPorDefecto.Contains(claveModulo, StringComparer.OrdinalIgnoreCase);
        return habilitados.Contains(claveModulo, StringComparer.OrdinalIgnoreCase);
    }

    public static string? ClaveModuloDesdePrograma(string programaNormalizado)
    {
        var raiz = programaNormalizado.Split('/')[0];
        return raiz.ToLowerInvariant() switch
        {
            "ventas" => "Ventas",
            "inventarios" => "Inventarios",
            "compras" => "Compras",
            "finanzas" => "Finanzas",
            "informes" => "Informes",
            _ => null
        };
    }
}
