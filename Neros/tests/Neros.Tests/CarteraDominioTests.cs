using Neros.Domain.Cartera;
using Xunit;

namespace Neros.Tests;

public sealed class CarteraDominioTests
{
    private static readonly Guid Tercero = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public void Saldo_Documento_EsCargosMenosAbonos_Plan43()
    {
        var doc = Guid.NewGuid();
        var movimientos = new List<MovimientoCarteraEntrada>
        {
            Movimiento(doc, NaturalezaMovimientoCartera.Cargo, 1_000_000m),
            Movimiento(doc, NaturalezaMovimientoCartera.Abono, 300_000m),
            Movimiento(doc, NaturalezaMovimientoCartera.Abono, 200_000m)
        };
        Assert.Equal(500_000m, CalculadorSaldoCartera.SaldoDocumento(doc, movimientos).Saldo);
    }

    [Fact]
    public void Pago_AplicacionesYAnticipo_ConservanImporte_EjemploPlan43()
    {
        var doc1 = Guid.NewGuid();
        var doc2 = Guid.NewGuid();
        var pago = new PagoCarteraEntrada(
            Guid.NewGuid(), TipoCartera.CxC, Tercero, 1_000_000m, new DateOnly(2026, 9, 26),
            [
                new AplicacionPagoEntrada(doc1, 300_000m),
                new AplicacionPagoEntrada(doc2, 500_000m)
            ]);
        var resultado = GestorAplicacionPago.Registrar(pago,
        [
            new SaldoDocumentoCartera(doc1, 800_000m),
            new SaldoDocumentoCartera(doc2, 600_000m)
        ]);
        Assert.Equal(200_000m, resultado.ImporteAnticipo);
        GestorAplicacionPago.ValidarConservacionImporte(1_000_000m, 800_000m, 200_000m);
        Assert.Equal(3, resultado.MovimientosGenerados.Count);
    }

    [Fact]
    public void Reconstructor_ValidaProyeccionContraMovimientos()
    {
        var doc = Guid.NewGuid();
        var movimientos = new[] { Movimiento(doc, NaturalezaMovimientoCartera.Cargo, 100m) };
        ReconstructorSaldoCartera.ValidarProyeccion(doc, 100m, movimientos);
        Assert.Throws<InvalidOperationException>(() => ReconstructorSaldoCartera.ValidarProyeccion(doc, 90m, movimientos));
    }

    [Fact]
    public void Documento_CuotasDebenCuadrarConTotal()
    {
        var docId = Guid.NewGuid();
        var entrada = new DocumentoCarteraEntrada(
            docId, TipoCartera.CxP, Tercero, "OC-001", new DateOnly(2026, 9, 1), 600_000m,
            [
                new CuotaDocumentoEntrada(1, new DateOnly(2026, 9, 30), 300_000m),
                new CuotaDocumentoEntrada(2, new DateOnly(2026, 10, 30), 300_000m)
            ]);
        var cargo = GestorDocumentoCartera.CargoInicialDocumento(entrada);
        Assert.Equal(600_000m, cargo.Importe);
    }

    [Fact]
    public void SaldoTercero_AgrupaVariosDocumentos()
    {
        var doc1 = Guid.NewGuid();
        var doc2 = Guid.NewGuid();
        var movimientos = new[]
        {
            Movimiento(doc1, NaturalezaMovimientoCartera.Cargo, 100m),
            Movimiento(doc2, NaturalezaMovimientoCartera.Cargo, 50m),
            Movimiento(doc1, NaturalezaMovimientoCartera.Abono, 20m)
        };
        Assert.Equal(130m, CalculadorSaldoCartera.SaldoTercero(Tercero, TipoCartera.CxC, movimientos).Saldo);
    }

    private static MovimientoCarteraEntrada Movimiento(Guid doc, NaturalezaMovimientoCartera naturaleza, decimal importe) =>
        new(Guid.NewGuid(), TipoCartera.CxC, Tercero, doc, naturaleza, ConceptoMovimientoCartera.Factura, importe, new DateOnly(2026, 9, 26));
}
