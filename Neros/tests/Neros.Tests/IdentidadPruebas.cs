using System.Security.Cryptography;

namespace Neros.Tests;

internal static class IdentidadPruebas
{
    public const string Emisor = "https://identity.example.test";
    public static readonly Guid TenantNerosTest = Guid.Parse("f70c8608-2f4a-4d79-90b8-30f1b569bd17");

    private static readonly Lazy<string> PemPuente = new(() =>
    {
        using var rsa = RSA.Create(2048);
        return rsa.ExportPkcs8PrivateKeyPem();
    });

    public static string BridgeSigningKeyPem => PemPuente.Value;
}
