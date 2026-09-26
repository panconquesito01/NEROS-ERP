using Neros.Domain.Globalizacion;
using Neros.Domain.Impuestos;

namespace Neros.Domain.Ventas;

/// <summary>Totales comerciales y snapshot al confirmar (plan §40, §36).</summary>
public static class MotorDocumentoVenta
{
    public static DocumentoVentaCalculado Calcular(
        IReadOnlyList<LineaDocumentoVentaEntrada> lineas,
        VersionImpuestosInmutable versionImpuestos,
        PoliticaRedondeo politica,
        decimal anticiposAplicados = 0,
        bool precioUnitarioIncluyeImpuesto = false)
    {
        if (lineas.Count == 0) throw new ArgumentException("Se requiere al menos una linea.", nameof(lineas));
        ValidarLineas(lineas);
        var entradasImpuesto = lineas
            .Select(l => new LineaEntrada(l.Cantidad, l.PrecioUnitario, l.DescuentoLinea))
            .ToList();
        var documento = MotorImpuestos.CalcularDocumento(
            entradasImpuesto, versionImpuestos, politica, anticiposAplicados, precioUnitarioIncluyeImpuesto);
        var calculadas = new List<LineaDocumentoVentaCalculada>();
        for (var i = 0; i < lineas.Count; i++)
        {
            var origen = lineas[i];
            var calculada = documento.Lineas[i];
            calculadas.Add(new LineaDocumentoVentaCalculada(
                origen.LineaId, origen.LineaNumero, origen.ProductoReferenciaId, origen.Descripcion,
                origen.Cantidad, origen.PrecioUnitario, origen.DescuentoLinea,
                calculada.Bruto, calculada.BaseNeta, calculada.TotalImpuestos, calculada.TotalRetenciones,
                calculada.Impuestos, calculada.Retenciones));
        }
        return new DocumentoVentaCalculado(
            calculadas, documento.Subtotal, documento.TotalImpuestos, documento.TotalRetenciones,
            documento.Total, documento.ValorAPagar);
    }

    public static DocumentoVentaConfirmado Confirmar(
        DocumentoVentaBorrador borrador,
        DatosClienteVivo cliente,
        VersionImpuestosInmutable versionImpuestos,
        PoliticaRedondeo politica,
        string versionSoftware,
        DateTime confirmadoEnUtc)
    {
        ArgumentNullException.ThrowIfNull(borrador);
        MaquinaEstadosDocumentoVenta.ValidarEditable(borrador.Estado);
        if (borrador.ClienteTerceroId != cliente.TerceroId)
            throw new InvalidOperationException("El cliente del documento no coincide con los datos vivos.");
        if (string.IsNullOrWhiteSpace(versionSoftware))
            throw new ArgumentException("Version de software requerida.", nameof(versionSoftware));
        var snapshot = SnapshotCliente.Desde(cliente);
        var totales = Calcular(
            borrador.Lineas, versionImpuestos, politica, borrador.AnticiposAplicados, borrador.PrecioUnitarioIncluyeImpuesto);
        var version = versionImpuestos.Validada();
        return new DocumentoVentaConfirmado(
            MaquinaEstadosDocumentoVenta.Confirmar(borrador.Estado),
            snapshot,
            version.Id,
            version.NumeroVersion,
            versionSoftware.Trim(),
            totales,
            confirmadoEnUtc);
    }

    public static DocumentoVentaBorrador CrearPedidoDesdeCotizacionConfirmada(
        Guid pedidoId,
        DocumentoVentaConfirmado cotizacion,
        IReadOnlyList<LineaDocumentoVentaEntrada> lineasCotizacion,
        decimal anticiposAplicados,
        bool precioUnitarioIncluyeImpuesto)
    {
        if (cotizacion.Estado != EstadoDocumentoVenta.Confirmado)
            throw new InvalidOperationException("La cotizacion debe estar confirmada.");
        if (lineasCotizacion.Count == 0) throw new ArgumentException("Lineas requeridas.", nameof(lineasCotizacion));
        return new DocumentoVentaBorrador(
            TipoDocumentoVenta.Pedido,
            pedidoId,
            EstadoDocumentoVenta.Borrador,
            cotizacion.Cliente.TerceroId,
            lineasCotizacion,
            anticiposAplicados,
            precioUnitarioIncluyeImpuesto);
    }

    private static void ValidarLineas(IReadOnlyList<LineaDocumentoVentaEntrada> lineas)
    {
        foreach (var linea in lineas)
        {
            if (linea.Cantidad <= 0) throw new ArgumentOutOfRangeException(nameof(lineas), "Cantidad debe ser positiva.");
            if (linea.PrecioUnitario < 0) throw new ArgumentOutOfRangeException(nameof(lineas), "Precio unitario invalido.");
            if (linea.DescuentoLinea < 0) throw new ArgumentOutOfRangeException(nameof(lineas), "Descuento invalido.");
            if (string.IsNullOrWhiteSpace(linea.Descripcion))
                throw new ArgumentException("Descripcion de linea requerida.", nameof(lineas));
        }
        var numeros = lineas.Select(l => l.LineaNumero).ToList();
        if (numeros.Distinct().Count() != numeros.Count)
            throw new ArgumentException("LineaNumero duplicado.", nameof(lineas));
    }
}
