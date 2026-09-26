namespace Neros.Domain.Analitica;

public static class TiposHechoAnalitico
{
    public const string PedidoConfirmado = "Ventas.PedidoConfirmado";
    public const string RecepcionCompra = "Compras.RecepcionRegistrada";
    public const string NominaLiquidada = "Nomina.LiquidacionContabilizada";
    public const string SaldoCartera = "Cartera.SaldoDocumento";
}

public static class TiposIndicadorAnalitico
{
    public const string VentasNetas = "VentasNetas";
    public const string ComprasRecibidas = "ComprasRecibidas";
    public const string NominaNeta = "NominaNeta";
}
