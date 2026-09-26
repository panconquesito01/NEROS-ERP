using Neros.Domain.Globalizacion;

namespace Neros.Domain.Activos;

public static class MotorDeterioroActivo
{
    public static ResultadoDeterioro Registrar(
        ActivoFijoParametros activo,
        decimal importeDeterioro,
        PoliticaRedondeo politica)
    {
        MaquinaEstadosActivoFijo.ValidarOperacionDepreciacion(activo.Estado);
        if (importeDeterioro <= 0) throw new ArgumentOutOfRangeException(nameof(importeDeterioro));
        var valorLibros = MotorDepreciacionLineal.ValorEnLibros(activo);
        if (importeDeterioro > valorLibros)
            throw new InvalidOperationException("El deterioro no puede superar el valor en libros.");
        var importe = MotorRedondeo.Aplicar(importeDeterioro, politica.Validada());
        var acumulado = activo.DeterioroAcumulado + importe;
        return new ResultadoDeterioro(importe, acumulado, activo.CostoAdquisicion - activo.DepreciacionAcumulada - acumulado);
    }
}
