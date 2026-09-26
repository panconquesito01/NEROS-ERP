using Neros.Contracts.Seguridad;

namespace Neros.Application.Operacion;

/// <summary>Permisos por clave normalizada <c>modulo/programa</c> (misma regla que la API de programas).</summary>
public static class PermisosProgramaOperativo
{
    public static string? Consultar(string claveNormalizada) => Permiso(claveNormalizada, consultar: true);

    public static string? Gestionar(string claveNormalizada) => Permiso(claveNormalizada, consultar: false);

    private static string? Permiso(string clave, bool consultar) => clave switch
    {
        "ventas/cotizaciones" => consultar ? CodigosPermiso.VentasCotizacionConsultar : CodigosPermiso.VentasCotizacionGestionar,
        "ventas/pedidos" or "ventas/entregas" or "ventas/devoluciones" or "ventas/listas-precio" =>
            consultar ? CodigosPermiso.VentasPedidoConsultar : CodigosPermiso.VentasPedidoGestionar,
        "ventas/facturacion" => consultar ? CodigosPermiso.FacturacionDocumentoConsultar : CodigosPermiso.FacturacionDocumentoEmitir,
        "ventas/clientes" => consultar ? CodigosPermiso.TerceroConsultar : CodigosPermiso.TerceroCrear,
        "inventarios/productos" or "inventarios/bodegas" or "inventarios/movimientos" or "inventarios/conteos" or "inventarios/periodos" =>
            consultar ? CodigosPermiso.InventarioMovimientoConsultar : CodigosPermiso.InventarioMovimientoRegistrar,
        "inventarios/existencias" => CodigosPermiso.InventarioMovimientoConsultar,
        "compras/solicitudes" or "compras/ordenes" or "compras/devoluciones" =>
            consultar ? CodigosPermiso.ComprasOrdenConsultar : CodigosPermiso.ComprasOrdenGestionar,
        "compras/recepciones" => consultar ? CodigosPermiso.ComprasRecepcionConsultar : CodigosPermiso.ComprasRecepcionRegistrar,
        "compras/proveedores" => consultar ? CodigosPermiso.TerceroConsultar : CodigosPermiso.TerceroCrear,
        "compras/aprobaciones" => consultar ? CodigosPermiso.ComprasOrdenConsultar : CodigosPermiso.ComprasOrdenAprobar,
        "finanzas/cuentas-cobrar" => consultar ? CodigosPermiso.CarteraCxcConsultar : CodigosPermiso.CarteraCxcGestionar,
        "finanzas/cuentas-pagar" => consultar ? CodigosPermiso.CarteraCxpConsultar : CodigosPermiso.CarteraCxpGestionar,
        "finanzas/pagos" => CodigosPermiso.CarteraPagoRegistrar,
        "finanzas/tesoreria-cuentas" => consultar ? CodigosPermiso.TesoreriaCuentaConsultar : CodigosPermiso.TesoreriaCuentaGestionar,
        "finanzas/tesoreria-movimientos" => consultar ? CodigosPermiso.TesoreriaCuentaConsultar : CodigosPermiso.TesoreriaMovimientoRegistrar,
        "finanzas/conciliacion" => consultar ? CodigosPermiso.TesoreriaConciliacionConsultar : CodigosPermiso.TesoreriaConciliacionGestionar,
        "finanzas/comprobantes" => consultar ? CodigosPermiso.ContabilidadComprobanteConsultar : CodigosPermiso.ContabilidadComprobanteContabilizar,
        "finanzas/reportes-contables" => consultar ? CodigosPermiso.ContabilidadReporteConsultar : CodigosPermiso.ContabilidadReporteConfigurar,
        "informes/operativos" or "informes/gerenciales" or "informes/exportaciones" or "informes/indicadores" =>
            consultar ? CodigosPermiso.AnaliticaIndicadorConsultar : CodigosPermiso.AnaliticaIndicadorConfigurar,
        "informes/presupuesto" => CodigosPermiso.PresupuestoEjecucionConsultar,
        "informes/busqueda" => CodigosPermiso.BusquedaIndiceConsultar,
        _ => CodigosPermiso.EmpresaInicioConsultar
    };
}
