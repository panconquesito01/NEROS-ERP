namespace Neros.Contracts.Seguridad;

/// <summary>Permisos de empresa por rol; compartido entre API de compatibilidad y el puente del gateway.</summary>
public static class PermisosEmpresa
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> PorRol = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
    {
        ["Administrador"] =
        [
            CodigosPermiso.EmpresaInicioConsultar, CodigosPermiso.EmpresaActividadConsultar, CodigosPermiso.EmpresaMiembroAdministrar,
            CodigosPermiso.EmpresaConfiguracionConsultar, CodigosPermiso.EmpresaConfiguracionModificar,
            CodigosPermiso.TerceroConsultar, CodigosPermiso.TerceroCrear, CodigosPermiso.TerceroModificar,
            CodigosPermiso.TerceroCuentaBancariaConsultar,
            CodigosPermiso.ContabilidadComprobanteConsultar, CodigosPermiso.ContabilidadComprobanteContabilizar,
            CodigosPermiso.ContabilidadPeriodoReabrir,
            CodigosPermiso.ContabilidadReporteConsultar, CodigosPermiso.ContabilidadReporteConfigurar,
            CodigosPermiso.InventarioMovimientoConsultar, CodigosPermiso.InventarioMovimientoRegistrar,
            CodigosPermiso.VentasCotizacionConsultar, CodigosPermiso.VentasCotizacionGestionar,
            CodigosPermiso.VentasPedidoConsultar, CodigosPermiso.VentasPedidoGestionar,
            CodigosPermiso.ComprasOrdenConsultar, CodigosPermiso.ComprasOrdenGestionar, CodigosPermiso.ComprasOrdenAprobar,
            CodigosPermiso.ComprasRecepcionConsultar, CodigosPermiso.ComprasRecepcionRegistrar,
            CodigosPermiso.CarteraCxcConsultar, CodigosPermiso.CarteraCxcGestionar,
            CodigosPermiso.CarteraCxpConsultar, CodigosPermiso.CarteraCxpGestionar, CodigosPermiso.CarteraPagoRegistrar,
            CodigosPermiso.TesoreriaCuentaConsultar, CodigosPermiso.TesoreriaCuentaGestionar,
            CodigosPermiso.TesoreriaMovimientoRegistrar, CodigosPermiso.TesoreriaConciliacionConsultar,
            CodigosPermiso.TesoreriaConciliacionGestionar,
            CodigosPermiso.FacturacionDocumentoConsultar, CodigosPermiso.FacturacionDocumentoEmitir,
            CodigosPermiso.FacturacionDocumentoAnular, CodigosPermiso.FacturacionNumeracionConfigurar,
            CodigosPermiso.FacturacionElectronicoConsultar,
            CodigosPermiso.NominaEmpleadoConsultar, CodigosPermiso.NominaEmpleadoGestionar,
            CodigosPermiso.NominaConceptoConsultar, CodigosPermiso.NominaConceptoConfigurar,
            CodigosPermiso.NominaLiquidacionConsultar, CodigosPermiso.NominaLiquidacionCalcular,
            CodigosPermiso.NominaLiquidacionContabilizar,
            CodigosPermiso.NominaLegalConsultar, CodigosPermiso.NominaLegalAprobar,
            CodigosPermiso.NominaElectronicoConsultar, CodigosPermiso.NominaElectronicoTransmitir,
            CodigosPermiso.ActivosActivoConsultar, CodigosPermiso.ActivosActivoGestionar,
            CodigosPermiso.ActivosDepreciacionCalcular, CodigosPermiso.ActivosDepreciacionContabilizar,
            CodigosPermiso.PresupuestoVersionConsultar, CodigosPermiso.PresupuestoVersionGestionar,
            CodigosPermiso.PresupuestoVersionAprobar, CodigosPermiso.PresupuestoEjecucionConsultar,
            CodigosPermiso.ProduccionOrdenConsultar, CodigosPermiso.ProduccionOrdenGestionar,
            CodigosPermiso.ProduccionOrdenLiberar, CodigosPermiso.ProduccionMovimientoRegistrar,
            CodigosPermiso.ProduccionListaMaterialesConfigurar,
            CodigosPermiso.ProyectosProyectoConsultar, CodigosPermiso.ProyectosProyectoGestionar,
            CodigosPermiso.ProyectosImputacionRegistrar,
            CodigosPermiso.AnaliticaIndicadorConsultar, CodigosPermiso.AnaliticaIndicadorConfigurar,
            CodigosPermiso.AnaliticaIngestaMonitorear,
            CodigosPermiso.BusquedaIndiceConsultar, CodigosPermiso.BusquedaIndiceMonitorear,
            CodigosPermiso.BusquedaIndiceReconstruir
        ],
        ["Operador"] =
        [
            CodigosPermiso.EmpresaInicioConsultar, CodigosPermiso.EmpresaActividadConsultar,
            CodigosPermiso.EmpresaConfiguracionConsultar,
            CodigosPermiso.TerceroConsultar, CodigosPermiso.TerceroCrear, CodigosPermiso.TerceroModificar,
            CodigosPermiso.ContabilidadComprobanteConsultar, CodigosPermiso.ContabilidadComprobanteContabilizar,
            CodigosPermiso.ContabilidadReporteConsultar,
            CodigosPermiso.InventarioMovimientoConsultar, CodigosPermiso.InventarioMovimientoRegistrar,
            CodigosPermiso.VentasCotizacionConsultar, CodigosPermiso.VentasCotizacionGestionar,
            CodigosPermiso.VentasPedidoConsultar, CodigosPermiso.VentasPedidoGestionar,
            CodigosPermiso.ComprasOrdenConsultar, CodigosPermiso.ComprasOrdenGestionar,
            CodigosPermiso.ComprasRecepcionConsultar, CodigosPermiso.ComprasRecepcionRegistrar,
            CodigosPermiso.CarteraCxcConsultar, CodigosPermiso.CarteraCxcGestionar,
            CodigosPermiso.CarteraCxpConsultar, CodigosPermiso.CarteraPagoRegistrar,
            CodigosPermiso.TesoreriaCuentaConsultar, CodigosPermiso.TesoreriaMovimientoRegistrar,
            CodigosPermiso.TesoreriaConciliacionConsultar,
            CodigosPermiso.FacturacionDocumentoConsultar, CodigosPermiso.FacturacionDocumentoEmitir,
            CodigosPermiso.FacturacionElectronicoConsultar,
            CodigosPermiso.NominaEmpleadoConsultar, CodigosPermiso.NominaEmpleadoGestionar,
            CodigosPermiso.NominaConceptoConsultar,
            CodigosPermiso.NominaLiquidacionConsultar, CodigosPermiso.NominaLiquidacionCalcular,
            CodigosPermiso.NominaLegalConsultar, CodigosPermiso.NominaElectronicoConsultar,
            CodigosPermiso.NominaElectronicoTransmitir,
            CodigosPermiso.ActivosActivoConsultar, CodigosPermiso.ActivosActivoGestionar,
            CodigosPermiso.ActivosDepreciacionCalcular,
            CodigosPermiso.PresupuestoVersionConsultar, CodigosPermiso.PresupuestoVersionGestionar,
            CodigosPermiso.PresupuestoEjecucionConsultar,
            CodigosPermiso.ProduccionOrdenConsultar, CodigosPermiso.ProduccionOrdenGestionar,
            CodigosPermiso.ProduccionMovimientoRegistrar,
            CodigosPermiso.ProyectosProyectoConsultar, CodigosPermiso.ProyectosProyectoGestionar,
            CodigosPermiso.ProyectosImputacionRegistrar,
            CodigosPermiso.AnaliticaIndicadorConsultar,
            CodigosPermiso.BusquedaIndiceConsultar
        ],
        ["Consulta"] =
        [
            CodigosPermiso.EmpresaInicioConsultar, CodigosPermiso.EmpresaConfiguracionConsultar, CodigosPermiso.TerceroConsultar,
            CodigosPermiso.ContabilidadComprobanteConsultar, CodigosPermiso.ContabilidadReporteConsultar,
            CodigosPermiso.InventarioMovimientoConsultar,
            CodigosPermiso.VentasCotizacionConsultar, CodigosPermiso.VentasPedidoConsultar,
            CodigosPermiso.ComprasOrdenConsultar, CodigosPermiso.ComprasRecepcionConsultar,
            CodigosPermiso.CarteraCxcConsultar, CodigosPermiso.CarteraCxpConsultar,
            CodigosPermiso.TesoreriaCuentaConsultar, CodigosPermiso.TesoreriaConciliacionConsultar,
            CodigosPermiso.FacturacionDocumentoConsultar, CodigosPermiso.FacturacionElectronicoConsultar,
            CodigosPermiso.NominaEmpleadoConsultar, CodigosPermiso.NominaConceptoConsultar,
            CodigosPermiso.NominaLiquidacionConsultar,
            CodigosPermiso.NominaLegalConsultar, CodigosPermiso.NominaElectronicoConsultar,
            CodigosPermiso.ActivosActivoConsultar,
            CodigosPermiso.PresupuestoVersionConsultar, CodigosPermiso.PresupuestoEjecucionConsultar,
            CodigosPermiso.ProduccionOrdenConsultar,
            CodigosPermiso.ProyectosProyectoConsultar,
            CodigosPermiso.AnaliticaIndicadorConsultar,
            CodigosPermiso.BusquedaIndiceConsultar
        ]
    };

    public static IReadOnlyList<string> DeRol(string rol) => PorRol.GetValueOrDefault(rol) ?? [];
}
