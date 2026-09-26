namespace Neros.Application.Globalizacion;

public sealed class OpcionesTasasCambio
{
    public const string Seccion = "TasasCambio";

    /// <summary>Job en segundo plano y sincronización al consultar si no hay tasas de hoy.</summary>
    public bool Automatico { get; set; } = true;

    /// <summary>Intervalo entre ciclos completos (todas las monedas en uso).</summary>
    public int IntervaloHoras { get; set; } = 24;

    /// <summary>Retraso inicial tras arrancar la API antes del primer ciclo.</summary>
    public int RetrasoInicioSegundos { get; set; } = 45;

    /// <summary>Si la consulta de tasas de hoy viene vacía, intenta traerlas del mercado.</summary>
    public bool SincronizarAlConsultarSiFaltaHoy { get; set; } = true;
}
