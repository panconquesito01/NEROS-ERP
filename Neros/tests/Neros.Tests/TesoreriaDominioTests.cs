using Neros.Domain.Tesoreria;
using Xunit;

namespace Neros.Tests;

public sealed class TesoreriaDominioTests
{
    private static readonly Guid CuentaBanco = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid CuentaCaja = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public void Saldo_SumaIngresosRestaEgresos()
    {
        var movimientos = new[]
        {
            Mov(CuentaBanco, TipoMovimientoTesoreria.Ingreso, 1_000_000m),
            Mov(CuentaBanco, TipoMovimientoTesoreria.Egreso, 200_000m)
        };
        Assert.Equal(800_000m, CalculadorSaldoTesoreria.Saldo(0, movimientos, CuentaBanco));
    }

    [Fact]
    public void Transferencia_GeneraSalidaYEntradaIguales()
    {
        var resultado = GestorTransferenciaTesoreria.Registrar(new TransferenciaTesoreriaEntrada(
            Guid.NewGuid(), CuentaBanco, CuentaCaja, 500_000m, new DateOnly(2026, 9, 26)));
        Assert.Equal(500_000m, resultado.Salida.Importe);
        Assert.Equal(500_000m, resultado.Entrada.Importe);
        Assert.Equal(TipoMovimientoTesoreria.TransferenciaSalida, resultado.Salida.Tipo);
        Assert.Equal(TipoMovimientoTesoreria.TransferenciaEntrada, resultado.Entrada.Tipo);
    }

    [Fact]
    public void Conciliacion_CalculaDiferenciaLibroVsExtracto()
    {
        var mov1 = Mov(CuentaBanco, TipoMovimientoTesoreria.Ingreso, 300_000m);
        var lin1 = new LineaExtractoEntrada(Guid.NewGuid(), new DateOnly(2026, 9, 25), 300_000m, EsCredito: true);
        var extracto = new ExtractoBancarioEntrada(
            Guid.NewGuid(), CuentaBanco, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30),
            0, 300_000m, [lin1]);
        var resultado = MotorConciliacionBancaria.Conciliar(new ConciliacionBancariaEntrada(
            Guid.NewGuid(), CuentaBanco, extracto, 0, [mov1],
            [new ParConciliacionEntrada(mov1.Id, lin1.Id, 300_000m)]));
        Assert.Equal(300_000m, resultado.SaldoLibroFinal);
        Assert.Equal(300_000m, resultado.SaldoExtractoFinal);
        Assert.Equal(0, resultado.Diferencia);
        Assert.Empty(resultado.MovimientosSinConciliar);
    }

    [Fact]
    public void Conciliacion_RechazaCierreConDiferencia()
    {
        Assert.Throws<InvalidOperationException>(() =>
            MotorConciliacionBancaria.ValidarCierre(EstadoConciliacion.Cerrada, 100m));
    }

    [Fact]
    public void Extracto_SaldoFinalCoherenteConLineas()
    {
        var lineas = new[]
        {
            new LineaExtractoEntrada(Guid.NewGuid(), new DateOnly(2026, 9, 1), 100m, true),
            new LineaExtractoEntrada(Guid.NewGuid(), new DateOnly(2026, 9, 2), 40m, false)
        };
        Assert.Equal(60m, MotorConciliacionBancaria.SaldoExtractoDesdeLineas(0, lineas));
    }

    private static MovimientoTesoreriaEntrada Mov(Guid cuenta, TipoMovimientoTesoreria tipo, decimal importe) =>
        new(Guid.NewGuid(), cuenta, tipo, importe, new DateOnly(2026, 9, 26));
}
