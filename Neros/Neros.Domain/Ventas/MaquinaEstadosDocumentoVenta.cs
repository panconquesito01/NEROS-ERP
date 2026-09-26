namespace Neros.Domain.Ventas;

/// <summary>Transiciones de cotizacion/pedido (plan §46).</summary>
public static class MaquinaEstadosDocumentoVenta
{
    public static EstadoDocumentoVenta Confirmar(EstadoDocumentoVenta actual)
    {
        if (actual != EstadoDocumentoVenta.Borrador)
            throw new InvalidOperationException("Solo un documento en borrador puede confirmarse.");
        return EstadoDocumentoVenta.Confirmado;
    }

    public static EstadoDocumentoVenta Anular(EstadoDocumentoVenta actual)
    {
        if (actual == EstadoDocumentoVenta.Anulado)
            throw new InvalidOperationException("El documento ya esta anulado.");
        return EstadoDocumentoVenta.Anulado;
    }

    public static void ValidarEditable(EstadoDocumentoVenta actual)
    {
        if (actual != EstadoDocumentoVenta.Borrador)
            throw new InvalidOperationException("Solo un documento en borrador admite cambios.");
    }
}
