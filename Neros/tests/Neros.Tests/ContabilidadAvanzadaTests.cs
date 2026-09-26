using Neros.Domain.Contabilidad;
using Neros.Domain.Contabilidad.DiferenciaCambio;
using Neros.Domain.Contabilidad.Reportes;
using Neros.Domain.Globalizacion;
using Xunit;

namespace Neros.Tests;

public sealed class ContabilidadAvanzadaTests
{
    private static PoliticaRedondeo PoliticaCop() => CatalogoIso.PoliticaPorDefecto("COP");

    private static readonly Guid Activo = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Pasivo = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Patrimonio = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid Ingreso = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Fact]
    public void ControlSituacionFinanciera_MuestraDiferenciaSiNoCuadra_Plan41()
    {
        var cuentas = new Dictionary<Guid, CuentaContable>
        {
            [Activo] = new(Activo, "1", NaturalezaCuenta.Debito, TipoCuenta.Activo, true),
            [Pasivo] = new(Pasivo, "2", NaturalezaCuenta.Credito, TipoCuenta.Pasivo, true),
            [Patrimonio] = new(Patrimonio, "3", NaturalezaCuenta.Credito, TipoCuenta.Patrimonio, true)
        };
        var saldos = new Dictionary<Guid, decimal> { [Activo] = 1000m, [Pasivo] = -400m, [Patrimonio] = -500m };
        var control = MotorReportesFinancieros.ControlSituacionFinanciera(cuentas, saldos, despuesCierreResultados: true);
        Assert.False(control.Cumple);
        Assert.Equal(100m, control.Diferencia);
    }

    [Fact]
    public void Formula_VariacionPorcentual_NaSiAnteriorEsCero()
    {
        var valores = new Dictionary<string, decimal?> { ["ACTUAL"] = 100m, ["ANTERIOR"] = 0m };
        var linea = new LineaReporteDefinicion(
            "VAR_PCT", "Variacion %", TipoLineaReporte.Formula, TipoFormulaReporte.PERCENT, "ACTUAL", "ANTERIOR", []);
        var resultado = MotorFormulasReporte.Evaluar(linea, valores, PoliticaCop());
        Assert.True(resultado.EsNoAplica);
    }

    [Fact]
    public void Formula_Ratio_LiquidezCorriente()
    {
        var valores = new Dictionary<string, decimal?> { ["AC"] = 300m, ["PC"] = 150m };
        var linea = new LineaReporteDefinicion(
            "LIQ", "Liquidez corriente", TipoLineaReporte.Formula, TipoFormulaReporte.RATIO, "AC", "PC", []);
        var resultado = MotorFormulasReporte.Evaluar(linea, valores, PoliticaCop());
        Assert.Equal(2m, resultado.Valor);
    }

    [Fact]
    public void Reporte_SumaLineasDeCuenta()
    {
        var c1 = Guid.NewGuid();
        var c2 = Guid.NewGuid();
        var definicion = new DefinicionReporteFinanciero("BALANCE", TipoReporteFinanciero.SituacionFinanciera,
        [
            new SeccionReporteDefinicion("ACT", "Activo",
            [
                new LineaReporteDefinicion("ACT_TOTAL", "Total activo", TipoLineaReporte.Cuenta, null, null, null, [c1, c2])
            ])
        ]);
        var resultado = MotorReportesFinancieros.Generar(definicion, new Dictionary<Guid, decimal> { [c1] = 100m, [c2] = 50m }, PoliticaCop());
        Assert.Equal(150m, resultado.Valores.Single(v => v.CodigoLinea == "ACT_TOTAL").Valor);
    }

    [Fact]
    public void DiferenciaCambio_Realizada_Plan45()
    {
        var diff = MotorDiferenciaCambio.CalcularRealizada(1000m, 4200m, 4000m);
        Assert.Equal(200_000m, diff);
        Assert.True(MotorDiferenciaCambio.EsIngreso(MotorDiferenciaCambio.ContextoCartera.CxC, diff));
        Assert.True(MotorDiferenciaCambio.EsGasto(MotorDiferenciaCambio.ContextoCartera.CxP, diff));
    }

    [Fact]
    public void DiferenciaCambio_PagoParcial_UsaImportePagado()
    {
        var diff = MotorDiferenciaCambio.CalcularRealizadaRedondeada(250m, 4100m, 4000m, PoliticaCop());
        Assert.Equal(25_000m, diff);
    }
}
