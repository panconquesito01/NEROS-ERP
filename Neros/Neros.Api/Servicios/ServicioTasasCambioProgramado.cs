using Microsoft.Extensions.Options;
using Neros.Application.Globalizacion;

namespace Neros.Api.Servicios;

public sealed class ServicioTasasCambioProgramado(
    IServiceScopeFactory alcances,
    IOptions<OpcionesTasasCambio> opciones,
    ILogger<ServicioTasasCambioProgramado> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = opciones.Value;
        if (!config.Automatico) return;

        if (config.RetrasoInicioSegundos > 0)
            await Task.Delay(TimeSpan.FromSeconds(config.RetrasoInicioSegundos), stoppingToken);

        var intervalo = TimeSpan.FromHours(Math.Max(1, config.IntervaloHoras));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = alcances.CreateAsyncScope();
                var sincronizacion = scope.ServiceProvider.GetRequiredService<IServicioSincronizacionTasasReferencia>();
                await sincronizacion.EjecutarCicloAsync(fecha: null, stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Ciclo automático de TRM falló; se reintentará en {Horas} h.", intervalo.TotalHours);
            }
            await Task.Delay(intervalo, stoppingToken);
        }
    }
}
