using System.Text;
using System.Text.RegularExpressions;

namespace Neros.Database.Deploy;

public static class AnalizadorScript
{
    private const RegexOptions Opciones = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled;
    private static readonly Regex Destructivo = new(@"\b(DROP\s+(TABLE|DATABASE|SCHEMA)|DROP\s+COLUMN|TRUNCATE\s+TABLE)\b", Opciones);
    private static readonly Regex TipoProhibido = new(@"\b(float|real|money|smallmoney)\b", Opciones);
    private static readonly Regex LecturaSucia = new(@"\b(NOLOCK|READUNCOMMITTED|READ\s+UNCOMMITTED)\b", Opciones);
    private static readonly Regex SeleccionTotal = new(@"\bSELECT\s+(DISTINCT\s+|TOP\s*\(?\s*\d+\s*\)?\s+)*(\w+\.)?\*", Opciones);
    private static readonly Regex ControlTransaccion = new(@"\b(BEGIN\s+(DISTRIBUTED\s+)?TRAN(SACTION)?|COMMIT|ROLLBACK|SAVE\s+TRAN(SACTION)?)\b", Opciones);

    public static void Analizar(string archivo, string contenido, EncabezadoScript encabezado, List<string> errores)
    {
        var codigo = SoloCodigo(contenido);
        if (!encabezado.Destructivo && Destructivo.Match(codigo) is { Success: true } destructivo)
            errores.Add($"{archivo}: contiene '{Normalizar(destructivo.Value)}' pero no declara Destructivo: SI.");
        if (TipoProhibido.Match(codigo) is { Success: true } tipo)
            errores.Add($"{archivo}: el tipo '{tipo.Value}' esta prohibido; usar decimal con la precision de SQL_CONVENTIONS.");
        if (LecturaSucia.Match(codigo) is { Success: true } lectura)
            errores.Add($"{archivo}: '{Normalizar(lectura.Value)}' esta prohibido.");
        if (SeleccionTotal.IsMatch(codigo))
            errores.Add($"{archivo}: SELECT * esta prohibido; listar las columnas.");
        if (encabezado.Transaccional && ControlTransaccion.Match(codigo) is { Success: true } control)
            errores.Add($"{archivo}: '{Normalizar(control.Value)}' no se permite en un script transaccional; el runner abre la transaccion.");
    }

    public static string SoloCodigo(string sql)
    {
        var resultado = new StringBuilder(sql.Length);
        var escaner = new EscanerSql();
        for (var i = 0; i < sql.Length; i++)
        {
            var visible = escaner.Avanzar(sql, ref i, out var inicio);
            resultado.Append(visible ? sql.AsSpan(inicio, i - inicio + 1) : sql[i] == '\n' ? "\n" : " ");
        }
        return resultado.ToString();
    }

    private static string Normalizar(string valor) => Regex.Replace(valor.ToUpperInvariant(), @"\s+", " ");
}

internal sealed class EscanerSql
{
    private int profundidadComentario;
    private bool comentarioLinea;
    private char? cierre;

    public bool EnCodigo => profundidadComentario == 0 && !comentarioLinea && cierre is null;

    public bool Avanzar(string sql, ref int i, out int inicio)
    {
        inicio = i;
        var actual = sql[i];
        var siguiente = i + 1 < sql.Length ? sql[i + 1] : '\0';
        if (comentarioLinea)
        {
            if (actual == '\n') comentarioLinea = false;
            return false;
        }
        if (profundidadComentario > 0)
        {
            if (actual == '/' && siguiente == '*') { profundidadComentario++; i++; }
            else if (actual == '*' && siguiente == '/') { profundidadComentario--; i++; }
            return false;
        }
        if (cierre is { } fin)
        {
            if (actual == fin)
            {
                if (siguiente == fin) { i++; return false; }
                cierre = null;
                return true;
            }
            return false;
        }
        switch (actual)
        {
            case '-' when siguiente == '-': comentarioLinea = true; i++; return false;
            case '/' when siguiente == '*': profundidadComentario = 1; i++; return false;
            case '\'': cierre = '\''; return true;
            case '[': cierre = ']'; return true;
            case '"': cierre = '"'; return true;
            default: return true;
        }
    }
}

public static class DivisorLotes
{
    private static readonly Regex Separador = new(@"^\s*GO(\s+(?<repeticiones>\d+))?\s*;?\s*(--.*)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static IReadOnlyList<string> Dividir(string sql, string archivo)
    {
        var lotes = new List<string>();
        var lote = new StringBuilder();
        var escaner = new EscanerSql();
        foreach (var linea in sql.TrimStart('\uFEFF').Replace("\r\n", "\n").Split('\n'))
        {
            var coincidencia = escaner.EnCodigo ? Separador.Match(linea) : Match.Empty;
            if (coincidencia.Success)
            {
                if (coincidencia.Groups["repeticiones"].Success)
                    throw new InvalidOperationException($"{archivo}: 'GO n' no esta soportado.");
                Agregar(lotes, lote);
                continue;
            }
            var texto = linea + "\n";
            for (var i = 0; i < texto.Length; i++) escaner.Avanzar(texto, ref i, out _);
            lote.Append(texto);
        }
        Agregar(lotes, lote);
        return lotes;
    }

    private static void Agregar(List<string> lotes, StringBuilder lote)
    {
        var texto = lote.ToString();
        if (!string.IsNullOrWhiteSpace(AnalizadorScript.SoloCodigo(texto))) lotes.Add(texto.TrimEnd());
        lote.Clear();
    }
}
