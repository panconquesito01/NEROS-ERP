namespace Neros.Domain.Nomina.Legal;

/// <summary>Control de paquete legal; solo versiones aprobadas por especialista alimentan calculo productivo.</summary>
public static class MotorPaqueteLegalColombia
{
    public const string CodigoPaquete = "CO-LEGAL";

    public static void ValidarParaCalculo(ParametrosLegalesColombia parametros, DateOnly fechaLiquidacion)
    {
        ArgumentNullException.ThrowIfNull(parametros);
        if (!string.Equals(parametros.CodigoPaquete, CodigoPaquete, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("El paquete legal no corresponde a Colombia.");
        if (parametros.EstadoAprobacion != EstadoPaqueteLegalNomina.Aprobado)
            throw new InvalidOperationException("El paquete legal Colombia no esta aprobado por especialista.");
        if (fechaLiquidacion < parametros.VigenciaDesde)
            throw new InvalidOperationException("La fecha de liquidacion es anterior a la vigencia del paquete.");
        if (parametros.VigenciaHasta is { } hasta && fechaLiquidacion > hasta)
            throw new InvalidOperationException("La fecha de liquidacion es posterior a la vigencia del paquete.");
    }
}
