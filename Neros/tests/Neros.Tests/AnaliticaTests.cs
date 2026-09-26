using Neros.Domain.Analitica;
using Neros.Domain.Globalizacion;
using Neros.Domain.Integracion;
using Xunit;

namespace Neros.Tests;

public sealed class AnaliticaTests
{
    private static PoliticaRedondeo PoliticaCop() => CatalogoIso.PoliticaPorDefecto("COP");

    [Fact]
    public void Ingesta_RechazaEventoTardioFueraDeTolerancia()
    {
        var tenant = Guid.NewGuid();
        var empresa = Guid.NewGuid();
        var marca = new MarcaAguaIngesta(tenant, empresa, "Ventas", DateTime.UtcNow, Guid.NewGuid());
        var evento = Evento(tenant, empresa, DateTimeOffset.UtcNow.AddDays(-30));
        var (resultado, _, _, _) = OrquestadorIngestaAnalitica.Ejecutar(new HashSet<Guid>(), marca, evento);
        Assert.Equal(ResultadoIngesta.RechazadoTardio, resultado);
    }

    [Fact]
    public void Ingesta_AceptaTardioDentroDeToleranciaYProyectaHecho()
    {
        var tenant = Guid.NewGuid();
        var empresa = Guid.NewGuid();
        var ahora = DateTime.UtcNow;
        var marca = new MarcaAguaIngesta(tenant, empresa, "Ventas", ahora, Guid.NewGuid());
        var evento = Evento(tenant, empresa, new DateTimeOffset(ahora.AddDays(-2)), totalNeto: 1_500_000m);
        var (resultado, _, registro, hechos) = OrquestadorIngestaAnalitica.Ejecutar(new HashSet<Guid>(), marca, evento);
        Assert.Equal(ResultadoIngesta.Ingerido, resultado);
        Assert.NotNull(registro);
        Assert.True(registro!.EsTardio);
        Assert.Single(hechos);
        Assert.Equal(TiposHechoAnalitico.PedidoConfirmado, hechos[0].TipoHecho);
    }

    [Fact]
    public void Ingesta_IgnoraDuplicadoPorMessageId()
    {
        var tenant = Guid.NewGuid();
        var empresa = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var evento = Evento(tenant, empresa, DateTimeOffset.UtcNow, messageId);
        var ids = new HashSet<Guid> { messageId };
        var (resultado, _, _, _) = OrquestadorIngestaAnalitica.Ejecutar(ids, null, evento);
        Assert.Equal(ResultadoIngesta.DuplicadoIgnorado, resultado);
    }

    [Fact]
    public void Indicador_VentasNetasSumaHechosDelPeriodo()
    {
        var periodo = new DateOnly(2026, 9, 1);
        var hechos = new List<HechoOperativo>
        {
            Hecho(periodo, TiposHechoAnalitico.PedidoConfirmado, 100m),
            Hecho(periodo, TiposHechoAnalitico.PedidoConfirmado, 250m),
            Hecho(periodo, TiposHechoAnalitico.RecepcionCompra, 999m)
        };
        var definicion = new DefinicionIndicador(Guid.NewGuid(), "VENTAS_MES", TiposIndicadorAnalitico.VentasNetas, true);
        var valor = MotorIndicadoresAnalitica.Calcular(definicion, periodo, hechos, PoliticaCop());
        Assert.Equal(350m, valor.Valor);
    }

    private static EventoParaIngesta Evento(
        Guid tenant, Guid empresa, DateTimeOffset ocurrio, Guid? messageId = null, decimal? totalNeto = null)
    {
        var pedidoId = Guid.NewGuid();
        var payload = $$"""
            {"pedidoId":"{{pedidoId}}","totalNeto":{{totalNeto ?? 0}},"lineas":[{"cantidad":2}]}
            """;
        return new EventoParaIngesta(
            messageId ?? Guid.NewGuid(), tenant, empresa, "Ventas",
            MotorEnrutamientoIntegracion.TipoPedidoConfirmado, pedidoId.ToString(), 1,
            ocurrio, Guid.NewGuid(), null, payload);
    }

    private static HechoOperativo Hecho(DateOnly periodo, string tipo, decimal importe) =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), tipo, periodo, importe, null, null, Guid.NewGuid());
}
