using Neros.Domain.Globalizacion;

namespace Neros.Domain.Activos;

/// <summary>Depreciacion lineal: (Costo - ValorResidual) / VidaUtil por periodo (plan §54).</summary>
public static class MotorDepreciacionLineal
{
    public static decimal BaseDepreciable(decimal costo, decimal valorResidual)
    {
        if (costo < 0 || valorResidual < 0 || valorResidual > costo)
            throw new ArgumentOutOfRangeException(nameof(valorResidual));
        return costo - valorResidual;
    }

    public static decimal CuotaMensualBruta(decimal costo, decimal valorResidual, int vidaUtilMeses)
    {
        if (vidaUtilMeses <= 0) throw new ArgumentOutOfRangeException(nameof(vidaUtilMeses));
        return BaseDepreciable(costo, valorResidual) / vidaUtilMeses;
    }

    public static ResultadoCuotaDepreciacion CalcularCuotaPeriodo(
        ActivoFijoParametros activo,
        int mesesDepreciadosAntes,
        PoliticaRedondeo politica)
    {
        if (activo.Estado != EstadoActivoFijo.Activo)
            throw new InvalidOperationException("Solo un activo en servicio deprecia.");
        if (activo.Metodo != MetodoDepreciacionActivo.Lineal)
            throw new NotSupportedException("Metodo de depreciacion no implementado.");

        var baseDep = BaseDepreciable(activo.CostoAdquisicion, activo.ValorResidual);
        var cuotaBruta = CuotaMensualBruta(activo.CostoAdquisicion, activo.ValorResidual, activo.VidaUtilMeses);
        var politicaValidada = politica.Validada();
        var cuota = MotorRedondeo.Aplicar(cuotaBruta, politicaValidada);

        var acumuladaAnterior = activo.DepreciacionAcumulada;
        var restante = baseDep - acumuladaAnterior;
        if (restante <= 0)
            return new ResultadoCuotaDepreciacion(0m, acumuladaAnterior, ValorEnLibros(activo), true);

        var importe = mesesDepreciadosAntes + 1 >= activo.VidaUtilMeses || cuota > restante
            ? restante
            : cuota;
        importe = MotorRedondeo.Aplicar(importe, politicaValidada);
        var acumulada = acumuladaAnterior + importe;
        var valorLibros = activo.CostoAdquisicion - acumulada - activo.DeterioroAcumulado;
        var ultimo = acumulada >= baseDep || mesesDepreciadosAntes + 1 >= activo.VidaUtilMeses;
        return new ResultadoCuotaDepreciacion(importe, acumulada, valorLibros, ultimo);
    }

    public static decimal ValorEnLibros(ActivoFijoParametros activo) =>
        activo.CostoAdquisicion - activo.DepreciacionAcumulada - activo.DeterioroAcumulado;
}
