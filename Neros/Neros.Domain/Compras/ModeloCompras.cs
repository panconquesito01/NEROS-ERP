namespace Neros.Domain.Compras;

public enum EstadoOrdenCompra
{
    Borrador,
    Aprobada,
    RecibidaParcial,
    RecibidaTotal
}

public sealed record DatosProveedorVivo(
    Guid TerceroId,
    string TipoIdentificacion,
    string NumeroIdentificacion,
    string RazonSocial,
    string? Direccion,
    string? Ciudad,
    string? Correo,
    string? ResponsabilidadesFiscales);

public sealed record SnapshotProveedor(
    Guid TerceroId,
    string TipoIdentificacion,
    string NumeroIdentificacion,
    string RazonSocial,
    string? Direccion,
    string? Ciudad,
    string? Correo,
    string? ResponsabilidadesFiscales)
{
    public static SnapshotProveedor Desde(DatosProveedorVivo vivo)
    {
        ArgumentNullException.ThrowIfNull(vivo);
        if (vivo.TerceroId == Guid.Empty) throw new ArgumentException("TerceroId requerido.", nameof(vivo));
        if (string.IsNullOrWhiteSpace(vivo.RazonSocial)) throw new ArgumentException("Razon social requerida.", nameof(vivo));
        if (string.IsNullOrWhiteSpace(vivo.NumeroIdentificacion))
            throw new ArgumentException("Numero de identificacion requerido.", nameof(vivo));
        return new SnapshotProveedor(
            vivo.TerceroId, vivo.TipoIdentificacion.Trim(), vivo.NumeroIdentificacion.Trim(), vivo.RazonSocial.Trim(),
            vivo.Direccion?.Trim(), vivo.Ciudad?.Trim(), vivo.Correo?.Trim(), vivo.ResponsabilidadesFiscales?.Trim());
    }
}

public sealed record LineaOrdenCompraEntrada(
    Guid LineaId,
    int LineaNumero,
    Guid? ProductoReferenciaId,
    string Descripcion,
    decimal CantidadPedida,
    decimal PrecioUnitario,
    decimal DescuentoLinea);

public sealed record LineaOrdenCompraCalculada(
    Guid LineaId,
    int LineaNumero,
    Guid? ProductoReferenciaId,
    string Descripcion,
    decimal CantidadPedida,
    decimal PrecioUnitario,
    decimal DescuentoLinea,
    decimal Bruto,
    decimal BaseNeta,
    decimal TotalImpuestosLinea,
    decimal TotalRetencionesLinea,
    IReadOnlyList<Impuestos.ResultadoImpuesto> DesgloseImpuestos,
    IReadOnlyList<Impuestos.ResultadoImpuesto> DesgloseRetenciones);

public sealed record OrdenCompraCalculada(
    IReadOnlyList<LineaOrdenCompraCalculada> Lineas,
    decimal Subtotal,
    decimal TotalImpuestos,
    decimal TotalRetenciones,
    decimal Total,
    decimal ValorAPagar);

public sealed record OrdenCompraAprobada(
    EstadoOrdenCompra Estado,
    SnapshotProveedor Proveedor,
    Guid ImpuestosVersionPublicadaId,
    int ImpuestosVersionNumero,
    string VersionSoftware,
    OrdenCompraCalculada Totales,
    Guid AprobadoPorUsuarioId,
    DateTime AprobadoEnUtc);

public sealed record OrdenCompraBorrador(
    Guid OrdenId,
    EstadoOrdenCompra Estado,
    Guid ProveedorTerceroId,
    Guid CreadoPorUsuarioId,
    IReadOnlyList<LineaOrdenCompraEntrada> Lineas,
    decimal AnticiposAplicados,
    bool PrecioUnitarioIncluyeImpuesto);

public sealed record LineaOrdenParaRecepcion(
    Guid LineaId,
    decimal CantidadPedida,
    decimal CantidadRecibidaAcumulada);

public sealed record LineaRecepcionEntrada(Guid OrdenCompraLineaId, decimal CantidadRecibida);

public sealed record LineaOrdenRecepcionActualizada(
    Guid LineaId,
    decimal CantidadPedida,
    decimal CantidadRecibidaAcumulada,
    decimal CantidadPendiente);

public sealed record ResultadoRecepcionCompra(
    EstadoOrdenCompra NuevoEstadoOrden,
    IReadOnlyList<LineaOrdenRecepcionActualizada> LineasActualizadas);
