namespace Neros.Domain.Contabilidad;

public enum NaturalezaCuenta { Debito, Credito }

public enum TipoCuenta { Activo, Pasivo, Patrimonio, Ingreso, Costo, Gasto, Orden }

public enum EstadoPeriodoContable { Abierto, EnCierre, Cerrado, Reabierto }

public enum EstadoComprobante { Borrador, Contabilizado, Reversado }

public enum LadoMovimiento { Debito, Credito }

public enum RolContable
{
    CuentaCliente,
    CuentaIngreso,
    CuentaIvaGenerado,
    CuentaInventario,
    CuentaIvaDescontable,
    CuentaProveedor,
    CuentaGastoNomina,
    CuentaRetencionNomina,
    CuentaObligacionNomina,
    ResultadoEjercicio
}

public static class TiposContables
{
    public static bool EsCuentaResultado(TipoCuenta tipo) =>
        tipo is TipoCuenta.Ingreso or TipoCuenta.Costo or TipoCuenta.Gasto;

    public static bool EsCuentaOrden(TipoCuenta tipo) => tipo == TipoCuenta.Orden;

    public static NaturalezaCuenta NaturalezaPorDefecto(TipoCuenta tipo) => tipo switch
    {
        TipoCuenta.Activo or TipoCuenta.Costo or TipoCuenta.Gasto => NaturalezaCuenta.Debito,
        TipoCuenta.Pasivo or TipoCuenta.Patrimonio or TipoCuenta.Ingreso => NaturalezaCuenta.Credito,
        TipoCuenta.Orden => NaturalezaCuenta.Debito,
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };
}
