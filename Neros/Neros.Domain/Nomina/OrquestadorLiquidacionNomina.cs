using Neros.Domain.Globalizacion;
using Neros.Domain.Nomina.Legal;

namespace Neros.Domain.Nomina;

/// <summary>Orquesta motores separados; no concentra formulas en un unico metodo monolitico.</summary>
public static class OrquestadorLiquidacionNomina
{
    public static ResultadoLiquidacionNomina Procesar(
        ContratoNomina contrato,
        PeriodoNominaRango periodo,
        IReadOnlyList<ConceptoNominaDefinicion> conceptos,
        IReadOnlyDictionary<Guid, decimal?> importesManuales,
        decimal diasTrabajados,
        decimal horasTrabajadas,
        PoliticaRedondeo politica,
        string versionSoftware,
        string? paqueteLegalCodigo,
        ParametrosLegalesColombia? parametrosLegalesColombia = null)
    {
        MotorContractualNomina.ValidarContratoVigente(contrato, periodo);
        var (_, advertencia) = MotorLaboralNomina.Evaluar(paqueteLegalCodigo);

        var horasMes = MotorContractualNomina.HorasReferenciaMes(contrato.HorasSemanales);
        var lineas = new List<LineaLiquidacionCalculada>();
        var numero = 1;
        foreach (var concepto in conceptos.Where(c => c.Activo).OrderBy(c => c.Codigo, StringComparer.Ordinal))
        {
            importesManuales.TryGetValue(concepto.Id, out var manual);
            lineas.Add(MotorMatematicoNomina.CalcularLinea(
                numero++, concepto, contrato.SalarioBase, horasMes, horasTrabajadas, manual, politica));
        }

        ResultadoLegalColombiaNomina? detalleLegal = null;
        if (parametrosLegalesColombia is not null)
        {
            var totalesPrevios = MotorMatematicoNomina.Sumar(lineas, politica);
            detalleLegal = MotorLegalColombiaNomina.Calcular(
                totalesPrevios.TotalDevengos, contrato.SalarioBase, parametrosLegalesColombia, periodo.FechaFin, politica);
            lineas.AddRange(MotorLegalColombiaNomina.ConvertirDeduccionesALineas(detalleLegal.DeduccionesEmpleado, numero));
        }

        var totales = MotorMatematicoNomina.Sumar(lineas, politica);
        var snapshot = MotorSnapshotLiquidacion.Crear(
            contrato, diasTrabajados, horasTrabajadas, versionSoftware, paqueteLegalCodigo,
            parametrosLegalesColombia?.VersionPaquete);
        return new ResultadoLiquidacionNomina(snapshot, lineas, totales, advertencia, detalleLegal);
    }
}
