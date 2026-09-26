using System.Globalization;
using System.Text.RegularExpressions;

namespace Neros.Database.Deploy;

public sealed record EncabezadoScript(
    string Script, string Modulo, DateOnly Fecha, string Autor, string Descripcion,
    IReadOnlyList<string> Dependencias, string Objetos, string Motivo, string? Impacto,
    bool Destructivo, bool Transaccional, string Riesgo, string Rollback, string? TicketAdr,
    string Validacion, string? Aprobacion)
{
    private static readonly string[] Obligatorios =
        ["Script", "Modulo", "Fecha", "Autor", "Descripcion", "Dependencias", "Objetos", "Motivo",
         "Destructivo", "Riesgo", "Rollback", "Validacion"];
    private static readonly HashSet<string> Conocidos = new(
        [.. Obligatorios, "Impacto", "Transaccional", "Ticket/ADR", "Aprobacion"], StringComparer.OrdinalIgnoreCase);
    private static readonly Regex Campo = new(@"^\s*(?<clave>[A-Za-z/]+)\s*:\s*(?<valor>.*)$", RegexOptions.Compiled);

    public static EncabezadoScript? Leer(string contenido, string archivo, string modulo, List<string> errores)
    {
        var texto = contenido.TrimStart('\uFEFF').TrimStart();
        var fin = texto.IndexOf("*/", StringComparison.Ordinal);
        if (!texto.StartsWith("/*", StringComparison.Ordinal) || fin < 0)
        {
            errores.Add($"{archivo}: falta el encabezado /* ... */ al inicio del archivo.");
            return null;
        }

        var campos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? actual = null;
        foreach (var linea in texto[2..fin].Split('\n'))
        {
            var coincidencia = Campo.Match(linea);
            if (coincidencia.Success && Conocidos.Contains(coincidencia.Groups["clave"].Value))
            {
                actual = Conocidos.First(c => c.Equals(coincidencia.Groups["clave"].Value, StringComparison.OrdinalIgnoreCase));
                if (campos.ContainsKey(actual)) errores.Add($"{archivo}: el campo {actual} esta repetido.");
                campos[actual] = coincidencia.Groups["valor"].Value.Trim();
            }
            else if (actual is not null && linea.Trim() is { Length: > 0 } continuacion && !continuacion.StartsWith("===", StringComparison.Ordinal))
            {
                campos[actual] = $"{campos[actual]} {continuacion}".Trim();
            }
        }

        var cantidad = errores.Count;
        foreach (var obligatorio in Obligatorios.Where(c => string.IsNullOrWhiteSpace(campos.GetValueOrDefault(c))))
            errores.Add($"{archivo}: falta el campo obligatorio {obligatorio}.");
        if (errores.Count > cantidad) return null;

        if (!campos["Script"].Equals(archivo, StringComparison.Ordinal))
            errores.Add($"{archivo}: el campo Script ({campos["Script"]}) no coincide con el nombre del archivo.");
        if (!campos["Modulo"].Equals(modulo, StringComparison.Ordinal))
            errores.Add($"{archivo}: el campo Modulo ({campos["Modulo"]}) no coincide con la carpeta {modulo}.");
        if (!DateOnly.TryParseExact(campos["Fecha"], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
            errores.Add($"{archivo}: Fecha debe tener formato yyyy-MM-dd.");
        var destructivo = SiNo(campos["Destructivo"], "Destructivo", archivo, errores);
        var transaccional = !campos.TryGetValue("Transaccional", out var valorTransaccional)
            || SiNo(valorTransaccional, "Transaccional", archivo, errores);
        var riesgo = campos["Riesgo"].ToUpperInvariant();
        if (riesgo is not ("BAJO" or "MEDIO" or "ALTO"))
            errores.Add($"{archivo}: Riesgo debe ser BAJO, MEDIO o ALTO.");
        var aprobacion = campos.GetValueOrDefault("Aprobacion");
        if (destructivo && string.IsNullOrWhiteSpace(aprobacion))
            errores.Add($"{archivo}: un script Destructivo: SI exige el campo Aprobacion.");
        var dependencias = campos["Dependencias"].Equals("Ninguna", StringComparison.OrdinalIgnoreCase)
            ? []
            : campos["Dependencias"].Split([',', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (errores.Count > cantidad) return null;

        return new(campos["Script"], campos["Modulo"], fecha, campos["Autor"], campos["Descripcion"], dependencias,
            campos["Objetos"], campos["Motivo"], campos.GetValueOrDefault("Impacto"), destructivo, transaccional, riesgo,
            campos["Rollback"], campos.GetValueOrDefault("Ticket/ADR"), campos["Validacion"], aprobacion);
    }

    private static bool SiNo(string valor, string campo, string archivo, List<string> errores)
    {
        if (valor.Equals("SI", StringComparison.OrdinalIgnoreCase)) return true;
        if (!valor.Equals("NO", StringComparison.OrdinalIgnoreCase)) errores.Add($"{archivo}: {campo} debe ser SI o NO.");
        return false;
    }
}
