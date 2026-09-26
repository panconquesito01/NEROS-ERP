using System.Text.Json;
using Neros.Contracts.Administracion;
using Neros.Persistence.Seguridad;

namespace Neros.Persistence.Administracion;

internal static class PerfilUsuarioMapper
{
    public static void AplicarPerfil(Usuario usuario, PerfilPersonaUsuario perfil)
    {
        usuario.Nombre = perfil.NombreCompleto();
        usuario.PrimerNombre = perfil.PrimerNombre.Trim();
        usuario.SegundoNombre = string.IsNullOrWhiteSpace(perfil.SegundoNombre) ? null : perfil.SegundoNombre.Trim();
        usuario.PrimerApellido = perfil.PrimerApellido.Trim();
        usuario.SegundoApellido = string.IsNullOrWhiteSpace(perfil.SegundoApellido) ? null : perfil.SegundoApellido.Trim();
        usuario.TipoDocumento = perfil.TipoDocumento.Trim().ToUpperInvariant();
        usuario.NumeroDocumento = perfil.NumeroDocumento.Trim();
        usuario.Ciudad = string.IsNullOrWhiteSpace(perfil.Ciudad) ? null : perfil.Ciudad.Trim();
        usuario.Direccion = string.IsNullOrWhiteSpace(perfil.Direccion) ? null : perfil.Direccion.Trim();
    }

    public static PerfilPersonaUsuario LeerPerfil(Usuario usuario) => new()
    {
        PrimerNombre = usuario.PrimerNombre ?? usuario.Nombre,
        SegundoNombre = usuario.SegundoNombre,
        PrimerApellido = usuario.PrimerApellido ?? string.Empty,
        SegundoApellido = usuario.SegundoApellido,
        TipoDocumento = usuario.TipoDocumento ?? "CC",
        NumeroDocumento = usuario.NumeroDocumento ?? string.Empty,
        Ciudad = usuario.Ciudad,
        Direccion = usuario.Direccion
    };

    public static string? SerializarModulos(IReadOnlyList<string> modulos)
    {
        var lista = modulos.Where(m => !string.IsNullOrWhiteSpace(m)).Select(m => m.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        return lista.Count == 0 ? null : JsonSerializer.Serialize(lista);
    }
}
