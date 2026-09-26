using Neros.Domain.Globalizacion;
using Neros.Domain.Proyectos;
using Xunit;

namespace Neros.Tests;

public sealed class ProyectosTests
{
    private static PoliticaRedondeo PoliticaCop() => CatalogoIso.PoliticaPorDefecto("COP");

    private static ProyectoContexto ProyectoActivo() => new(
        Guid.NewGuid(), EstadoProyecto.Activo, 1_000_000m,
        new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));

    [Fact]
    public void Imputacion_ValidaEstadoYFechas()
    {
        var entrada = new ReferenciaMovimientoProyectoEntrada(
            ModuloOrigenProyecto.Compras, "OrdenCompra", Guid.NewGuid(), 100_000m, new DateOnly(2026, 3, 15));
        MotorImputacionProyecto.ValidarReferencia(ProyectoActivo(), entrada);
        var cerrado = ProyectoActivo() with { Estado = EstadoProyecto.Cerrado };
        Assert.Throws<InvalidOperationException>(() => MotorImputacionProyecto.ValidarReferencia(cerrado, entrada));
    }

    [Fact]
    public void Imputacion_DetectaSuperacionPresupuesto()
    {
        var saldo = MotorImputacionProyecto.CalcularSaldo(ProyectoActivo(), 950_000m, 100_000m);
        Assert.True(saldo.SuperaPresupuesto);
        Assert.Equal(-50_000m, saldo.PresupuestoRestante);
    }

    [Fact]
    public void Seguimiento_VariacionYEjecucion()
    {
        var seg = MotorSeguimientoProyecto.Evaluar(1_000_000m, 800_000m, PoliticaCop());
        Assert.Equal(-200_000m, seg.VariacionPresupuesto);
        Assert.Equal(80m, seg.EjecucionPorcentaje);
    }

    [Fact]
    public void MaquinaEstados_FlujoBasico()
    {
        var estado = MaquinaEstadosProyecto.Activar(EstadoProyecto.Planificado);
        estado = MaquinaEstadosProyecto.Suspender(estado);
        Assert.Equal(EstadoProyecto.Cerrado, MaquinaEstadosProyecto.Cerrar(estado));
    }
}
