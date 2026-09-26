namespace Neros.Contracts.Integracion;

/// <summary>Contratos de integracion entre modulos (fase 13).</summary>
public static class TiposEventoIntegracion
{
    public const string PedidoConfirmado = "SalesOrderConfirmed.v1";
    public const string ReservaInventarioSolicitada = "InventoryReservationRequested.v1";
    public const string RecepcionCompraRegistrada = "PurchaseReceiptPosted.v1";
    public const string EntradaInventarioRegistrada = "InventoryReceiptPosted.v1";
    public const string ContabilizacionSolicitada = "AccountingPostingRequested.v1";
    public const string NominaLiquidada = "PayrollLiquidationPosted.v1";

    public const string ProductorVentas = "Ventas";
    public const string ProductorCompras = "Compras";
    public const string ProductorInventario = "Inventario";
    public const string ProductorContabilidad = "Contabilidad";

    public const string ConsumidorInventarioReservas = "inventario.reservas";
    public const string ConsumidorInventarioEntradas = "inventario.entradas";
    public const string ConsumidorContabilidad = "contabilidad.posting";
}
