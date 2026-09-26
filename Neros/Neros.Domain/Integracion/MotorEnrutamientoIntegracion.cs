namespace Neros.Domain.Integracion;

/// <summary>Resuelve consumidor persistente por tipo de evento (plan §55, fase 24).</summary>
public static class MotorEnrutamientoIntegracion
{
    public const string TipoPedidoConfirmado = "SalesOrderConfirmed.v1";
    public const string TipoReservaSolicitada = "InventoryReservationRequested.v1";
    public const string TipoRecepcionCompra = "PurchaseReceiptPosted.v1";
    public const string TipoEntradaInventario = "InventoryReceiptPosted.v1";
    public const string TipoContabilizacionSolicitada = "AccountingPostingRequested.v1";
    public const string TipoNominaLiquidada = "PayrollLiquidationPosted.v1";

    public const string ConsumidorInventarioReservas = "inventario.reservas";
    public const string ConsumidorInventarioEntradas = "inventario.entradas";
    public const string ConsumidorContabilidad = "contabilidad.posting";
    public const string ConsumidorBusqueda = "busqueda.indexacion";

    public static bool EsEventoIndexable(string tipoEvento) =>
        tipoEvento == TipoPedidoConfirmado;

    public static string? ConsumidorParaTipo(string tipoEvento) => tipoEvento switch
    {
        TipoPedidoConfirmado => ConsumidorInventarioReservas,
        TipoReservaSolicitada => ConsumidorInventarioReservas,
        TipoRecepcionCompra or TipoEntradaInventario => ConsumidorInventarioEntradas,
        TipoContabilizacionSolicitada or TipoNominaLiquidada => ConsumidorContabilidad,
        _ => null
    };

    public static IReadOnlyList<string> ConsumidoresParaTipo(string tipoEvento)
    {
        if (tipoEvento == TipoRecepcionCompra)
            return [ConsumidorInventarioEntradas, ConsumidorContabilidad];
        var unico = ConsumidorParaTipo(tipoEvento);
        return unico is null ? [] : [unico];
    }
}
