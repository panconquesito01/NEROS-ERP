namespace Neros.Contracts.Seguridad;

/// <summary>Codigos <c>MODULO.RECURSO.ACCION</c> compartidos entre API y BFF. La asignacion vive en la aplicacion.</summary>
public static class CodigosPermiso
{
    public const string Claim = "permiso";

    public const string UsuarioConsultar = "PLATAFORMA.USUARIO.CONSULTAR";
    public const string UsuarioRestablecerClave = "PLATAFORMA.USUARIO.RESTABLECER_CLAVE";
    public const string UsuarioDesbloquear = "PLATAFORMA.USUARIO.DESBLOQUEAR";
    public const string UsuarioCrear = "PLATAFORMA.USUARIO.CREAR";
    public const string UsuarioModificar = "PLATAFORMA.USUARIO.MODIFICAR";
    public const string UsuarioEliminar = "PLATAFORMA.USUARIO.ELIMINAR";
    public const string PlataformaEmpresaConsultar = "PLATAFORMA.EMPRESA.CONSULTAR";
    public const string PlataformaEmpresaCrear = "PLATAFORMA.EMPRESA.CREAR";
    public const string PlataformaEmpresaModificar = "PLATAFORMA.EMPRESA.MODIFICAR";

    public const string EmpresaInicioConsultar = "EMPRESA.INICIO.CONSULTAR";
    public const string EmpresaActividadConsultar = "EMPRESA.ACTIVIDAD.CONSULTAR";
    public const string EmpresaMiembroAdministrar = "EMPRESA.MIEMBRO.ADMINISTRAR";
    public const string EmpresaConfiguracionConsultar = "EMPRESA.CONFIGURACION.CONSULTAR";
    public const string EmpresaConfiguracionModificar = "EMPRESA.CONFIGURACION.MODIFICAR";

    public const string TerceroConsultar = "TERCEROS.TERCERO.CONSULTAR";
    public const string TerceroCrear = "TERCEROS.TERCERO.CREAR";
    public const string TerceroModificar = "TERCEROS.TERCERO.MODIFICAR";
    public const string TerceroCuentaBancariaConsultar = "TERCEROS.CUENTA_BANCARIA.CONSULTAR";

    public const string ContabilidadComprobanteConsultar = "CONTABILIDAD.COMPROBANTE.CONSULTAR";
    public const string ContabilidadComprobanteContabilizar = "CONTABILIDAD.COMPROBANTE.CONTABILIZAR";
    public const string ContabilidadPeriodoReabrir = "CONTABILIDAD.PERIODO.REABRIR";
    public const string ContabilidadReporteConsultar = "CONTABILIDAD.REPORTE.CONSULTAR";
    public const string ContabilidadReporteConfigurar = "CONTABILIDAD.REPORTE.CONFIGURAR";

    public const string InventarioMovimientoConsultar = "INVENTARIO.MOVIMIENTO.CONSULTAR";
    public const string InventarioMovimientoRegistrar = "INVENTARIO.MOVIMIENTO.REGISTRAR";

    public const string VentasCotizacionConsultar = "VENTAS.COTIZACION.CONSULTAR";
    public const string VentasCotizacionGestionar = "VENTAS.COTIZACION.GESTIONAR";
    public const string VentasPedidoConsultar = "VENTAS.PEDIDO.CONSULTAR";
    public const string VentasPedidoGestionar = "VENTAS.PEDIDO.GESTIONAR";

    public const string ComprasOrdenConsultar = "COMPRAS.ORDEN.CONSULTAR";
    public const string ComprasOrdenGestionar = "COMPRAS.ORDEN.GESTIONAR";
    public const string ComprasOrdenAprobar = "COMPRAS.ORDEN.APROBAR";
    public const string ComprasRecepcionConsultar = "COMPRAS.RECEPCION.CONSULTAR";
    public const string ComprasRecepcionRegistrar = "COMPRAS.RECEPCION.REGISTRAR";

    public const string CarteraCxcConsultar = "CARTERA.CXC.CONSULTAR";
    public const string CarteraCxcGestionar = "CARTERA.CXC.GESTIONAR";
    public const string CarteraCxpConsultar = "CARTERA.CXP.CONSULTAR";
    public const string CarteraCxpGestionar = "CARTERA.CXP.GESTIONAR";
    public const string CarteraPagoRegistrar = "CARTERA.PAGO.REGISTRAR";

    public const string TesoreriaCuentaConsultar = "TESORERIA.CUENTA.CONSULTAR";
    public const string TesoreriaCuentaGestionar = "TESORERIA.CUENTA.GESTIONAR";
    public const string TesoreriaMovimientoRegistrar = "TESORERIA.MOVIMIENTO.REGISTRAR";
    public const string TesoreriaConciliacionConsultar = "TESORERIA.CONCILIACION.CONSULTAR";
    public const string TesoreriaConciliacionGestionar = "TESORERIA.CONCILIACION.GESTIONAR";

    public const string FacturacionDocumentoConsultar = "FACTURACION.DOCUMENTO.CONSULTAR";
    public const string FacturacionDocumentoEmitir = "FACTURACION.DOCUMENTO.EMITIR";
    public const string FacturacionDocumentoAnular = "FACTURACION.DOCUMENTO.ANULAR";
    public const string FacturacionNumeracionConfigurar = "FACTURACION.NUMERACION.CONFIGURAR";
    public const string FacturacionElectronicoConsultar = "FACTURACION.ELECTRONICO.CONSULTAR";

    public const string NominaEmpleadoConsultar = "NOMINA.EMPLEADO.CONSULTAR";
    public const string NominaEmpleadoGestionar = "NOMINA.EMPLEADO.GESTIONAR";
    public const string NominaConceptoConsultar = "NOMINA.CONCEPTO.CONSULTAR";
    public const string NominaConceptoConfigurar = "NOMINA.CONCEPTO.CONFIGURAR";
    public const string NominaLiquidacionConsultar = "NOMINA.LIQUIDACION.CONSULTAR";
    public const string NominaLiquidacionCalcular = "NOMINA.LIQUIDACION.CALCULAR";
    public const string NominaLiquidacionContabilizar = "NOMINA.LIQUIDACION.CONTABILIZAR";
    public const string NominaLegalConsultar = "NOMINA.LEGAL.CONSULTAR";
    public const string NominaLegalAprobar = "NOMINA.LEGAL.APROBAR";
    public const string NominaElectronicoConsultar = "NOMINA.ELECTRONICO.CONSULTAR";
    public const string NominaElectronicoTransmitir = "NOMINA.ELECTRONICO.TRANSMITIR";

    public const string ActivosActivoConsultar = "ACTIVOS.ACTIVO.CONSULTAR";
    public const string ActivosActivoGestionar = "ACTIVOS.ACTIVO.GESTIONAR";
    public const string ActivosDepreciacionCalcular = "ACTIVOS.DEPRECIACION.CALCULAR";
    public const string ActivosDepreciacionContabilizar = "ACTIVOS.DEPRECIACION.CONTABILIZAR";

    public const string PresupuestoVersionConsultar = "PRESUPUESTO.VERSION.CONSULTAR";
    public const string PresupuestoVersionGestionar = "PRESUPUESTO.VERSION.GESTIONAR";
    public const string PresupuestoVersionAprobar = "PRESUPUESTO.VERSION.APROBAR";
    public const string PresupuestoEjecucionConsultar = "PRESUPUESTO.EJECUCION.CONSULTAR";

    public const string ProduccionOrdenConsultar = "PRODUCCION.ORDEN.CONSULTAR";
    public const string ProduccionOrdenGestionar = "PRODUCCION.ORDEN.GESTIONAR";
    public const string ProduccionOrdenLiberar = "PRODUCCION.ORDEN.LIBERAR";
    public const string ProduccionMovimientoRegistrar = "PRODUCCION.MOVIMIENTO.REGISTRAR";
    public const string ProduccionListaMaterialesConfigurar = "PRODUCCION.LISTA_MATERIALES.CONFIGURAR";

    public const string ProyectosProyectoConsultar = "PROYECTOS.PROYECTO.CONSULTAR";
    public const string ProyectosProyectoGestionar = "PROYECTOS.PROYECTO.GESTIONAR";
    public const string ProyectosImputacionRegistrar = "PROYECTOS.IMPUTACION.REGISTRAR";

    public const string AnaliticaIndicadorConsultar = "ANALITICA.INDICADOR.CONSULTAR";
    public const string AnaliticaIndicadorConfigurar = "ANALITICA.INDICADOR.CONFIGURAR";
    public const string AnaliticaIngestaMonitorear = "ANALITICA.INGESTA.MONITOREAR";

    public const string BusquedaIndiceConsultar = "BUSQUEDA.INDICE.CONSULTAR";
    public const string BusquedaIndiceMonitorear = "BUSQUEDA.INDICE.MONITOREAR";
    public const string BusquedaIndiceReconstruir = "BUSQUEDA.INDICE.RECONSTRUIR";
}
