namespace Neros.Contracts.Privacidad;

public sealed record DefinicionCookiePublica(
    string Nombre, string Almacenamiento, string Proveedor, string Categoria, string Finalidad, string Duracion,
    bool PrimeraParte, string? Dominio, bool Esencial, string? UrlPolitica);

public sealed record DocumentoLegalPendiente(string Codigo, string Nombre, int Version, Guid VersionId, string HashContenido);

public sealed record DocumentoLegalPublicado(
    string Codigo, string Nombre, int Version, Guid VersionId, string Contenido, string HashContenido, bool RequiereAceptacion);

public sealed record SolicitudAceptacionLegal(Guid VersionId, string HashContenido);

public static class CodigosPrivacidad
{
    public const string DocumentosPendientes = "documentos_pendientes";
    public const string VersionInvalida = "version_invalida";
    public const string DocumentoNoEncontrado = "documento_no_encontrado";
}

public static class FormasAceptacionLegal
{
    public const string Explicita = "Explicita";
    public const string ConsentimientoCookies = "ConsentimientoCookies";
}
