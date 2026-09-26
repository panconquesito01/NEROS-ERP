namespace Neros.Domain.Nomina;

/// <summary>Snapshot inmutable de contrato y parametros usados (plan §53).</summary>
public static class MotorSnapshotLiquidacion
{
    public static SnapshotLiquidacionNomina Crear(
        ContratoNomina contrato,
        decimal diasTrabajados,
        decimal horasTrabajadas,
        string versionSoftware,
        string? paqueteLegalCodigo,
        string? versionPaqueteLegal = null)
    {
        ArgumentNullException.ThrowIfNull(contrato);
        var (aplicaLegal, _) = MotorLaboralNomina.Evaluar(paqueteLegalCodigo);
        return new SnapshotLiquidacionNomina(
            contrato.Id,
            contrato.TipoContrato,
            contrato.SalarioBase,
            contrato.FechaInicio,
            contrato.FechaFin,
            diasTrabajados,
            horasTrabajadas,
            MotorLaboralNomina.VersionReglasDesdePaquete(paqueteLegalCodigo, versionPaqueteLegal),
            paqueteLegalCodigo,
            aplicaLegal,
            versionSoftware);
    }
}
