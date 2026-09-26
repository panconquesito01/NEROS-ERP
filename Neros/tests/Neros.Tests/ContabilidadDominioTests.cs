using Neros.Domain.Contabilidad;
using Neros.Domain.Globalizacion;
using Xunit;

namespace Neros.Tests;

public sealed class ContabilidadDominioTests
{
    private static PoliticaRedondeo PoliticaCop() => CatalogoIso.PoliticaPorDefecto("COP");

    private static readonly Guid CxC = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Ingresos = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Iva = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid Caja = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid Proveedor = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid Capital = Guid.Parse("66666666-6666-6666-6666-666666666666");

    private static IReadOnlyDictionary<Guid, CuentaContable> PlanBasico() => new Dictionary<Guid, CuentaContable>
    {
        [CxC] = new(CxC, "130505", NaturalezaCuenta.Debito, TipoCuenta.Activo, true),
        [Ingresos] = new(Ingresos, "4135", NaturalezaCuenta.Credito, TipoCuenta.Ingreso, true),
        [Iva] = new(Iva, "240801", NaturalezaCuenta.Credito, TipoCuenta.Pasivo, true),
        [Caja] = new(Caja, "110505", NaturalezaCuenta.Debito, TipoCuenta.Activo, true),
        [Proveedor] = new(Proveedor, "220505", NaturalezaCuenta.Credito, TipoCuenta.Pasivo, true),
        [Capital] = new(Capital, "310505", NaturalezaCuenta.Credito, TipoCuenta.Patrimonio, true)
    };

    private static PeriodoContable PeriodoAbierto() => new(
        Guid.NewGuid(), new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), EstadoPeriodoContable.Abierto);

    [Fact]
    public void MotorContabilizacion_VentaFacturada_CoincideConPlan37()
    {
        var reglas = MotorContabilizacion.ReglasVentaFacturada(
            new MotorContabilizacion.EventoVentaFacturada(1_190_000m, 1_000_000m, 190_000m), PoliticaCop());
        var movimientos = MotorContabilizacion.ResolverMovimientos(reglas, new Dictionary<RolContable, Guid>
        {
            [RolContable.CuentaCliente] = CxC,
            [RolContable.CuentaIngreso] = Ingresos,
            [RolContable.CuentaIvaGenerado] = Iva
        });
        var borrador = new ComprobanteContable(Guid.NewGuid(), PeriodoAbierto().Id, new DateOnly(2026, 1, 15),
            EstadoComprobante.Borrador, movimientos, ReglaContabilizacionVersion: 1);
        var contabilizado = MotorPartidaDoble.Contabilizar(borrador, PlanBasico(), PeriodoAbierto(), PoliticaCop());
        Assert.Equal(EstadoComprobante.Contabilizado, contabilizado.Estado);
        Assert.Equal(1_190_000m, contabilizado.Movimientos.Where(m => m.Lado == LadoMovimiento.Debito).Sum(m => m.ImporteMonedaFuncional));
    }

    [Fact]
    public void PartidaDoble_RechazaComprobanteDescuadrado()
    {
        var movimientos = new[]
        {
            new MovimientoLinea(CxC, LadoMovimiento.Debito, 100m),
            new MovimientoLinea(Ingresos, LadoMovimiento.Credito, 90m)
        };
        Assert.Throws<InvalidOperationException>(() => MotorPartidaDoble.ValidarMovimientos(movimientos, PlanBasico()));
    }

    [Fact]
    public void PeriodoCerrado_RechazaContabilizacion()
    {
        var periodo = PeriodoAbierto() with { Estado = EstadoPeriodoContable.Cerrado };
        var movimientos = new[]
        {
            new MovimientoLinea(CxC, LadoMovimiento.Debito, 100m),
            new MovimientoLinea(Ingresos, LadoMovimiento.Credito, 100m)
        };
        var borrador = new ComprobanteContable(Guid.NewGuid(), periodo.Id, new DateOnly(2026, 1, 10),
            EstadoComprobante.Borrador, movimientos);
        Assert.Throws<InvalidOperationException>(() => MotorPartidaDoble.Contabilizar(borrador, PlanBasico(), periodo, PoliticaCop()));
    }

    [Fact]
    public void BalanceComprobacion_CuadraMovimientosYSaldos()
    {
        var movimientos = new[]
        {
            new MovimientoLinea(Caja, LadoMovimiento.Debito, 1_000_000m),
            new MovimientoLinea(Capital, LadoMovimiento.Credito, 1_000_000m)
        };
        var balance = CalculadorSaldos.BalanceComprobacion(PlanBasico(), movimientos, new Dictionary<Guid, decimal>());
        Assert.True(balance.Cuadrado);
        Assert.Equal(1_000_000m, balance.TotalDebitos);
    }

    [Fact]
    public void EcuacionContable_AntesDelCierre_IncluyeResultado()
    {
        var cuentas = PlanBasico();
        var saldos = new Dictionary<Guid, decimal>
        {
            [Caja] = 1_190_000m,
            [CxC] = 0m,
            [Ingresos] = -1_000_000m,
            [Iva] = -190_000m,
            [Capital] = 0m
        };
        var ecuacion = EcuacionContable.Evaluar(cuentas, saldos, despuesCierreResultados: false);
        Assert.True(ecuacion.Cumple);
        Assert.Equal(1_190_000m, ecuacion.Activo);
    }

    [Fact]
    public void Reversion_InvierteMovimientosYEnlazaOriginal()
    {
        var movimientos = new[]
        {
            new MovimientoLinea(CxC, LadoMovimiento.Debito, 500m),
            new MovimientoLinea(Ingresos, LadoMovimiento.Credito, 500m)
        };
        var original = new ComprobanteContable(Guid.NewGuid(), PeriodoAbierto().Id, new DateOnly(2026, 1, 5),
            EstadoComprobante.Contabilizado, movimientos);
        var reversionId = Guid.NewGuid();
        var reversion = ReversionComprobante.CrearReversion(original, reversionId, "Error en el tercero");
        var marcado = ReversionComprobante.MarcarOriginalReversado(original, reversionId);
        Assert.Equal(EstadoComprobante.Reversado, marcado.Estado);
        Assert.Equal(500m, reversion.Movimientos.Single(m => m.CuentaId == Ingresos).ImporteMonedaFuncional);
        Assert.Equal(LadoMovimiento.Debito, reversion.Movimientos.Single(m => m.CuentaId == Ingresos).Lado);
    }

    [Fact]
    public void GestorPeriodo_ReaperturaExigeMotivo()
    {
        var cerrado = PeriodoAbierto() with { Estado = EstadoPeriodoContable.Cerrado };
        Assert.Throws<ArgumentException>(() => GestorPeriodo.Reabrir(cerrado, " "));
        var reabierto = GestorPeriodo.Reabrir(cerrado, "Ajuste autorizado por contador");
        Assert.Equal(EstadoPeriodoContable.Reabierto, reabierto.Estado);
    }

    [Fact]
    public void SaldoPresentacion_RespetaNaturalezaCredito()
    {
        var cuenta = PlanBasico()[Proveedor];
        var presentacion = CalculadorSaldos.SaldoPresentacion(0, cuenta.Naturaleza, 0, 250_000m);
        Assert.Equal(250_000m, presentacion);
    }
}
