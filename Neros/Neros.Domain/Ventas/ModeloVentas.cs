namespace Neros.Domain.Ventas;

public enum EstadoDocumentoVenta
{
    Borrador,
    Confirmado,
    Anulado
}

public enum TipoDocumentoVenta
{
    Cotizacion,
    Pedido
}

public sealed record DatosClienteVivo(
    Guid TerceroId,
    string TipoIdentificacion,
    string NumeroIdentificacion,
    string RazonSocial,
    string? Direccion,
    string? Ciudad,
    string? Correo,
    string? ResponsabilidadesFiscales);

public sealed record SnapshotCliente(
    Guid TerceroId,
    string TipoIdentificacion,
    string NumeroIdentificacion,
    string RazonSocial,
    string? Direccion,
    string? Ciudad,
    string? Correo,
    string? ResponsabilidadesFiscales)
{
    public static SnapshotCliente Desde(DatosClienteVivo vivo)
    {
        ArgumentNullException.ThrowIfNull(vivo);
        if (vivo.TerceroId == Guid.Empty) throw new ArgumentException("TerceroId requerido.", nameof(vivo));
        if (string.IsNullOrWhiteSpace(vivo.RazonSocial)) throw new ArgumentException("Razon social requerida.", nameof(vivo));
        if (string.IsNullOrWhiteSpace(vivo.NumeroIdentificacion))
            throw new ArgumentException("Numero de identificacion requerido.", nameof(vivo));
        return new SnapshotCliente(
            vivo.TerceroId, vivo.TipoIdentificacion.Trim(), vivo.NumeroIdentificacion.Trim(), vivo.RazonSocial.Trim(),
            vivo.Direccion?.Trim(), vivo.Ciudad?.Trim(), vivo.Correo?.Trim(), vivo.ResponsabilidadesFiscales?.Trim());
    }
}

public sealed record LineaDocumentoVentaEntrada(
    Guid LineaId,
    int LineaNumero,
    Guid? ProductoReferenciaId,
    string Descripcion,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal DescuentoLinea);

public sealed record LineaDocumentoVentaCalculada(
    Guid LineaId,
    int LineaNumero,
    Guid? ProductoReferenciaId,
    string Descripcion,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal DescuentoLinea,
    decimal Bruto,
    decimal BaseNeta,
    decimal TotalImpuestosLinea,
    decimal TotalRetencionesLinea,
    IReadOnlyList<Impuestos.ResultadoImpuesto> DesgloseImpuestos,
    IReadOnlyList<Impuestos.ResultadoImpuesto> DesgloseRetenciones);

public sealed record DocumentoVentaCalculado(
    IReadOnlyList<LineaDocumentoVentaCalculada> Lineas,
    decimal Subtotal,
    decimal TotalImpuestos,
    decimal TotalRetenciones,
    decimal Total,
    decimal ValorAPagar);

public sealed record DocumentoVentaConfirmado(
    EstadoDocumentoVenta Estado,
    SnapshotCliente Cliente,
    Guid ImpuestosVersionPublicadaId,
    int ImpuestosVersionNumero,
    string VersionSoftware,
    DocumentoVentaCalculado Totales,
    DateTime ConfirmadoEnUtc);

public sealed record DocumentoVentaBorrador(
    TipoDocumentoVenta Tipo,
    Guid DocumentoId,
    EstadoDocumentoVenta Estado,
    Guid ClienteTerceroId,
    IReadOnlyList<LineaDocumentoVentaEntrada> Lineas,
    decimal AnticiposAplicados,
    bool PrecioUnitarioIncluyeImpuesto);
