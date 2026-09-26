using Neros.Domain.Compras;
using Neros.Domain.Globalizacion;
using Neros.Domain.Impuestos;
using Xunit;

namespace Neros.Tests;

public sealed class ComprasDominioTests
{
    private static PoliticaRedondeo PoliticaCop() => CatalogoIso.PoliticaPorDefecto("COP");

    private static readonly Guid Creador = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid Aprobador = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private static VersionImpuestosInmutable VersionIva() => new(
        Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), 1,
        [new ReglaCalculoImpuesto("IVA", 0.19m, 0, false, false, false)]);

    private static IReadOnlyList<LineaOrdenCompraEntrada> LineasOrden() =>
    [
        new(Guid.Parse("11111111-1111-1111-1111-111111111101"), 1, null, "Insumo A", 100, 10_000m, 0),
        new(Guid.Parse("11111111-1111-1111-1111-111111111102"), 2, null, "Insumo B", 50, 20_000m, 0)
    ];

    [Fact]
    public void Aprobar_CongelaSnapshotYTotales_Plan40()
    {
        var borrador = new OrdenCompraBorrador(
            Guid.NewGuid(), EstadoOrdenCompra.Borrador, Guid.NewGuid(), Creador, LineasOrden(), 0, false);
        var proveedor = new DatosProveedorVivo(
            borrador.ProveedorTerceroId, "NIT", "900999888-7", "Proveedor Test", null, null, null, null);
        var aprobada = MotorDocumentoCompra.Aprobar(
            borrador, proveedor, VersionIva(), PoliticaCop(), Aprobador, "Neros.Tests/1.0", DateTime.UtcNow);
        Assert.Equal(EstadoOrdenCompra.Aprobada, aprobada.Estado);
        Assert.Equal("Proveedor Test", aprobada.Proveedor.RazonSocial);
        Assert.Equal(2_000_000m, aprobada.Totales.Subtotal);
        Assert.Equal(380_000m, aprobada.Totales.TotalImpuestos);
        Assert.Equal(Aprobador, aprobada.AprobadoPorUsuarioId);
    }

    [Fact]
    public void Aprobar_RechazaMismoUsuarioQueCreo_Segregacion47()
    {
        var borrador = new OrdenCompraBorrador(
            Guid.NewGuid(), EstadoOrdenCompra.Borrador, Guid.NewGuid(), Creador, LineasOrden(), 0, false);
        var proveedor = new DatosProveedorVivo(
            borrador.ProveedorTerceroId, "NIT", "800", "P", null, null, null, null);
        Assert.Throws<InvalidOperationException>(() => MotorDocumentoCompra.Aprobar(
            borrador, proveedor, VersionIva(), PoliticaCop(), Creador, "1.0", DateTime.UtcNow));
    }

    [Fact]
    public void Recepcion_Parcial_DejaEstadoRecibidaParcial()
    {
        var lineas = new[]
        {
            new LineaOrdenParaRecepcion(Guid.Parse("11111111-1111-1111-1111-111111111101"), 100, 0),
            new LineaOrdenParaRecepcion(Guid.Parse("11111111-1111-1111-1111-111111111102"), 50, 0)
        };
        var resultado = GestorRecepcionCompra.Registrar(
            EstadoOrdenCompra.Aprobada,
            lineas,
            [new LineaRecepcionEntrada(lineas[0].LineaId, 40)]);
        Assert.Equal(EstadoOrdenCompra.RecibidaParcial, resultado.NuevoEstadoOrden);
        Assert.Equal(40, resultado.LineasActualizadas[0].CantidadRecibidaAcumulada);
        Assert.Equal(60, resultado.LineasActualizadas[0].CantidadPendiente);
    }

    [Fact]
    public void Recepcion_CompletaTodasLasLineas_RecibidaTotal()
    {
        var linea1 = Guid.Parse("11111111-1111-1111-1111-111111111101");
        var linea2 = Guid.Parse("11111111-1111-1111-1111-111111111102");
        var lineas = new[]
        {
            new LineaOrdenParaRecepcion(linea1, 100, 40),
            new LineaOrdenParaRecepcion(linea2, 50, 0)
        };
        var resultado = GestorRecepcionCompra.Registrar(
            EstadoOrdenCompra.RecibidaParcial,
            lineas,
            [
                new LineaRecepcionEntrada(linea1, 60),
                new LineaRecepcionEntrada(linea2, 50)
            ]);
        Assert.Equal(EstadoOrdenCompra.RecibidaTotal, resultado.NuevoEstadoOrden);
        Assert.All(resultado.LineasActualizadas, l => Assert.Equal(0, l.CantidadPendiente));
    }

    [Fact]
    public void Recepcion_RechazaExcesoSobrePendiente()
    {
        var lineaId = Guid.Parse("11111111-1111-1111-1111-111111111101");
        var lineas = new[] { new LineaOrdenParaRecepcion(lineaId, 10, 8) };
        Assert.Throws<InvalidOperationException>(() => GestorRecepcionCompra.Registrar(
            EstadoOrdenCompra.Aprobada,
            lineas,
            [new LineaRecepcionEntrada(lineaId, 3)]));
    }

    [Fact]
    public void Recepcion_RechazaOrdenEnBorrador()
    {
        var lineaId = Guid.Parse("11111111-1111-1111-1111-111111111101");
        Assert.Throws<InvalidOperationException>(() => GestorRecepcionCompra.Registrar(
            EstadoOrdenCompra.Borrador,
            [new LineaOrdenParaRecepcion(lineaId, 10, 0)],
            [new LineaRecepcionEntrada(lineaId, 1)]));
    }
}
