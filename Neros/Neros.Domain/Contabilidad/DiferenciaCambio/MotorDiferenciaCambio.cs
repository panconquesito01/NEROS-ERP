using Neros.Domain.Globalizacion;

namespace Neros.Domain.Contabilidad.DiferenciaCambio;

/// <summary>Diferencia en cambio realizada (plan §45).</summary>
public static class MotorDiferenciaCambio
{
    public enum ContextoCartera
    {
        CxC,
        CxP
    }

    public static decimal CalcularRealizada(decimal importeMonedaExtranjera, decimal tasaPago, decimal tasaDocumento)
    {
        if (importeMonedaExtranjera <= 0) throw new ArgumentOutOfRangeException(nameof(importeMonedaExtranjera));
        return importeMonedaExtranjera * (tasaPago - tasaDocumento);
    }

    public static decimal CalcularRealizadaRedondeada(
        decimal importeMonedaExtranjera, decimal tasaPago, decimal tasaDocumento, PoliticaRedondeo politica)
        => MotorRedondeo.Aplicar(CalcularRealizada(importeMonedaExtranjera, tasaPago, tasaDocumento), politica.Validada());

    public static bool EsIngreso(ContextoCartera contexto, decimal diferencia) => contexto switch
    {
        ContextoCartera.CxC => diferencia > 0,
        ContextoCartera.CxP => diferencia < 0,
        _ => throw new ArgumentOutOfRangeException(nameof(contexto))
    };

    public static bool EsGasto(ContextoCartera contexto, decimal diferencia) => contexto switch
    {
        ContextoCartera.CxC => diferencia < 0,
        ContextoCartera.CxP => diferencia > 0,
        _ => throw new ArgumentOutOfRangeException(nameof(contexto))
    };

    public static decimal ReexpresionNoRealizada(decimal saldoMonedaExtranjera, decimal tasaCierre, decimal tasaAnterior)
        => saldoMonedaExtranjera * (tasaCierre - tasaAnterior);
}
