namespace Neros.Blazor.Components.Shared;

public sealed record ProgramaModulo(
    string ModuloClave,
    string Slug,
    string Icono,
    string PermisoConsultar,
    string? PermisoGestionar,
    bool PermiteCrearBorrador,
    string Titulo,
    string Detalle)
{
    public string Ruta => $"/{ModuloClave.ToLowerInvariant()}/{Slug}";
}

public static class CatalogoProgramasModulo
{
    public static IReadOnlyList<ProgramaModulo> DeModulo(string claveModulo) =>
        PorModulo.TryGetValue(claveModulo, out var modulo) ? modulo.Programas : [];

    public static ProgramaModulo? Resolver(string rutaModulo, string slug) =>
        DeModulo(rutaModulo).FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

    public static string ClaveApi(ProgramaModulo programa) =>
        $"{programa.ModuloClave.ToLowerInvariant()}-{programa.Slug}";

    private static readonly ProgramaModulo[] ProgramasVentas =
    [
        Prog("Ventas", "cotizaciones", "receipt-text", "VENTAS.COTIZACION.CONSULTAR", "VENTAS.COTIZACION.GESTIONAR", true, "Cotizaciones", "Propuestas comerciales"),
        Prog("Ventas", "pedidos", "package", "VENTAS.PEDIDO.CONSULTAR", "VENTAS.PEDIDO.GESTIONAR", true, "Pedidos", "Órdenes de venta"),
        Prog("Ventas", "entregas", "arrow-right", "VENTAS.PEDIDO.CONSULTAR", "VENTAS.PEDIDO.GESTIONAR", false, "Entregas", "Despachos y cumplimiento"),
        Prog("Ventas", "devoluciones", "refresh-cw", "VENTAS.PEDIDO.CONSULTAR", "VENTAS.PEDIDO.GESTIONAR", false, "Devoluciones", "Notas crédito y devoluciones"),
        Prog("Ventas", "facturacion", "wallet", "FACTURACION.DOCUMENTO.CONSULTAR", "FACTURACION.DOCUMENTO.EMITIR", true, "Facturación", "Documentos de cobro"),
        Prog("Ventas", "clientes", "users-round", "TERCEROS.TERCERO.CONSULTAR", "TERCEROS.TERCERO.CREAR", true, "Clientes", "Terceros con rol cliente"),
        Prog("Ventas", "listas-precio", "layers-2", "VENTAS.COTIZACION.CONSULTAR", "VENTAS.COTIZACION.GESTIONAR", true, "Listas de precio", "Catálogo de productos como base de precios")
    ];

    private static readonly ProgramaModulo[] ProgramasInventarios =
    [
        Prog("Inventarios", "productos", "package", "INVENTARIO.MOVIMIENTO.CONSULTAR", "INVENTARIO.MOVIMIENTO.REGISTRAR", true, "Productos", "Catálogo de productos"),
        Prog("Inventarios", "bodegas", "building-2", "INVENTARIO.MOVIMIENTO.CONSULTAR", "INVENTARIO.MOVIMIENTO.REGISTRAR", true, "Bodegas", "Ubicaciones de almacenamiento"),
        Prog("Inventarios", "movimientos", "arrow-left-right", "INVENTARIO.MOVIMIENTO.CONSULTAR", "INVENTARIO.MOVIMIENTO.REGISTRAR", true, "Movimientos", "Entradas, salidas y traslados"),
        Prog("Inventarios", "existencias", "layers-2", "INVENTARIO.MOVIMIENTO.CONSULTAR", null, false, "Existencias", "Saldo por producto y bodega"),
        Prog("Inventarios", "conteos", "search", "INVENTARIO.MOVIMIENTO.CONSULTAR", "INVENTARIO.MOVIMIENTO.REGISTRAR", true, "Conteos", "Periodos abiertos para conteo"),
        Prog("Inventarios", "periodos", "history", "INVENTARIO.MOVIMIENTO.CONSULTAR", "INVENTARIO.MOVIMIENTO.REGISTRAR", true, "Periodos", "Cierre de periodos de inventario")
    ];

    private static readonly ProgramaModulo[] ProgramasCompras =
    [
        Prog("Compras", "solicitudes", "layers-2", "COMPRAS.ORDEN.CONSULTAR", "COMPRAS.ORDEN.GESTIONAR", false, "Solicitudes", "Requisiciones internas"),
        Prog("Compras", "ordenes", "package", "COMPRAS.ORDEN.CONSULTAR", "COMPRAS.ORDEN.GESTIONAR", true, "Órdenes de compra", "Pedidos a proveedor"),
        Prog("Compras", "recepciones", "arrow-down", "COMPRAS.RECEPCION.CONSULTAR", "COMPRAS.RECEPCION.REGISTRAR", true, "Recepciones", "Ingreso de mercancía"),
        Prog("Compras", "devoluciones", "refresh-cw", "COMPRAS.RECEPCION.CONSULTAR", "COMPRAS.RECEPCION.REGISTRAR", false, "Devoluciones", "Devoluciones a proveedor"),
        Prog("Compras", "proveedores", "users-round", "TERCEROS.TERCERO.CONSULTAR", "TERCEROS.TERCERO.CREAR", true, "Proveedores", "Terceros con rol proveedor"),
        Prog("Compras", "aprobaciones", "shield-check", "COMPRAS.ORDEN.CONSULTAR", "COMPRAS.ORDEN.APROBAR", false, "Aprobaciones", "Órdenes pendientes de aprobación")
    ];

    private static readonly ProgramaModulo[] ProgramasFinanzas =
    [
        Prog("Finanzas", "cuentas-cobrar", "wallet", "CARTERA.CXC.CONSULTAR", "CARTERA.CXC.GESTIONAR", true, "Cuentas por cobrar", "Cartera de clientes"),
        Prog("Finanzas", "cuentas-pagar", "wallet", "CARTERA.CXP.CONSULTAR", "CARTERA.CXP.GESTIONAR", true, "Cuentas por pagar", "Obligaciones con proveedores"),
        Prog("Finanzas", "pagos", "arrow-left-right", "CARTERA.PAGO.REGISTRAR", "CARTERA.PAGO.REGISTRAR", true, "Pagos", "Registro de pagos y aplicaciones"),
        Prog("Finanzas", "tesoreria-cuentas", "building-2", "TESORERIA.CUENTA.CONSULTAR", "TESORERIA.CUENTA.GESTIONAR", true, "Cuentas tesorería", "Caja y bancos"),
        Prog("Finanzas", "tesoreria-movimientos", "arrow-left-right", "TESORERIA.CUENTA.CONSULTAR", "TESORERIA.MOVIMIENTO.REGISTRAR", false, "Movimientos tesorería", "Entradas y salidas de efectivo"),
        Prog("Finanzas", "conciliacion", "search", "TESORERIA.CONCILIACION.CONSULTAR", "TESORERIA.CONCILIACION.GESTIONAR", false, "Conciliación", "Extractos vs movimientos"),
        Prog("Finanzas", "comprobantes", "layers-2", "CONTABILIDAD.COMPROBANTE.CONSULTAR", "CONTABILIDAD.COMPROBANTE.CONTABILIZAR", true, "Comprobantes", "Asientos contables"),
        Prog("Finanzas", "reportes-contables", "chart-no-axes-combined", "CONTABILIDAD.REPORTE.CONSULTAR", "CONTABILIDAD.REPORTE.CONFIGURAR", false, "Reportes contables", "Balance, resultados y auxiliares")
    ];

    private static readonly ProgramaModulo[] ProgramasInformes =
    [
        Prog("Informes", "operativos", "chart-no-axes-combined", "ANALITICA.INDICADOR.CONSULTAR", null, false, "Informes operativos", "Ventas, compras e inventario"),
        Prog("Informes", "gerenciales", "chart-no-axes-combined", "ANALITICA.INDICADOR.CONSULTAR", "ANALITICA.INDICADOR.CONFIGURAR", false, "Informes gerenciales", "Margen, rotación y tendencias"),
        Prog("Informes", "presupuesto", "layers-2", "PRESUPUESTO.EJECUCION.CONSULTAR", null, false, "Ejecución presupuesto", "Real vs presupuestado"),
        Prog("Informes", "busqueda", "search", "BUSQUEDA.INDICE.CONSULTAR", null, false, "Búsqueda global", "Vista 360° indexada"),
        Prog("Informes", "indicadores", "history", "ANALITICA.INDICADOR.CONSULTAR", "ANALITICA.INDICADOR.CONFIGURAR", false, "Indicadores", "KPI configurables"),
        Prog("Informes", "exportaciones", "arrow-down", "ANALITICA.INDICADOR.CONSULTAR", null, false, "Exportaciones", "Excel, PDF y programación")
    ];

    private static ProgramaModulo Prog(string modulo, string slug, string icono, string consultar, string? gestionar, bool crear, string titulo, string detalle) =>
        new(modulo, slug, icono, consultar, gestionar, crear, titulo, detalle);

    private sealed record ModuloProgramas(string Clave, string Icono, ProgramaModulo[] Programas);

    private static readonly Dictionary<string, ModuloProgramas> PorModulo = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Ventas"] = new("Ventas", "receipt-text", ProgramasVentas),
        ["Inventarios"] = new("Inventarios", "package", ProgramasInventarios),
        ["Compras"] = new("Compras", "arrow-left-right", ProgramasCompras),
        ["Finanzas"] = new("Finanzas", "wallet", ProgramasFinanzas),
        ["Informes"] = new("Informes", "chart-no-axes-combined", ProgramasInformes)
    };
}
