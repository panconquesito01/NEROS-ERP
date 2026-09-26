using Neros.Domain.Integracion;
using Neros.Messaging.Abstractions;
using Neros.Messaging.RabbitMQ;
using Neros.Messaging.Sql;

namespace Neros.Integration.Worker;

public sealed class ServicioConsumoIntegracion(
    BusEventosRabbitMq bus,
    IConfiguration configuracion,
    ILogger<ServicioConsumoIntegracion> logger) : BackgroundService
{
    private readonly Dictionary<string, ConsumidorIntegracionPersistente> _consumidores = new(StringComparer.Ordinal);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cadenas = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Inventario"] = configuracion.GetConnectionString("Inventario") ?? "",
            ["Contabilidad"] = configuracion.GetConnectionString("Contabilidad") ?? "",
            ["Search"] = configuracion.GetConnectionString("Search") ?? ""
        };
        foreach (var def in RegistroConsumidoresIntegracion.DefinicionesPorDefecto())
        {
            var cadena = configuracion.GetConnectionString(def.CadenaConexion);
            if (string.IsNullOrWhiteSpace(cadena))
            {
                logger.LogWarning("ConnectionStrings:{Modulo} omitida; consumidor {Consumidor} inactivo.",
                    def.CadenaConexion, def.Consumidor);
                continue;
            }

            var clave = def.Consumidor + "|" + def.CadenaConexion;
            if (!_consumidores.ContainsKey(clave))
            {
                var escritor = RegistroConsumidoresIntegracion.ResolverEscritor(def.Consumidor, cadenas);
                _consumidores[clave] = RegistroConsumidoresIntegracion.Crear(def.Consumidor, cadena, escritor);
            }

            await bus.AsegurarColaAsync(def.ColaRabbit, def.RoutingKey, stoppingToken);
            logger.LogInformation("Cola {Cola} enlazada a {RoutingKey}.", def.ColaRabbit, def.RoutingKey);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var procesados = 0;
            try
            {
                foreach (var def in RegistroConsumidoresIntegracion.DefinicionesPorDefecto())
                {
                    var cadena = configuracion.GetConnectionString(def.CadenaConexion);
                    if (string.IsNullOrWhiteSpace(cadena)) continue;
                    var clave = def.Consumidor + "|" + def.CadenaConexion;
                    if (!_consumidores.TryGetValue(clave, out var consumidor)) continue;

                    while (await bus.IntentarRecibirAsync(def.ColaRabbit, async (sobre, ct) =>
                           {
                               var (resultado, efectos) = await consumidor.ProcesarAsync(sobre, ct);
                               if (resultado == ResultadoProcesamientoIntegracion.Procesado)
                                   logger.LogInformation("Integracion {Consumidor}: {Tipo} -> {Resumen}",
                                       consumidor.Consumidor, sobre.Type, efectos[0].Resumen);
                           }, stoppingToken))
                    {
                        procesados++;
                    }
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Fallo temporal en consumo integracion.");
            }

            var espera = procesados > 0 ? TimeSpan.FromMilliseconds(200) : TimeSpan.FromSeconds(2);
            await Task.Delay(espera, stoppingToken);
        }
    }
}
