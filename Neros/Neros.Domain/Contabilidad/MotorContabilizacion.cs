using Neros.Domain.Globalizacion;

namespace Neros.Domain.Contabilidad;

/// <summary>Generacion de movimientos desde eventos de negocio (plan §37). Sin numeros de cuenta en codigo.</summary>
public static class MotorContabilizacion
{
    public sealed record EventoVentaFacturada(decimal Total, decimal BaseGravable, decimal IvaGenerado);

    public sealed record EventoCompraRecibida(decimal Total, decimal BaseGravable, decimal IvaDescontable);

    public sealed record EventoNominaLiquidada(decimal TotalDevengos, decimal TotalDeducciones, decimal NetoPagar);

    public sealed record LineaRegla(RolContable Rol, LadoMovimiento Lado, decimal Importe);

    public static IReadOnlyList<LineaRegla> ReglasVentaFacturada(EventoVentaFacturada evento, PoliticaRedondeo politica)
    {
        var p = politica.Validada();
        var total = MotorRedondeo.Aplicar(evento.Total, p);
        var baseGravable = MotorRedondeo.Aplicar(evento.BaseGravable, p);
        var iva = MotorRedondeo.Aplicar(evento.IvaGenerado, p);
        if (total != baseGravable + iva)
            throw new InvalidOperationException("El evento de venta no cuadra: total != base + IVA.");
        return
        [
            new LineaRegla(RolContable.CuentaCliente, LadoMovimiento.Debito, total),
            new LineaRegla(RolContable.CuentaIngreso, LadoMovimiento.Credito, baseGravable),
            new LineaRegla(RolContable.CuentaIvaGenerado, LadoMovimiento.Credito, iva)
        ];
    }

    public static IReadOnlyList<LineaRegla> ReglasCompraRecibida(EventoCompraRecibida evento, PoliticaRedondeo politica)
    {
        var p = politica.Validada();
        var total = MotorRedondeo.Aplicar(evento.Total, p);
        var baseGravable = MotorRedondeo.Aplicar(evento.BaseGravable, p);
        var iva = MotorRedondeo.Aplicar(evento.IvaDescontable, p);
        if (total != baseGravable + iva)
            throw new InvalidOperationException("El evento de compra no cuadra: total != base + IVA.");
        return
        [
            new LineaRegla(RolContable.CuentaInventario, LadoMovimiento.Debito, baseGravable),
            new LineaRegla(RolContable.CuentaIvaDescontable, LadoMovimiento.Debito, iva),
            new LineaRegla(RolContable.CuentaProveedor, LadoMovimiento.Credito, total)
        ];
    }

    public static IReadOnlyList<LineaRegla> ReglasNominaLiquidada(EventoNominaLiquidada evento, PoliticaRedondeo politica)
    {
        var p = politica.Validada();
        var devengos = MotorRedondeo.Aplicar(evento.TotalDevengos, p);
        var deducciones = MotorRedondeo.Aplicar(evento.TotalDeducciones, p);
        var neto = MotorRedondeo.Aplicar(evento.NetoPagar, p);
        if (devengos != deducciones + neto)
            throw new InvalidOperationException("La liquidacion no cuadra: devengos != deducciones + neto.");
        return
        [
            new LineaRegla(RolContable.CuentaGastoNomina, LadoMovimiento.Debito, devengos),
            new LineaRegla(RolContable.CuentaRetencionNomina, LadoMovimiento.Credito, deducciones),
            new LineaRegla(RolContable.CuentaObligacionNomina, LadoMovimiento.Credito, neto)
        ];
    }

    public static IReadOnlyList<MovimientoLinea> ResolverMovimientos(
        IReadOnlyList<LineaRegla> reglas, IReadOnlyDictionary<RolContable, Guid> cuentasPorRol, Guid? terceroId = null)
    {
        var movimientos = new List<MovimientoLinea>();
        foreach (var regla in reglas)
        {
            if (!cuentasPorRol.TryGetValue(regla.Rol, out var cuentaId))
                throw new InvalidOperationException($"Falta resolver el rol contable {regla.Rol}.");
            movimientos.Add(new MovimientoLinea(cuentaId, regla.Lado, regla.Importe, terceroId));
        }
        return movimientos;
    }
}
