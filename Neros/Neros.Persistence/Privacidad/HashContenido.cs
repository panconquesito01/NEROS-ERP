using System.Security.Cryptography;
using System.Text;

namespace Neros.Persistence.Privacidad;

public static class HashContenido
{
    public static string Sha256Hex(string texto) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(texto))).ToLowerInvariant();
}
