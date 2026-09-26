using Neros.Domain.Globalizacion;
using Neros.Domain.Impuestos;

namespace Neros.Domain.Compras;

/// <summary>Totales comerciales al aprobar orden (plan §40, §36).</summary>
public static class MotorDocumentoCompra
{
    public static OrdenCompraCalculada Calcular(
        IReadOnlyList<LineaOrdenCompraEntrada> lineas,
        VersionImpuestosInmutable versionImpuestos,
        PoliticaRedondeo politica,
        decimal anticiposAplicados = 0,
        bool precioUnitarioIncluyeImpuesto = false)
    {
        if (lineas.Count == 0) throw new ArgumentException("Se requiere al menos una linea.", nameof(lineas));
        ValidarLineas(lineas);
        var entradasImpuesto = lineas
            .Select(l => new LineaEntrada(l.CantidadPedida, l.PrecioUnitario, l.DescuentoLinea))
            .ToList();
        var documento = MotorImpuestos.CalcularDocumento(
            entradasImpuesto, versionImpuestos, politica, anticiposAplicados, precioUnitarioIncluyeImpuesto);
        var calculadas = new List<LineaOrdenCompraCalculada>();
        for (var i = 0; i < lineas.Count; i++)
        {
            var origen = lineas[i];
            var calculada = documento.Lineas[i];
            calculadas.Add(new LineaOrdenCompraCalculada(
                origen.LineaId, origen.LineaNumero, origen.ProductoReferenciaId, origen.Descripcion,
                origen.CantidadPedida, origen.PrecioUnitario, origen.DescuentoLinea,
                calculada.Bruto, calculada.BaseNeta, calculada.TotalImpuestos, calculada.TotalRetenciones,
                calculada.Impuestos, calculada.Retenciones));
        }
        return new OrdenCompraCalculada(
            calculadas, documento.Subtotal, documento.TotalImpuestos, documento.TotalRetenciones,
            documento.Total, documento.ValorAPagar);
    }

    public static OrdenCompraAprobada Aprobar(
        OrdenCompraBorrador borrador,
        DatosProveedorVivo proveedor,
        VersionImpuestosInmutable versionImpuestos,
        PoliticaRedondeo politica,
        Guid aprobadorUsuarioId,
        string versionSoftware,
        DateTime aprobadoEnUtc)
    {
        ArgumentNullException.ThrowIfNull(borrador);
        MaquinaEstadosOrdenCompra.ValidarEditable(borrador.Estado);
        SegregacionFuncionesCompras.ValidarAprobacion(borrador.CreadoPorUsuarioId, aprobadorUsuarioId);
        if (borrador.ProveedorTerceroId != proveedor.TerceroId)
            throw new InvalidOperationException("El proveedor de la orden no coincide con los datos vivos.");
        if (string.IsNullOrWhiteSpace(versionSoftware))
            throw new ArgumentException("Version de software requerida.", nameof(versionSoftware));
        var snapshot = SnapshotProveedor.Desde(proveedor);
        var totales = Calcular(
            borrador.Lineas, versionImpuestos, politica, borrador.AnticiposAplicados, borrador.PrecioUnitarioIncluyeImpuesto);
        var version = versionImpuestos.Validada();
        return new OrdenCompraAprobada(
            MaquinaEstadosOrdenCompra.Aprobar(borrador.Estado),
            snapshot,
            version.Id,
            version.NumeroVersion,
            versionSoftware.Trim(),
            totales,
            aprobadorUsuarioId,
            aprobadoEnUtc);
    }

    private static void ValidarLineas(IReadOnlyList<LineaOrdenCompraEntrada> lineas)
    {
        foreach (var linea in lineas)
        {
            if (linea.CantidadPedida <= 0) throw new ArgumentOutOfRangeException(nameof(lineas), "Cantidad pedida debe ser positiva.");
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
