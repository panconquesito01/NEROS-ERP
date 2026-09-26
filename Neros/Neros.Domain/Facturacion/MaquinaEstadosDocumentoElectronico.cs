namespace Neros.Domain.Facturacion;

/// <summary>Transiciones permitidas del documento electronico (plan §50).</summary>
public static class MaquinaEstadosDocumentoElectronico
{
    public static EstadoDocumentoElectronico GenerarXml(EstadoDocumentoElectronico actual)
    {
        if (actual != EstadoDocumentoElectronico.Borrador)
            throw new InvalidOperationException("Solo un electronico en borrador puede generarse.");
        return EstadoDocumentoElectronico.Generado;
    }

    public static EstadoDocumentoElectronico Firmar(EstadoDocumentoElectronico actual)
    {
        if (actual != EstadoDocumentoElectronico.Generado)
            throw new InvalidOperationException("Solo un electronico generado puede firmarse.");
        return EstadoDocumentoElectronico.Firmado;
    }

    public static EstadoDocumentoElectronico Enviar(EstadoDocumentoElectronico actual)
    {
        if (actual != EstadoDocumentoElectronico.Firmado)
            throw new InvalidOperationException("Solo un electronico firmado puede enviarse a la DIAN.");
        return EstadoDocumentoElectronico.Enviado;
    }

    public static EstadoDocumentoElectronico MarcarValidado(EstadoDocumentoElectronico actual)
    {
        if (actual != EstadoDocumentoElectronico.Enviado)
            throw new InvalidOperationException("Solo un electronico enviado puede validarse.");
        return EstadoDocumentoElectronico.Validado;
    }

    public static EstadoDocumentoElectronico MarcarRechazado(EstadoDocumentoElectronico actual)
    {
        if (actual is not (EstadoDocumentoElectronico.Enviado or EstadoDocumentoElectronico.Firmado))
            throw new InvalidOperationException("Solo un electronico enviado o firmado puede rechazarse.");
        return EstadoDocumentoElectronico.Rechazado;
    }

    public static EstadoDocumentoElectronico MarcarEntregado(EstadoDocumentoElectronico actual)
    {
        if (actual != EstadoDocumentoElectronico.Validado)
            throw new InvalidOperationException("Solo un electronico validado puede marcarse entregado.");
        return EstadoDocumentoElectronico.Entregado;
    }

    public static EstadoDocumentoElectronico Anular(EstadoDocumentoElectronico actual)
    {
        if (actual is EstadoDocumentoElectronico.Anulado or EstadoDocumentoElectronico.Entregado)
            throw new InvalidOperationException("No se puede anular un electronico entregado o ya anulado.");
        return EstadoDocumentoElectronico.Anulado;
    }

    public static bool PdfIndicaExitoDian(EstadoDocumentoElectronico estado) =>
        estado is EstadoDocumentoElectronico.Validado or EstadoDocumentoElectronico.Entregado;
}
