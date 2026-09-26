namespace Neros.Domain.Compras;

/// <summary>Segregacion creador vs aprobador (plan §47).</summary>
public static class SegregacionFuncionesCompras
{
    public static void ValidarAprobacion(Guid creadoPorUsuarioId, Guid aprobadorUsuarioId)
    {
        if (creadoPorUsuarioId == Guid.Empty || aprobadorUsuarioId == Guid.Empty)
            throw new ArgumentException("Usuarios de creacion y aprobacion requeridos.");
        if (creadoPorUsuarioId == aprobadorUsuarioId)
            throw new InvalidOperationException("El aprobador no puede ser quien creo la orden de compra.");
    }
}
