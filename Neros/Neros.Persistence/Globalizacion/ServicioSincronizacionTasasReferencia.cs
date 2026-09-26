using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neros.Application.Globalizacion;
using Neros.Contracts.Globalizacion;
using Neros.Organization.Persistence;

namespace Neros.Persistence.Globalizacion;

public sealed class ServicioSincronizacionTasasReferencia(
    IServicioTasasCambio tasas,
    TimeProvider reloj,
    IOptions<OpcionesTasasCambio> opciones,
    IServiceProvider services,
    ILogger<ServicioSincronizacionTasasReferencia> logger) : IServicioSincronizacionTasasReferencia
{
    private static readonly (string Moneda, string Pais)[] ReferenciasMinimas =
    [
        ("COP", "CO"), ("MXN", "MX"), ("USD", "US"), ("BRL", "BR"), ("PEN", "PE"), ("CLP", "CL")
    ];

    public async Task<int> EjecutarCicloAsync(DateOnly? fecha, CancellationToken cancellationToken)
    {
        if (!opciones.Value.Automatico) return 0;
        var dia = fecha ?? DateOnly.FromDateTime(reloj.GetUtcNow().UtcDateTime);
        var pares = await ObtenerParesAsync(cancellationToken);
        var sincronizados = 0;
        foreach (var (moneda, pais) in pares)
        {
            if (await SincronizarParInternoAsync(moneda, pais, dia, cancellationToken))
                sincronizados++;
        }
        if (sincronizados > 0)
            logger.LogInformation("TRM automática: {Cantidad} moneda(s) actualizada(s) para {Fecha}.", sincronizados, dia);
        return sincronizados;
    }

    public Task<bool> SincronizarParAsync(string monedaDestino, string? pais, DateOnly? fecha, CancellationToken cancellationToken)
    {
        var dia = fecha ?? DateOnly.FromDateTime(reloj.GetUtcNow().UtcDateTime);
        var moneda = monedaDestino.Trim().ToUpperInvariant();
        var paisResuelto = string.IsNullOrWhiteSpace(pais) ? InferirPais(moneda) : pais.Trim().ToUpperInvariant();
        return SincronizarParInternoAsync(moneda, paisResuelto, dia, cancellationToken);
    }

    private async Task<bool> SincronizarParInternoAsync(string moneda, string? pais, DateOnly dia, CancellationToken cancellationToken)
    {
        var (resultado, error) = await tasas.SincronizarMercadoAsync(new SolicitudSincronizarTasas
        {
            MonedaDestino = moneda,
            Pais = pais,
            Fecha = dia
        }, cancellationToken);
        if (error is not null)
        {
            logger.LogDebug("TRM {Moneda}/{Pais} {Fecha}: {Error}.", moneda, pais, dia, error);
            return false;
        }
        _ = resultado;
        return true;
    }

    private async Task<IReadOnlyList<(string Moneda, string Pais)>> ObtenerParesAsync(CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var organizacion = scope.ServiceProvider.GetService<OrganizationDbContext>();
        if (organizacion is not null)
        {
            var filas = await organizacion.Empresas.AsNoTracking()
                .Where(e => e.Activa && e.MonedaFuncional != "" && e.Pais != "")
                .Select(e => new { e.MonedaFuncional, e.Pais })
                .ToListAsync(cancellationToken);
            var desdeEmpresas = filas
                .Select(e => (Moneda: e.MonedaFuncional.Trim().ToUpperInvariant(), Pais: e.Pais.Trim().ToUpperInvariant()))
                .Distinct()
                .ToList();
            if (desdeEmpresas.Count > 0)
                return desdeEmpresas;
        }
        return ReferenciasMinimas;
    }

    internal static string? InferirPais(string moneda) =>
        moneda.ToUpperInvariant() switch
        {
            "COP" => "CO",
            "MXN" => "MX",
            "USD" => "US",
            "BRL" => "BR",
            "PEN" => "PE",
            "CLP" => "CL",
            "EUR" => "DE",
            _ => null
        };
}
