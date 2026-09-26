using Neros.Domain.Globalizacion;

namespace Neros.Domain.Nomina;

/// <summary>Calculo puro de conceptos y totales (plan §53).</summary>
public static class MotorMatematicoNomina
{
    public static LineaLiquidacionCalculada CalcularLinea(
        int lineaNumero,
        ConceptoNominaDefinicion concepto,
        decimal salarioBase,
        decimal horasReferenciaMes,
        decimal horasTrabajadas,
        decimal? importeManual,
        PoliticaRedondeo politica)
    {
        ArgumentNullException.ThrowIfNull(concepto);
        if (!concepto.Activo) throw new InvalidOperationException($"El concepto {concepto.Codigo} no esta activo.");
        var politicaValidada = politica.Validada();

        decimal baseCalculo;
        decimal tarifa;
        decimal bruto = concepto.TipoFormula switch
        {
            TipoFormulaConceptoNomina.Fijo => concepto.ValorFijo ?? throw new InvalidOperationException("Valor fijo requerido."),
            TipoFormulaConceptoNomina.PorcentajeSalario => CalcularPorcentaje(
                salarioBase, concepto.PorcentajeSalario ?? throw new InvalidOperationException("Porcentaje requerido.")),
            TipoFormulaConceptoNomina.Horas => CalcularHoras(
                salarioBase, horasReferenciaMes, horasTrabajadas),
            TipoFormulaConceptoNomina.Manual => importeManual ?? throw new InvalidOperationException("Importe manual requerido."),
            _ => throw new ArgumentOutOfRangeException(nameof(concepto))
        };

        (baseCalculo, tarifa) = concepto.TipoFormula switch
        {
            TipoFormulaConceptoNomina.Fijo => (0m, concepto.ValorFijo ?? 0m),
            TipoFormulaConceptoNomina.PorcentajeSalario => (salarioBase, concepto.PorcentajeSalario ?? 0m),
            TipoFormulaConceptoNomina.Horas => (horasTrabajadas, salarioBase / horasReferenciaMes),
            TipoFormulaConceptoNomina.Manual => (importeManual ?? 0m, 1m),
            _ => (0m, 0m)
        };

        var importe = MotorRedondeo.Aplicar(bruto, politicaValidada);
        if (importe < 0) throw new InvalidOperationException("El importe calculado no puede ser negativo.");

        return new LineaLiquidacionCalculada(
            lineaNumero, concepto.Id, concepto.Codigo, concepto.Nombre, concepto.Naturaleza, concepto.TipoFormula,
            baseCalculo, tarifa, importe);
    }

    public static TotalesLiquidacionNomina Sumar(IReadOnlyList<LineaLiquidacionCalculada> lineas, PoliticaRedondeo politica)
    {
        var devengos = lineas.Where(l => l.Naturaleza == NaturalezaConceptoNomina.Devengo).Sum(l => l.Importe);
        var deducciones = lineas.Where(l => l.Naturaleza == NaturalezaConceptoNomina.Deduccion).Sum(l => l.Importe);
        var politicaValidada = politica.Validada();
        devengos = MotorRedondeo.Aplicar(devengos, politicaValidada);
        deducciones = MotorRedondeo.Aplicar(deducciones, politicaValidada);
        var neto = MotorRedondeo.Aplicar(devengos - deducciones, politicaValidada);
        return new TotalesLiquidacionNomina(devengos, deducciones, neto);
    }

    private static decimal CalcularPorcentaje(decimal salarioBase, decimal porcentaje) =>
        salarioBase * porcentaje / 100m;

    private static decimal CalcularHoras(decimal salarioBase, decimal horasReferenciaMes, decimal horasTrabajadas)
    {
        if (horasReferenciaMes <= 0) throw new ArgumentOutOfRangeException(nameof(horasReferenciaMes));
        if (horasTrabajadas < 0) throw new ArgumentOutOfRangeException(nameof(horasTrabajadas));
        return salarioBase / horasReferenciaMes * horasTrabajadas;
    }
}
