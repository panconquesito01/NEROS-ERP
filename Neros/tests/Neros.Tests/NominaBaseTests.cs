using Neros.Domain.Globalizacion;
using Neros.Domain.Nomina;
using Xunit;

namespace Neros.Tests;

public sealed class NominaBaseTests
{
    private static PoliticaRedondeo PoliticaCop() => CatalogoIso.PoliticaPorDefecto("COP");

    private static ContratoNomina ContratoActivo() => new(
        Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 1, 1), null, "Indefinido",
        3_000_000m, 48m, EstadoContrato.Activo);

    private static PeriodoNominaRango PeriodoAbierto() => new(
        Guid.NewGuid(), new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), EstadoPeriodoNomina.Abierto);

    [Fact]
    public void Liquidacion_SumaDevengosYDeducciones()
    {
        var salario = new ConceptoNominaDefinicion(
            Guid.NewGuid(), "SAL", "Salario", NaturalezaConceptoNomina.Devengo,
            TipoFormulaConceptoNomina.Fijo, 3_000_000m, null, true);
        var prestamo = new ConceptoNominaDefinicion(
            Guid.NewGuid(), "PRE", "Prestamo", NaturalezaConceptoNomina.Deduccion,
            TipoFormulaConceptoNomina.Manual, null, null, true);
        var resultado = OrquestadorLiquidacionNomina.Procesar(
            ContratoActivo(), PeriodoAbierto(), [salario, prestamo],
            new Dictionary<Guid, decimal?> { [prestamo.Id] = 200_000m },
            30m, 192m, PoliticaCop(), "Neros-1.0.0", paqueteLegalCodigo: null);
        Assert.Equal(3_000_000m, resultado.Totales.TotalDevengos);
        Assert.Equal(200_000m, resultado.Totales.TotalDeducciones);
        Assert.Equal(2_800_000m, resultado.Totales.NetoPagar);
        Assert.False(resultado.Snapshot.AplicaReglasLegalesColombia);
        Assert.NotNull(resultado.AdvertenciaLegal);
    }

    [Fact]
    public void MaquinaEstados_SegregaContabilizacion()
    {
        var calculada = MaquinaEstadosLiquidacionNomina.MarcarCalculada(EstadoLiquidacionNomina.Borrador);
        var contabilizada = MaquinaEstadosLiquidacionNomina.Contabilizar(calculada);
        Assert.Equal(EstadoLiquidacionNomina.Contabilizada, contabilizada);
        Assert.Throws<InvalidOperationException>(() =>
            MaquinaEstadosLiquidacionNomina.ValidarInmutable(contabilizada));
        Assert.Throws<InvalidOperationException>(() =>
            MaquinaEstadosLiquidacionNomina.Anular(contabilizada));
    }

    [Fact]
    public void Contractual_RechazaContratoInactivo()
    {
        var contrato = ContratoActivo() with { Estado = EstadoContrato.Finalizado };
        Assert.Throws<InvalidOperationException>(() =>
            MotorContractualNomina.ValidarContratoVigente(contrato, PeriodoAbierto()));
    }

    [Fact]
    public void Matematico_PorcentajeSalario()
    {
        var concepto = new ConceptoNominaDefinicion(
            Guid.NewGuid(), "AUX", "Auxilio", NaturalezaConceptoNomina.Devengo,
            TipoFormulaConceptoNomina.PorcentajeSalario, null, 10m, true);
        var linea = MotorMatematicoNomina.CalcularLinea(
            1, concepto, 2_000_000m, 207.84m, 0m, null, PoliticaCop());
        Assert.Equal(200_000m, linea.Importe);
    }
}
