using System.Text.Json;
using Neros.Messaging.Abstractions;

namespace Neros.Messaging.Sql;

public sealed class DespachadorOutbox(AlmacenOutboxSql almacen, IEventBus bus)
{
    private static readonly JsonSerializerOptions JsonOpciones = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<int> DespacharLoteAsync(string propietarioLease, int maximo, CancellationToken cancellationToken = default)
    {
        var pendientes = await almacen.ReclamarPendientesAsync(propietarioLease, TimeSpan.FromMinutes(2), maximo, cancellationToken);
        var publicados = 0;
        foreach (var pendiente in pendientes)
        {
            try
            {
                var mensaje = JsonSerializer.Deserialize<IntegrationEnvelope>(pendiente.PayloadJson, JsonOpciones)
                    ?? throw new InvalidOperationException("Payload invalido.");
                await bus.PublishAsync(mensaje, cancellationToken);
                await almacen.MarcarPublicadoAsync(pendiente.Id, cancellationToken);
                publicados++;
            }
            catch (Exception ex)
            {
                await almacen.RegistrarErrorAsync(pendiente.Id, ex.Message, cancellationToken);
            }
        }
        return publicados;
    }
}
