namespace Neros.Domain.Contabilidad;

/// <summary>Invariante Activo = Pasivo + Patrimonio (+ resultados antes del cierre) — plan §33.</summary>
public static class EcuacionContable
{
    public static EcuacionContableResultado Evaluar(
        IReadOnlyDictionary<Guid, CuentaContable> cuentas,
        IReadOnlyDictionary<Guid, decimal> saldosFirmados,
        bool despuesCierreResultados)
    {
        decimal MagnitudNatural(CuentaContable cuenta)
        {
            var firmado = saldosFirmados.GetValueOrDefault(cuenta.Id);
            return cuenta.Naturaleza == NaturalezaCuenta.Debito ? firmado : -firmado;
        }

        decimal SumarTipo(TipoCuenta tipo) => cuentas.Values
            .Where(c => c.Tipo == tipo && !TiposContables.EsCuentaOrden(c.Tipo))
            .Sum(MagnitudNatural);

        var activo = SumarTipo(TipoCuenta.Activo);
        var pasivo = SumarTipo(TipoCuenta.Pasivo);
        var patrimonio = SumarTipo(TipoCuenta.Patrimonio);
        var ingresos = SumarTipo(TipoCuenta.Ingreso);
        var costos = SumarTipo(TipoCuenta.Costo);
        var gastos = SumarTipo(TipoCuenta.Gasto);
        var resultado = ingresos - costos - gastos;
        var cumple = despuesCierreResultados
            ? activo == pasivo + patrimonio
            : activo == pasivo + patrimonio + resultado;
        return new EcuacionContableResultado(activo, pasivo, patrimonio, ingresos, costos, gastos, cumple);
    }
}
