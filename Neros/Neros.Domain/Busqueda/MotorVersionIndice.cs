namespace Neros.Domain.Busqueda;

/// <summary>Evita sobrescribir el indice con versiones antiguas del agregado.</summary>
public sealed class MotorVersionIndice
{
    private readonly Dictionary<string, long> _ultimaVersion = new(StringComparer.Ordinal);

    private static string Clave(string tipoEntidad, string entidadId) => tipoEntidad + "|" + entidadId;

    public bool DebeIndexar(string tipoEntidad, string entidadId, long versionAgregado)
    {
        if (string.IsNullOrWhiteSpace(tipoEntidad) || string.IsNullOrWhiteSpace(entidadId))
            throw new ArgumentException("Entidad requerida.");
        if (versionAgregado <= 0) throw new ArgumentOutOfRangeException(nameof(versionAgregado));
        var clave = Clave(tipoEntidad, entidadId);
        if (_ultimaVersion.TryGetValue(clave, out var ultima) && versionAgregado <= ultima)
            return false;
        _ultimaVersion[clave] = versionAgregado;
        return true;
    }
}
