using System.Globalization;
using System.Text.RegularExpressions;

namespace Neros.Database.Deploy;

public sealed class CatalogoScripts
{
    private const string Descripcion = "[a-z0-9]+(?:_[a-z0-9]+)*";
    private static readonly Regex NombreModulo = new("^[a-z][a-z0-9_]*$");
    private static readonly Regex NombreVersion = new($"^V(?<numero>\\d{{4}})__{Descripcion}\\.sql$");
    private static readonly Regex NombreRepetible = new($"^R__{Descripcion}\\.sql$");
    private static readonly Regex NombreSemilla = new($"^S(?<numero>\\d{{4}})__{Descripcion}\\.sql$");

    public required string Modulo { get; init; }
    public IReadOnlyList<ScriptSql> Versiones { get; private init; } = [];
    public IReadOnlyList<ScriptSql> Repetibles { get; private init; } = [];
    public IReadOnlyList<ScriptSql> Semillas { get; private init; } = [];
    public IReadOnlyList<ScriptValidacion> Validaciones { get; private init; } = [];
    public IReadOnlyList<string> Errores { get; private init; } = [];
    public IEnumerable<ScriptSql> Todos => Versiones.Concat(Repetibles).Concat(Semillas);

    public static CatalogoScripts Cargar(string raiz, string modulo)
    {
        var errores = new List<string>();
        var carpeta = Path.Combine(raiz, modulo);
        if (!NombreModulo.IsMatch(modulo))
            errores.Add($"Modulo '{modulo}' invalido: usar minusculas, digitos y guion bajo.");
        else if (!Directory.Exists(Path.Combine(carpeta, "migrations")))
            errores.Add($"No existe la carpeta {Path.Combine(carpeta, "migrations")}.");
        if (errores.Count > 0) return new() { Modulo = modulo, Errores = errores };

        var versiones = Leer(carpeta, "migrations", NombreVersion, TipoScript.Version, modulo, errores);
        var repetibles = Leer(carpeta, "repeatable", NombreRepetible, TipoScript.Repetible, modulo, errores);
        var semillas = Leer(carpeta, "seed", NombreSemilla, TipoScript.Semilla, modulo, errores);
        for (var i = 0; i < versiones.Count; i++)
            if (versiones[i].Numero != i + 1)
                errores.Add($"{versiones[i].Nombre}: se esperaba V{i + 1:0000}; la numeracion no admite huecos ni duplicados.");
        foreach (var duplicado in semillas.GroupBy(s => s.Numero).Where(g => g.Count() > 1))
            errores.Add($"Numero de semilla S{duplicado.Key:0000} duplicado.");

        var nombres = versiones.Concat(repetibles).Concat(semillas).ToDictionary(s => s.Nombre, StringComparer.Ordinal);
        foreach (var script in nombres.Values)
            foreach (var dependencia in script.Encabezado.Dependencias)
            {
                if (!nombres.TryGetValue(dependencia, out var requerido))
                    errores.Add($"{script.Nombre}: la dependencia {dependencia} no existe en el modulo {modulo}.");
                else if (requerido.Tipo != TipoScript.Version && script.Tipo == TipoScript.Version)
                    errores.Add($"{script.Nombre}: una version solo puede depender de otras versiones.");
                else if (script.Tipo == TipoScript.Version && requerido.Numero >= script.Numero)
                    errores.Add($"{script.Nombre}: depende de {dependencia}, que es posterior.");
            }

        var carpetaValidacion = Path.Combine(carpeta, "validation");
        var validaciones = Directory.Exists(carpetaValidacion)
            ? Directory.GetFiles(carpetaValidacion, "*.sql").Order(StringComparer.Ordinal)
                .Select(ruta => new ScriptValidacion(Path.GetFileName(ruta), File.ReadAllText(ruta))).ToList()
            : [];
        return new()
        {
            Modulo = modulo, Versiones = versiones, Repetibles = repetibles, Semillas = semillas,
            Validaciones = validaciones, Errores = errores
        };
    }

    private static List<ScriptSql> Leer(string carpeta, string subcarpeta, Regex patron, TipoScript tipo, string modulo, List<string> errores)
    {
        var ruta = Path.Combine(carpeta, subcarpeta);
        if (!Directory.Exists(ruta)) return [];
        var scripts = new List<ScriptSql>();
        foreach (var archivo in Directory.GetFiles(ruta).Order(StringComparer.Ordinal))
        {
            var nombre = Path.GetFileName(archivo);
            if (nombre.Equals(".gitkeep", StringComparison.Ordinal)) continue;
            var coincidencia = patron.Match(nombre);
            if (!coincidencia.Success)
            {
                errores.Add($"{subcarpeta}/{nombre}: nombre invalido; ver SQL_CONVENTIONS.");
                continue;
            }
            var contenido = File.ReadAllText(archivo);
            var cantidad = errores.Count;
            var encabezado = EncabezadoScript.Leer(contenido, nombre, modulo, errores);
            if (encabezado is null) continue;
            AnalizadorScript.Analizar(nombre, contenido, encabezado, errores);
            try { DivisorLotes.Dividir(contenido, nombre); }
            catch (InvalidOperationException error) { errores.Add(error.Message); }
            if (errores.Count > cantidad) continue;
            var numero = coincidencia.Groups["numero"].Success ? int.Parse(coincidencia.Groups["numero"].Value, CultureInfo.InvariantCulture) : 0;
            scripts.Add(new(nombre, tipo, numero, contenido, ChecksumScript.Calcular(contenido), encabezado));
        }
        return scripts;
    }
}
