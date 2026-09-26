namespace Neros.Domain.Inventario;

/// <summary>Costeo por promedio ponderado (plan §49).</summary>
public static class MotorPromedioPonderado
{
    public static decimal CalcularCostoPromedio(decimal cantidadAnterior, decimal valorAnterior, decimal cantidadEntrada, decimal valorEntrada)
    {
        var cantidadNueva = cantidadAnterior + cantidadEntrada;
        if (cantidadNueva <= 0) return 0;
        return (valorAnterior + valorEntrada) / cantidadNueva;
    }

    public static (EstadoExistencia Estado, MovimientoInventarioProcesado Movimiento) Aplicar(
        EstadoExistencia estado, MovimientoInventarioPendiente movimiento, PoliticaBodega politica) =>
        movimiento.EsEntrada
            ? AplicarEntrada(estado, movimiento)
            : AplicarSalida(estado, movimiento, politica);

    public static (EstadoExistencia Estado, MovimientoInventarioProcesado Movimiento) AplicarEntrada(
        EstadoExistencia estado, MovimientoInventarioPendiente movimiento)
    {
        if (!movimiento.EsEntrada)
            throw new InvalidOperationException("Movimiento no es entrada.");
        var costoUnitario = movimiento.CostoUnitarioEntrada
            ?? throw new InvalidOperationException("La entrada requiere costo unitario.");
        if (costoUnitario < 0) throw new ArgumentOutOfRangeException(nameof(costoUnitario));
        var valorEntrada = movimiento.Cantidad * costoUnitario;
        var costoPromedio = CalcularCostoPromedio(estado.Cantidad, estado.ValorTotal, movimiento.Cantidad, valorEntrada);
        var cantidad = estado.Cantidad + movimiento.Cantidad;
        var valor = estado.ValorTotal + valorEntrada;
        var procesado = new MovimientoInventarioProcesado(
            movimiento.Id, movimiento.Fecha, movimiento.Secuencia, movimiento.Tipo, movimiento.Cantidad,
            costoUnitario, valorEntrada, false);
        return (new EstadoExistencia(cantidad, valor, costoPromedio), procesado);
    }

    public static (EstadoExistencia Estado, MovimientoInventarioProcesado Movimiento) AplicarSalida(
        EstadoExistencia estado, MovimientoInventarioPendiente movimiento, PoliticaBodega politica)
    {
        if (movimiento.EsEntrada)
            throw new InvalidOperationException("Movimiento no es salida.");
        var costoUnitario = estado.CostoPromedio;
        var marcadoAjuste = false;
        if (movimiento.Cantidad > estado.Cantidad)
        {
            if (!politica.PermitirExistenciasNegativas)
                throw new InvalidOperationException("Existencia insuficiente.");
            if (estado.CostoPromedio <= 0 && estado.Cantidad <= 0)
                throw new InvalidOperationException("No hay ultimo costo conocido para salida negativa.");
            marcadoAjuste = true;
        }
        var valorSalida = movimiento.Cantidad * costoUnitario;
        var cantidad = estado.Cantidad - movimiento.Cantidad;
        var valor = estado.ValorTotal - valorSalida;
        var costoPromedio = cantidad > 0 ? valor / cantidad : costoUnitario;
        var procesado = new MovimientoInventarioProcesado(
            movimiento.Id, movimiento.Fecha, movimiento.Secuencia, movimiento.Tipo, movimiento.Cantidad,
            costoUnitario, valorSalida, marcadoAjuste);
        return (new EstadoExistencia(cantidad, valor, costoPromedio), procesado);
    }
}
