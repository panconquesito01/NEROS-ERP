namespace Neros.Domain.Nomina.Electronica;

public static class MaquinaEstadosNominaElectronica
{
    public static EstadoNominaElectronica GenerarXml(EstadoNominaElectronica actual)
    {
        if (actual != EstadoNominaElectronica.Borrador)
            throw new InvalidOperationException("Solo una nomina electronica en borrador puede generarse.");
        return EstadoNominaElectronica.Generado;
    }

    public static EstadoNominaElectronica Firmar(EstadoNominaElectronica actual)
    {
        if (actual != EstadoNominaElectronica.Generado)
            throw new InvalidOperationException("Solo una nomina electronica generada puede firmarse.");
        return EstadoNominaElectronica.Firmado;
    }

    public static EstadoNominaElectronica Enviar(EstadoNominaElectronica actual)
    {
        if (actual != EstadoNominaElectronica.Firmado)
            throw new InvalidOperationException("Solo una nomina electronica firmada puede enviarse.");
        return EstadoNominaElectronica.Enviado;
    }

    public static EstadoNominaElectronica MarcarValidado(EstadoNominaElectronica actual)
    {
        if (actual != EstadoNominaElectronica.Enviado)
            throw new InvalidOperationException("Solo una nomina electronica enviada puede validarse.");
        return EstadoNominaElectronica.Validado;
    }

    public static EstadoNominaElectronica MarcarRechazado(EstadoNominaElectronica actual)
    {
        if (actual is not (EstadoNominaElectronica.Enviado or EstadoNominaElectronica.Firmado))
            throw new InvalidOperationException("Solo una nomina electronica enviada o firmada puede rechazarse.");
        return EstadoNominaElectronica.Rechazado;
    }

    public static bool RequiereLiquidacionContabilizada(EstadoLiquidacionNominaContabilizable estadoLiquidacion) =>
        estadoLiquidacion == EstadoLiquidacionNominaContabilizable.Contabilizada;
}

/// <summary>Evita acoplar el modulo electronico al enum completo de liquidacion.</summary>
public enum EstadoLiquidacionNominaContabilizable
{
    Borrador,
    Calculada,
    Contabilizada,
    Anulada
}
