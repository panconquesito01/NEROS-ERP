namespace Neros.Domain.Integracion;

/// <summary>Rechaza eventos fuera de orden para un mismo agregado (plan §55 desorden).</summary>
public sealed class OrdenadorVersionAgregado
{
    private readonly Dictionary<string, long> _ultimaVersion = new(StringComparer.Ordinal);

    public bool DebeProcesar(string agregadoId, long versionAgregado)
    {
        if (string.IsNullOrWhiteSpace(agregadoId)) throw new ArgumentException("Agregado requerido.", nameof(agregadoId));
        if (versionAgregado <= 0) throw new ArgumentOutOfRangeException(nameof(versionAgregado));
        if (_ultimaVersion.TryGetValue(agregadoId, out var ultima) && versionAgregado <= ultima)
            return false;
        _ultimaVersion[agregadoId] = versionAgregado;
        return true;
    }
}
