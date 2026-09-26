using Neros.Domain.Inventario;
using Xunit;

namespace Neros.Tests;

public sealed class InventarioDominioTests
{
    private static readonly ClaveExistencia Clave = new(Guid.NewGuid(), Guid.NewGuid());
    private static readonly PoliticaBodega Estricta = new(false);
    private static readonly PoliticaBodega PermiteNegativos = new(true);

    private static MovimientoInventarioPendiente Entrada(DateOnly fecha, int seq, decimal cant, decimal costo, Guid? id = null) =>
        new(id ?? Guid.NewGuid(), fecha, seq, TipoMovimientoInventario.Entrada, true, cant, costo);

    private static MovimientoInventarioPendiente Salida(DateOnly fecha, int seq, decimal cant, Guid? id = null) =>
        new(id ?? Guid.NewGuid(), fecha, seq, TipoMovimientoInventario.Salida, false, cant, null);

    [Fact]
    public void PromedioPonderado_EntradaActualizaCosto()
    {
        var movimientos = new[]
        {
            Entrada(new DateOnly(2026, 1, 1), 1, 100, 10m),
            Entrada(new DateOnly(2026, 1, 2), 1, 100, 20m)
        };
        var (estado, _) = RecosteadorInventario.Recostear(movimientos, Estricta);
        Assert.Equal(200, estado.Cantidad);
        Assert.Equal(15m, estado.CostoPromedio);
    }

    [Fact]
    public void SalidaSeValoraAlPromedioVigente()
    {
        var movimientos = new[]
        {
            Entrada(new DateOnly(2026, 1, 1), 1, 10, 100m),
            Salida(new DateOnly(2026, 1, 2), 1, 4)
        };
        var (_, procesados) = RecosteadorInventario.Recostear(movimientos, Estricta);
        var salida = procesados.Last();
        Assert.Equal(100m, salida.CostoUnitarioAplicado);
        Assert.Equal(400m, salida.ValorTotal);
    }

    [Fact]
    public void MovimientoRetroactivoRecosteaPosteriores()
    {
        var idEntradaTardia = Guid.NewGuid();
        var movimientos = new List<MovimientoInventarioPendiente>
        {
            Entrada(new DateOnly(2026, 1, 10), 1, 10, 100m),
            Salida(new DateOnly(2026, 1, 15), 1, 5),
            Entrada(new DateOnly(2026, 1, 5), 2, 10, 50m, idEntradaTardia)
        };
        var (estado, procesados) = RecosteadorInventario.Recostear(movimientos, Estricta);
        var entradaRetro = procesados.Single(p => p.Id == idEntradaTardia);
        Assert.Equal(50m, entradaRetro.CostoUnitarioAplicado);
        Assert.Equal(15, estado.Cantidad);
        Assert.Equal(75m, estado.CostoPromedio);
    }

    [Fact]
    public void PeriodoCerradoRechazaMovimiento()
    {
        var movimientos = new[] { Entrada(new DateOnly(2026, 1, 15), 1, 1, 10m) };
        var periodos = new[] { new PeriodoInventario(Guid.NewGuid(), new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), EstadoPeriodoInventario.Cerrado) };
        Assert.Throws<InvalidOperationException>(() => RecosteadorInventario.Recostear(movimientos, Estricta, periodos));
    }

    [Fact]
    public void ExistenciaNegativaProhibidaPorDefecto()
    {
        var movimientos = new[] { Salida(new DateOnly(2026, 1, 1), 1, 1) };
        Assert.Throws<InvalidOperationException>(() => RecosteadorInventario.Recostear(movimientos, Estricta));
    }

    [Fact]
    public void ExistenciaNegativaPermitidaMarcaAjuste()
    {
        var movimientos = new[]
        {
            Entrada(new DateOnly(2026, 1, 1), 1, 5, 100m),
            Salida(new DateOnly(2026, 1, 2), 1, 8)
        };
        var (_, procesados) = RecosteadorInventario.Recostear(movimientos, PermiteNegativos);
        Assert.True(procesados.Last().MarcadoAjusteNegativo);
    }

    [Fact]
    public async Task Concurrencia_DosSalidasSimultaneas_NoDejanNegativoSinPoliticaAsync()
    {
        var libro = new LibroInventario(Estricta);
        libro.Registrar(Entrada(new DateOnly(2026, 1, 1), 1, 10, 50m), Clave);
        var salida1 = Salida(new DateOnly(2026, 1, 2), 1, 6);
        var salida2 = Salida(new DateOnly(2026, 1, 2), 2, 6);
        var errores = 0;
        await Task.WhenAll(
            Task.Run(async () => { try { await libro.RegistrarSalidaConcurrenteAsync(salida1, Clave); } catch { Interlocked.Increment(ref errores); } }),
            Task.Run(async () => { try { await libro.RegistrarSalidaConcurrenteAsync(salida2, Clave); } catch { Interlocked.Increment(ref errores); } }));
        Assert.Equal(1, errores);
        Assert.Equal(4, libro.EstadoActual(Clave).Cantidad);
    }

    [Fact]
    public void KardexReconstruidoCoincideConExistencia()
    {
        var movimientos = new[]
        {
            Entrada(new DateOnly(2026, 1, 1), 1, 10, 100m),
            Salida(new DateOnly(2026, 1, 2), 1, 3),
            Entrada(new DateOnly(2026, 1, 3), 1, 5, 120m)
        };
        var (estado, _) = RecosteadorInventario.Recostear(movimientos, Estricta);
        var kardex = ReconstructorKardex.ConstruirDesdePendientes(movimientos, Estricta);
        var ultima = kardex.Last();
        Assert.Equal(estado.Cantidad, ultima.SaldoCantidad);
        Assert.Equal(estado.ValorTotal, ultima.SaldoValor, precision: 4);
    }
}
