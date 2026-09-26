using System.Security.Cryptography;
using System.Text;

namespace Neros.Database.Deploy;

public enum TipoScript { Version = 'V', Repetible = 'R', Semilla = 'S' }

public sealed record ScriptSql(
    string Nombre, TipoScript Tipo, int Numero, string Contenido, string Checksum, EncabezadoScript Encabezado)
{
    public char Codigo => (char)Tipo;
}

public sealed record ScriptValidacion(string Nombre, string Contenido);

public static class ChecksumScript
{
    public static string Calcular(string contenido)
    {
        var normalizado = contenido.TrimStart('\uFEFF').Replace("\r\n", "\n").Replace('\r', '\n');
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(normalizado)));
    }
}
