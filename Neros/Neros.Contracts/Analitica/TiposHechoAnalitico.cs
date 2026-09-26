namespace Neros.Contracts.Analitica;

/// <summary>Contratos publicos alineados con <c>Neros.Domain.Analitica</c>.</summary>
public static class TiposHechoAnalitico
{
    public const string PedidoConfirmado = "Ventas.PedidoConfirmado";
    public const string RecepcionCompra = "Compras.RecepcionRegistrada";
    public const string NominaLiquidada = "Nomina.LiquidacionContabilizada";
}

public static class TiposIndicadorAnalitico
{
    public const string VentasNetas = "VentasNetas";
    public const string ComprasRecibidas = "ComprasRecibidas";
    public const string NominaNeta = "NominaNeta";
}
