using Neros.Domain.Globalizacion;
using Neros.Domain.Impuestos;
using Xunit;

namespace Neros.Tests;

public sealed class ImpuestosMotorTests
{
    private static PoliticaRedondeo PoliticaCop() => CatalogoIso.PoliticaPorDefecto("COP");

    private static VersionImpuestosInmutable VersionSoloIva(decimal tarifa = 0.19m) => new(
        Guid.NewGuid(), 1, [new ReglaCalculoImpuesto("IVA", tarifa, 0, false, false, false)]);

    private static VersionImpuestosInmutable VersionIvaIncluido(decimal tarifa = 0.19m) => new(
        Guid.NewGuid(), 1, [new ReglaCalculoImpuesto("IVA", tarifa, 0, true, false, false)]);

    [Fact]
    public void PorcentajeSobreBase_Iva19PorCiento_SegunPlan37()
    {
        var (baseGravable, impuesto) = MotorImpuestos.CalcularPorcentajeSobreBase(1_000_000m, 0.19m, PoliticaCop());
        Assert.Equal(1_000_000m, baseGravable);
        Assert.Equal(190_000m, impuesto);
        Assert.Equal(1_190_000m, baseGravable + impuesto);
    }

    [Fact]
    public void ImpuestoIncluido_UnSoloPorcentual_BaseMasImpuestoIgualTotal()
    {
        var total = 1_190_000m;
        var (baseGravable, impuesto) = MotorImpuestos.CalcularImpuestoIncluido(total, 0.19m, PoliticaCop());
        Assert.Equal(1_000_000m, baseGravable);
        Assert.Equal(190_000m, impuesto);
        Assert.Equal(total, baseGravable + impuesto);
    }

    [Fact]
    public void Linea_BrutoDescuentoBaseNeta_SegunPlan40()
    {
        var linea = new LineaEntrada(10, 100_000m, 50_000m);
        var calculada = MotorImpuestos.CalcularLinea(linea, VersionSoloIva(), PoliticaCop());
        Assert.Equal(1_000_000m, calculada.Bruto);
        Assert.Equal(50_000m, calculada.Descuentos);
        Assert.Equal(950_000m, calculada.BaseNeta);
        Assert.Equal(180_500m, calculada.TotalImpuestos);
    }

    [Fact]
    public void Documento_SubtotalImpuestosTotalYValorAPagar_SegunPlan40()
    {
        var lineas = new[]
        {
            new LineaEntrada(1, 1_000_000m, 0),
            new LineaEntrada(2, 100_000m, 0)
        };
        var version = new VersionImpuestosInmutable(Guid.NewGuid(), 1,
        [
            new ReglaCalculoImpuesto("IVA", 0.19m, 0, false, false, false),
            new ReglaCalculoImpuesto("RET-FTE", 0.025m, 1, false, true, false)
        ]);
        var documento = MotorImpuestos.CalcularDocumento(lineas, version, PoliticaCop());
        Assert.Equal(1_200_000m, documento.Subtotal);
        Assert.Equal(228_000m, documento.TotalImpuestos);
        Assert.Equal(30_000m, documento.TotalRetenciones);
        Assert.Equal(1_428_000m, documento.Total);
        Assert.Equal(1_398_000m, documento.ValorAPagar);
    }

    [Fact]
    public void Cascada_ImpuestoSobreBaseMasImpuestoAnterior()
    {
        var version = new VersionImpuestosInmutable(Guid.NewGuid(), 2,
        [
            new ReglaCalculoImpuesto("IMPO1", 0.10m, 0, false, false, false),
            new ReglaCalculoImpuesto("IMPO2", 0.05m, 1, false, false, true)
        ]);
        var linea = MotorImpuestos.CalcularLinea(new LineaEntrada(1, 1000m, 0), version, PoliticaCop());
        Assert.Equal(100m, linea.Impuestos[0].Importe);
        Assert.Equal(55m, linea.Impuestos[1].Importe);
    }

    [Fact]
    public void PrecioUnitarioIncluyeImpuesto_UsaFormulaIncluidaEnLinea()
    {
        var linea = MotorImpuestos.CalcularLinea(
            new LineaEntrada(1, 1_190_000m, 0), VersionIvaIncluido(), PoliticaCop(), precioUnitarioIncluyeImpuesto: true);
        Assert.Equal(1_000_000m, linea.BaseNeta);
        Assert.Equal(190_000m, linea.TotalImpuestos);
    }

    [Fact]
    public void Retencion_NoIncrementaTotalDocumento()
    {
        var sinRetencion = MotorImpuestos.CalcularDocumento(
            [new LineaEntrada(1, 1_000_000m, 0)], VersionSoloIva(), PoliticaCop());
        var conRetencion = MotorImpuestos.CalcularDocumento(
            [new LineaEntrada(1, 1_000_000m, 0)],
            new VersionImpuestosInmutable(Guid.NewGuid(), 1,
            [
                new ReglaCalculoImpuesto("IVA", 0.19m, 0, false, false, false),
                new ReglaCalculoImpuesto("RET", 0.01m, 1, false, true, false)
            ]),
            PoliticaCop());
        Assert.Equal(sinRetencion.Total, conRetencion.Total);
        Assert.True(conRetencion.ValorAPagar < conRetencion.Total);
    }
}
