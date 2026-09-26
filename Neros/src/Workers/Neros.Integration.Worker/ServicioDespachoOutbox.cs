using Neros.Messaging.Sql;

namespace Neros.Integration.Worker;

public sealed class ServicioDespachoOutbox(DespachadorOutboxModulos despachador, ILogger<ServicioDespachoOutbox> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var instancia = Environment.MachineName;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var publicados = await despachador.DespacharLoteAsync(instancia, 50, stoppingToken);
                if (publicados > 0)
                    logger.LogInformation("Outbox multi-modulo: {Cantidad} mensajes publicados.", publicados);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Fallo temporal en despacho outbox.");
            }
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
