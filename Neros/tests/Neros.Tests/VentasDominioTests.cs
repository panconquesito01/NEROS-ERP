using Neros.Domain.Globalizacion;
using Neros.Domain.Impuestos;
using Neros.Domain.Ventas;
using Xunit;

namespace Neros.Tests;

public sealed class VentasDominioTests
{
    private static PoliticaRedondeo PoliticaCop() => CatalogoIso.PoliticaPorDefecto("COP");

    private static VersionImpuestosInmutable VersionIvaRetencion() => new(
        Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), 3,
        [
            new ReglaCalculoImpuesto("IVA", 0.19m, 0, false, false, false),
            new ReglaCalculoImpuesto("RET-FTE", 0.025m, 1, false, true, false)
        ]);

    private static IReadOnlyList<LineaDocumentoVentaEntrada> LineasEjemplo() =>
    [
        new(Guid.NewGuid(), 1, null, "Servicio A", 1, 1_000_000m, 0),
        new(Guid.NewGuid(), 2, null, "Servicio B", 2, 100_000m, 0)
    ];

    [Fact]
    public void CalcularDocumento_AlineadoConMotorImpuestos_Plan40()
    {
        var calculado = MotorDocumentoVenta.Calcular(LineasEjemplo(), VersionIvaRetencion(), PoliticaCop());
        var referencia = MotorImpuestos.CalcularDocumento(
            [new LineaEntrada(1, 1_000_000m, 0), new LineaEntrada(2, 100_000m, 0)],
            VersionIvaRetencion(), PoliticaCop());
        Assert.Equal(referencia.Subtotal, calculado.Subtotal);
        Assert.Equal(referencia.TotalImpuestos, calculado.TotalImpuestos);
        Assert.Equal(referencia.TotalRetenciones, calculado.TotalRetenciones);
        Assert.Equal(referencia.Total, calculado.Total);
        Assert.Equal(referencia.ValorAPagar, calculado.ValorAPagar);
    }

    [Fact]
    public void Confirmar_CongelaSnapshotEImpuestos()
    {
        var borrador = new DocumentoVentaBorrador(
            TipoDocumentoVenta.Cotizacion, Guid.NewGuid(), EstadoDocumentoVenta.Borrador,
            Guid.Parse("11111111-1111-1111-1111-111111111111"), LineasEjemplo(), 0, false);
        var cliente = new DatosClienteVivo(
            borrador.ClienteTerceroId, "NIT", "900123456-7", "Cliente Demo S.A.S.",
            "Calle 1", "Bogota", "ventas@demo.co", "O-47");
        var confirmado = MotorDocumentoVenta.Confirmar(
            borrador, cliente, VersionIvaRetencion(), PoliticaCop(), "Neros.Tests/1.0", new DateTime(2026, 9, 26, 12, 0, 0, DateTimeKind.Utc));
        Assert.Equal(EstadoDocumentoVenta.Confirmado, confirmado.Estado);
        Assert.Equal("Cliente Demo S.A.S.", confirmado.Cliente.RazonSocial);
        Assert.Equal(VersionIvaRetencion().Validada().Id, confirmado.ImpuestosVersionPublicadaId);
        Assert.Equal(VersionIvaRetencion().NumeroVersion, confirmado.ImpuestosVersionNumero);
        Assert.Equal(1_398_000m, confirmado.Totales.ValorAPagar);
        Assert.Equal(2, confirmado.Totales.Lineas.Count);
    }

    [Fact]
    public void Confirmar_RechazaSiNoEsBorrador()
    {
        var borrador = new DocumentoVentaBorrador(
            TipoDocumentoVenta.Pedido, Guid.NewGuid(), EstadoDocumentoVenta.Confirmado,
            Guid.NewGuid(), LineasEjemplo(), 0, false);
        var cliente = new DatosClienteVivo(
            borrador.ClienteTerceroId, "CC", "123", "X", null, null, null, null);
        Assert.Throws<InvalidOperationException>(() => MotorDocumentoVenta.Confirmar(
            borrador, cliente, VersionIvaRetencion(), PoliticaCop(), "1.0", DateTime.UtcNow));
    }

    [Fact]
    public void MaquinaEstados_AnularDesdeConfirmado()
    {
        Assert.Equal(EstadoDocumentoVenta.Anulado, MaquinaEstadosDocumentoVenta.Anular(EstadoDocumentoVenta.Confirmado));
        Assert.Throws<InvalidOperationException>(() => MaquinaEstadosDocumentoVenta.Anular(EstadoDocumentoVenta.Anulado));
    }

    [Fact]
    public void PedidoDesdeCotizacionConfirmada_IniciaEnBorrador()
    {
        var borrador = new DocumentoVentaBorrador(
            TipoDocumentoVenta.Cotizacion, Guid.NewGuid(), EstadoDocumentoVenta.Borrador,
            Guid.Parse("22222222-2222-2222-2222-222222222222"), LineasEjemplo(), 0, false);
        var cliente = new DatosClienteVivo(
            borrador.ClienteTerceroId, "NIT", "800", "Origen", null, null, null, null);
        var cotizacion = MotorDocumentoVenta.Confirmar(
            borrador, cliente, VersionIvaRetencion(), PoliticaCop(), "1.0", DateTime.UtcNow);
        var pedido = MotorDocumentoVenta.CrearPedidoDesdeCotizacionConfirmada(
            Guid.NewGuid(), cotizacion, LineasEjemplo(), 0, false);
        Assert.Equal(TipoDocumentoVenta.Pedido, pedido.Tipo);
        Assert.Equal(EstadoDocumentoVenta.Borrador, pedido.Estado);
        Assert.Equal(cotizacion.Cliente.TerceroId, pedido.ClienteTerceroId);
    }
}
