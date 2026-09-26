namespace Neros.Domain.Nomina;

/// <summary>Reglas contractuales de vigencia y salario (plan §53).</summary>
public static class MotorContractualNomina
{
    public static void ValidarContratoVigente(ContratoNomina contrato, PeriodoNominaRango periodo)
    {
        ArgumentNullException.ThrowIfNull(contrato);
        ArgumentNullException.ThrowIfNull(periodo);
        if (contrato.Estado != EstadoContrato.Activo)
            throw new InvalidOperationException("El contrato no esta activo.");
        if (periodo.Estado != EstadoPeriodoNomina.Abierto)
            throw new InvalidOperationException("El periodo de nomina no esta abierto.");
        if (contrato.FechaInicio > periodo.FechaFin)
            throw new InvalidOperationException("El contrato no estaba vigente en el periodo.");
        if (contrato.FechaFin is { } fin && fin < periodo.FechaInicio)
            throw new InvalidOperationException("El contrato ya habia finalizado antes del periodo.");
    }

    public static decimal HorasReferenciaMes(decimal horasSemanales) =>
        horasSemanales <= 0 ? throw new ArgumentOutOfRangeException(nameof(horasSemanales)) : horasSemanales * 4.33m;
}
