namespace Neros.Domain.Busqueda;

public static class MotorTombstoneBusqueda
{
    public static DocumentoIndice Aplicar(DocumentoIndice actual, DateTimeOffset tombstoneEnUtc)
    {
        ArgumentNullException.ThrowIfNull(actual);
        if (tombstoneEnUtc.Offset != TimeSpan.Zero)
            throw new ArgumentException("Instante UTC requerido.", nameof(tombstoneEnUtc));
        return actual with
        {
            Activo = false,
            TombstoneEnUtc = tombstoneEnUtc,
            IndexadoEnUtc = tombstoneEnUtc
        };
    }
}
