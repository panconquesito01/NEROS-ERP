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

    /// <summary>El API entrega fechas UTC sin zona; se marcan como UTC para que el navegador las convierta a hora local.</summary>
    public static string FechaIso(DateTime utc) => DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("O", System.Globalization.CultureInfo.InvariantCulture);

    public static string FechaUtc(DateTime utc) => utc.ToString("yyyy-MM-dd HH:mm 'UTC'", System.Globalization.CultureInfo.InvariantCulture);

    public static string Numero(decimal valor) => valor.ToString("N4", System.Globalization.CultureInfo.CurrentCulture);

    /// <summary>Navegador y sistema aproximados a partir del agente de usuario; es informativo, no identifica al equipo.</summary>
    public static (string? Navegador, string? Sistema) Dispositivo(string? agente)
    {
        if (string.IsNullOrWhiteSpace(agente)) return (null, null);
        string? navegador = agente switch
        {
            _ when agente.Contains("Edg/", StringComparison.Ordinal) => "Edge",
            _ when agente.Contains("OPR/", StringComparison.Ordinal) => "Opera",
            _ when agente.Contains("Firefox/", StringComparison.Ordinal) => "Firefox",
            _ when agente.Contains("Chrome/", StringComparison.Ordinal) => "Chrome",
            _ when agente.Contains("Safari/", StringComparison.Ordinal) => "Safari",
            _ => null
        };
        string? sistema = agente switch
        {
            _ when agente.Contains("Windows", StringComparison.Ordinal) => "Windows",
            _ when agente.Contains("Android", StringComparison.Ordinal) => "Android",
            _ when agente.Contains("iPhone", StringComparison.Ordinal) || agente.Contains("iPad", StringComparison.Ordinal) => "iOS",
            _ when agente.Contains("Mac OS X", StringComparison.Ordinal) => "macOS",
            _ when agente.Contains("Linux", StringComparison.Ordinal) => "Linux",
            _ => null
        };
        return (navegador, sistema);
    }

    /// <summary>Devuelve la clave de TextosComunes de la seccion actual, o null si la ruta no tiene seccion.</summary>
    public static string? ClaveSeccion(string rutaRelativa) =>
        rutaRelativa.Split('?', '#')[0].Trim('/').ToLowerInvariant() switch
        {
            "home" => "Seccion.Inicio",
            "empresas" => "Seccion.Empresas",
            "cuenta/clave" => "Seccion.CambioClave",
            "cuenta/sesiones" => "Seccion.Sesiones",
            "administracion/usuarios" => "Seccion.Usuarios",
            "empresa/configuracion" => "Seccion.ConfiguracionEmpresa",
            "administracion/empresa/usuarios" => "Seccion.UsuariosEmpresa",
            "administracion/plataforma/empresas" => "Seccion.EmpresasPlataforma",
            "ventas" => "Modulo.Ventas.Nombre",
            "inventarios" => "Modulo.Inventarios.Nombre",
            "compras" => "Modulo.Compras.Nombre",
            "finanzas" => "Modulo.Finanzas.Nombre",
            "informes" => "Modulo.Informes.Nombre",
            "cookies" => "Seccion.Cookies",
            "legal/aceptar" => "Seccion.Legal",
            _ => null
        };
}
