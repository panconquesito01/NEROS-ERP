namespace Neros.Domain.Busqueda;

/// <summary>Filtra por permiso antes de paginar (D-17b).</summary>
public static class MotorAclBusqueda
{
    public static bool PuedeVer(IReadOnlySet<string> permisosUsuario, DocumentoIndice documento)
    {
        ArgumentNullException.ThrowIfNull(permisosUsuario);
        ArgumentNullException.ThrowIfNull(documento);
        if (!documento.Activo || documento.TombstoneEnUtc is not null)
            return false;
        if (string.IsNullOrWhiteSpace(documento.PermisoRequerido))
            return false;
        return permisosUsuario.Contains(documento.PermisoRequerido);
    }

    public static IEnumerable<DocumentoIndice> FiltrarVisibles(
        IEnumerable<DocumentoIndice> candidatos,
        IReadOnlySet<string> permisosUsuario,
        Guid tenantId,
        Guid empresaId)
    {
        ArgumentNullException.ThrowIfNull(candidatos);
        return candidatos.Where(d =>
            d.TenantId == tenantId
            && d.EmpresaId == empresaId
            && PuedeVer(permisosUsuario, d));
    }
}
