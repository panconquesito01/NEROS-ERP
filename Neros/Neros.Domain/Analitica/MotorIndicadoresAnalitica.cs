using Neros.Domain.Globalizacion;

namespace Neros.Domain.Analitica;

public static class MotorIndicadoresAnalitica
{
    public static ValorIndicadorCalculado Calcular(
        DefinicionIndicador definicion,
        DateOnly periodo,
        IReadOnlyList<HechoOperativo> hechos,
        PoliticaRedondeo politica)
    {
        ArgumentNullException.ThrowIfNull(definicion);
        ArgumentNullException.ThrowIfNull(hechos);
        if (!definicion.Activo) throw new InvalidOperationException("Indicador inactivo.");

        var delPeriodo = hechos.Where(h => h.PeriodoNegocio == periodo).ToList();
        var bruto = definicion.TipoIndicador switch
        {
            TiposIndicadorAnalitico.VentasNetas => Sumar(delPeriodo, TiposHechoAnalitico.PedidoConfirmado),
            TiposIndicadorAnalitico.ComprasRecibidas => Sumar(delPeriodo, TiposHechoAnalitico.RecepcionCompra),
            TiposIndicadorAnalitico.NominaNeta => Sumar(delPeriodo, TiposHechoAnalitico.NominaLiquidada),
            _ => throw new InvalidOperationException($"Tipo de indicador no soportado: {definicion.TipoIndicador}.")
        };
        var valor = MotorRedondeo.Aplicar(bruto, politica.Validada());
        return new ValorIndicadorCalculado(definicion.Codigo, periodo, valor);
    }

    public static IReadOnlyList<ValorIndicadorCalculado> CalcularLote(
        IEnumerable<DefinicionIndicador> definiciones,
        DateOnly periodo,
        IReadOnlyList<HechoOperativo> hechos,
        PoliticaRedondeo politica)
        => definiciones.Where(d => d.Activo)
            .Select(d => Calcular(d, periodo, hechos, politica))
            .ToList();

    private static decimal Sumar(IReadOnlyList<HechoOperativo> hechos, string tipoHecho)
        => hechos.Where(h => h.TipoHecho == tipoHecho).Sum(h => h.ImporteMonedaFuncional);
}
