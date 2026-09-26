using System.Text.RegularExpressions;

namespace Neros.Domain.Globalizacion;

public static partial class CatalogoIso
{
    private static readonly IReadOnlyDictionary<string, MonedaIso> Monedas = new Dictionary<string, MonedaIso>(StringComparer.Ordinal)
    {
        ["COP"] = new("COP", "Peso colombiano", 2),
        ["USD"] = new("USD", "Dolar estadounidense", 2),
        ["EUR"] = new("EUR", "Euro", 2),
        ["BRL"] = new("BRL", "Real brasileno", 2),
        ["MXN"] = new("MXN", "Peso mexicano", 2),
        ["JPY"] = new("JPY", "Yen japones", 0),
        ["CLP"] = new("CLP", "Peso chileno", 0),
        ["PEN"] = new("PEN", "Sol peruano", 2)
    };

    private static readonly IReadOnlyDictionary<string, PaisIso> Paises = new Dictionary<string, PaisIso>(StringComparer.Ordinal)
    {
        ["CO"] = new("CO", "Colombia"),
        ["US"] = new("US", "Estados Unidos"),
        ["BR"] = new("BR", "Brasil"),
        ["MX"] = new("MX", "Mexico"),
        ["ES"] = new("ES", "Espana"),
        ["PE"] = new("PE", "Peru"),
        ["CL"] = new("CL", "Chile")
    };

    public static IReadOnlyCollection<MonedaIso> MonedasConocidas => Monedas.Values.ToList();
    public static IReadOnlyCollection<PaisIso> PaisesConocidos => Paises.Values.ToList();

    public static MonedaIso ObtenerMoneda(string codigo)
    {
        ValidarMoneda(codigo);
        return Monedas[codigo];
    }

    public static PaisIso ObtenerPais(string codigo)
    {
        ValidarPais(codigo);
        return Paises[codigo];
    }

    public static void ValidarMoneda(string codigo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        var normalizado = codigo.Trim().ToUpperInvariant();
        if (!MonedaValida().IsMatch(normalizado))
            throw new ArgumentException("La moneda debe ser ISO 4217 de tres letras.", nameof(codigo));
        if (!Monedas.ContainsKey(normalizado))
            throw new ArgumentException($"Moneda '{normalizado}' no esta catalogada en Neros.", nameof(codigo));
    }

    public static void ValidarPais(string codigo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        var normalizado = codigo.Trim().ToUpperInvariant();
        if (!PaisValido().IsMatch(normalizado))
            throw new ArgumentException("El pais debe ser ISO 3166-1 alpha-2.", nameof(codigo));
        if (!Paises.ContainsKey(normalizado))
            throw new ArgumentException($"Pais '{normalizado}' no esta catalogado en Neros.", nameof(codigo));
    }

    public static PoliticaRedondeo PoliticaPorDefecto(string moneda)
    {
        var codigo = moneda.Trim().ToUpperInvariant();
        return new PoliticaRedondeo(codigo, 6, ObtenerMoneda(codigo).DecimalesIso4217, ModoRedondeo.AwayFromZero).Validada();
    }

    [GeneratedRegex("^[A-Z]{3}$")]
    private static partial Regex MonedaValida();

    [GeneratedRegex("^[A-Z]{2}$")]
    private static partial Regex PaisValido();
}

public sealed record MonedaIso(string Codigo, string Nombre, int DecimalesIso4217);
public sealed record PaisIso(string Codigo, string Nombre);
