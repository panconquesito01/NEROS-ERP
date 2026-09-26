using System.Security.Claims;
using Neros.Contracts.Seguridad;

namespace Neros.Application.Seguridad;

/// <summary>Catalogo inicial de permisos <c>MODULO.RECURSO.ACCION</c> (plan maestro 18).</summary>
public static class Permisos
{
    public const string Claim = CodigosPermiso.Claim;
    public const string RolAdministradorGlobal = "AdministradorGlobal";

    public const string UsuarioConsultar = CodigosPermiso.UsuarioConsultar;
    public const string UsuarioRestablecerClave = CodigosPermiso.UsuarioRestablecerClave;
    public const string UsuarioDesbloquear = CodigosPermiso.UsuarioDesbloquear;
    public const string UsuarioCrear = CodigosPermiso.UsuarioCrear;
    public const string UsuarioModificar = CodigosPermiso.UsuarioModificar;
    public const string UsuarioEliminar = CodigosPermiso.UsuarioEliminar;
    public const string PlataformaEmpresaConsultar = CodigosPermiso.PlataformaEmpresaConsultar;
    public const string PlataformaEmpresaCrear = CodigosPermiso.PlataformaEmpresaCrear;
    public const string PlataformaEmpresaModificar = CodigosPermiso.PlataformaEmpresaModificar;

    public const string EmpresaInicioConsultar = CodigosPermiso.EmpresaInicioConsultar;
    public const string EmpresaActividadConsultar = CodigosPermiso.EmpresaActividadConsultar;
    public const string EmpresaMiembroAdministrar = CodigosPermiso.EmpresaMiembroAdministrar;
    public const string EmpresaConfiguracionConsultar = CodigosPermiso.EmpresaConfiguracionConsultar;
    public const string EmpresaConfiguracionModificar = CodigosPermiso.EmpresaConfiguracionModificar;

    /// <summary>Conserva identidad y permisos de plataforma; aplica rol de la empresa activa.</summary>
    public static List<Claim> ActualizarClaimsEmpresa(IEnumerable<Claim> actuales, string? rolEmpresa)
    {
        var lista = actuales.Where(c => c.Type != Claim).ToList();
        foreach (var permiso in Plataforma)
        {
            if (actuales.Any(c => c.Type == Claim && c.Value == permiso))
                lista.Add(new Claim(Claim, permiso));
        }
        if (!string.IsNullOrWhiteSpace(rolEmpresa))
        {
            var rolEfectivo = rolEmpresa == "Administrador global" ? "Administrador" : rolEmpresa;
            foreach (var permiso in DeRolEmpresa(rolEfectivo))
                lista.Add(new Claim(Claim, permiso));
        }
        return lista;
    }

    public const string TerceroConsultar = CodigosPermiso.TerceroConsultar;
    public const string TerceroCrear = CodigosPermiso.TerceroCrear;
    public const string TerceroModificar = CodigosPermiso.TerceroModificar;
    public const string TerceroCuentaBancariaConsultar = CodigosPermiso.TerceroCuentaBancariaConsultar;

    public const string ContabilidadComprobanteConsultar = CodigosPermiso.ContabilidadComprobanteConsultar;
    public const string ContabilidadComprobanteContabilizar = CodigosPermiso.ContabilidadComprobanteContabilizar;
    public const string ContabilidadPeriodoReabrir = CodigosPermiso.ContabilidadPeriodoReabrir;
    public const string ContabilidadReporteConsultar = CodigosPermiso.ContabilidadReporteConsultar;
    public const string ContabilidadReporteConfigurar = CodigosPermiso.ContabilidadReporteConfigurar;

    public const string InventarioMovimientoConsultar = CodigosPermiso.InventarioMovimientoConsultar;
    public const string InventarioMovimientoRegistrar = CodigosPermiso.InventarioMovimientoRegistrar;

    public const string VentasCotizacionConsultar = CodigosPermiso.VentasCotizacionConsultar;
    public const string VentasCotizacionGestionar = CodigosPermiso.VentasCotizacionGestionar;
    public const string VentasPedidoConsultar = CodigosPermiso.VentasPedidoConsultar;
    public const string VentasPedidoGestionar = CodigosPermiso.VentasPedidoGestionar;

    public const string ComprasOrdenConsultar = CodigosPermiso.ComprasOrdenConsultar;
    public const string ComprasOrdenGestionar = CodigosPermiso.ComprasOrdenGestionar;
    public const string ComprasOrdenAprobar = CodigosPermiso.ComprasOrdenAprobar;
    public const string ComprasRecepcionConsultar = CodigosPermiso.ComprasRecepcionConsultar;
    public const string ComprasRecepcionRegistrar = CodigosPermiso.ComprasRecepcionRegistrar;

    public const string CarteraCxcConsultar = CodigosPermiso.CarteraCxcConsultar;
    public const string CarteraCxcGestionar = CodigosPermiso.CarteraCxcGestionar;
    public const string CarteraCxpConsultar = CodigosPermiso.CarteraCxpConsultar;
    public const string CarteraCxpGestionar = CodigosPermiso.CarteraCxpGestionar;
    public const string CarteraPagoRegistrar = CodigosPermiso.CarteraPagoRegistrar;

    public const string TesoreriaCuentaConsultar = CodigosPermiso.TesoreriaCuentaConsultar;
    public const string TesoreriaCuentaGestionar = CodigosPermiso.TesoreriaCuentaGestionar;
    public const string TesoreriaMovimientoRegistrar = CodigosPermiso.TesoreriaMovimientoRegistrar;
    public const string TesoreriaConciliacionConsultar = CodigosPermiso.TesoreriaConciliacionConsultar;
    public const string TesoreriaConciliacionGestionar = CodigosPermiso.TesoreriaConciliacionGestionar;

    public const string FacturacionDocumentoConsultar = CodigosPermiso.FacturacionDocumentoConsultar;
    public const string FacturacionDocumentoEmitir = CodigosPermiso.FacturacionDocumentoEmitir;
    public const string FacturacionDocumentoAnular = CodigosPermiso.FacturacionDocumentoAnular;
    public const string FacturacionNumeracionConfigurar = CodigosPermiso.FacturacionNumeracionConfigurar;
    public const string FacturacionElectronicoConsultar = CodigosPermiso.FacturacionElectronicoConsultar;

    public const string NominaEmpleadoConsultar = CodigosPermiso.NominaEmpleadoConsultar;
    public const string NominaEmpleadoGestionar = CodigosPermiso.NominaEmpleadoGestionar;
    public const string NominaConceptoConsultar = CodigosPermiso.NominaConceptoConsultar;
    public const string NominaConceptoConfigurar = CodigosPermiso.NominaConceptoConfigurar;
    public const string NominaLiquidacionConsultar = CodigosPermiso.NominaLiquidacionConsultar;
    public const string NominaLiquidacionCalcular = CodigosPermiso.NominaLiquidacionCalcular;
    public const string NominaLiquidacionContabilizar = CodigosPermiso.NominaLiquidacionContabilizar;
    public const string NominaLegalConsultar = CodigosPermiso.NominaLegalConsultar;
    public const string NominaLegalAprobar = CodigosPermiso.NominaLegalAprobar;
    public const string NominaElectronicoConsultar = CodigosPermiso.NominaElectronicoConsultar;
    public const string NominaElectronicoTransmitir = CodigosPermiso.NominaElectronicoTransmitir;

    public const string ActivosActivoConsultar = CodigosPermiso.ActivosActivoConsultar;
    public const string ActivosActivoGestionar = CodigosPermiso.ActivosActivoGestionar;
    public const string ActivosDepreciacionCalcular = CodigosPermiso.ActivosDepreciacionCalcular;
    public const string ActivosDepreciacionContabilizar = CodigosPermiso.ActivosDepreciacionContabilizar;

    public const string PresupuestoVersionConsultar = CodigosPermiso.PresupuestoVersionConsultar;
    public const string PresupuestoVersionGestionar = CodigosPermiso.PresupuestoVersionGestionar;
    public const string PresupuestoVersionAprobar = CodigosPermiso.PresupuestoVersionAprobar;
    public const string PresupuestoEjecucionConsultar = CodigosPermiso.PresupuestoEjecucionConsultar;

    public const string ProduccionOrdenConsultar = CodigosPermiso.ProduccionOrdenConsultar;
    public const string ProduccionOrdenGestionar = CodigosPermiso.ProduccionOrdenGestionar;
    public const string ProduccionOrdenLiberar = CodigosPermiso.ProduccionOrdenLiberar;
    public const string ProduccionMovimientoRegistrar = CodigosPermiso.ProduccionMovimientoRegistrar;
    public const string ProduccionListaMaterialesConfigurar = CodigosPermiso.ProduccionListaMaterialesConfigurar;

    public const string ProyectosProyectoConsultar = CodigosPermiso.ProyectosProyectoConsultar;
    public const string ProyectosProyectoGestionar = CodigosPermiso.ProyectosProyectoGestionar;
    public const string ProyectosImputacionRegistrar = CodigosPermiso.ProyectosImputacionRegistrar;

    public const string AnaliticaIndicadorConsultar = CodigosPermiso.AnaliticaIndicadorConsultar;
    public const string AnaliticaIndicadorConfigurar = CodigosPermiso.AnaliticaIndicadorConfigurar;
    public const string AnaliticaIngestaMonitorear = CodigosPermiso.AnaliticaIngestaMonitorear;

    public const string BusquedaIndiceConsultar = CodigosPermiso.BusquedaIndiceConsultar;
    public const string BusquedaIndiceMonitorear = CodigosPermiso.BusquedaIndiceMonitorear;
    public const string BusquedaIndiceReconstruir = CodigosPermiso.BusquedaIndiceReconstruir;

    public static IReadOnlyList<string> Plataforma { get; } =
    [
        UsuarioConsultar, UsuarioRestablecerClave, UsuarioDesbloquear, UsuarioCrear, UsuarioModificar, UsuarioEliminar,
        CodigosPermiso.PlataformaEmpresaConsultar, CodigosPermiso.PlataformaEmpresaCrear, CodigosPermiso.PlataformaEmpresaModificar
    ];

    public static IEnumerable<string> Todos => Plataforma.Concat(PermisosEmpresa.DeRol("Administrador")
        .Concat(PermisosEmpresa.DeRol("Operador")).Concat(PermisosEmpresa.DeRol("Consulta"))).Distinct();

    /// <summary>Permisos de plataforma; solo el administrador global los recibe.</summary>
    public static IReadOnlyList<string> DePlataforma(bool administradorGlobal) => administradorGlobal ? Plataforma : [];

    /// <summary>Permisos del rol dentro de una empresa. Un rol desconocido no concede nada.</summary>
    public static IReadOnlyList<string> DeRolEmpresa(string rol) => PermisosEmpresa.DeRol(rol);
}
