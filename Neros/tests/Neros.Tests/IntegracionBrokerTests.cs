using System.Net.Sockets;
using System.Text.Json;
using Neros.Messaging.Abstractions;
using Neros.Messaging.RabbitMQ;
using Xunit;

namespace Neros.Tests;

public sealed class IntegracionBrokerTests
{
    [Fact]
    public async Task RabbitMq_PublicaCuandoBrokerAccesibleAsync()
    {
        if (!BrokerAccesible())
        {
            if (Environment.GetEnvironmentVariable("NEROS_REQUIRE_BROKER_TESTS") == "1")
                throw new InvalidOperationException(
                    "RabbitMQ requerido en localhost:5672. Levante deploy/development/docker-compose.rabbitmq.yml.");
            return;
        }

        await using var bus = new BusEventosRabbitMq(new OpcionesRabbitMq());
        using var doc = JsonDocument.Parse("{\"demo\":true}");
        var mensaje = new IntegrationEnvelope(
            Guid.NewGuid(), "SalesOrderConfirmed.v1", "Ventas", Guid.NewGuid(), Guid.NewGuid(),
            "pedido-demo", 1, DateTimeOffset.UtcNow, null, Guid.NewGuid(), null, null, doc.RootElement);
        await bus.PublishAsync(mensaje, CancellationToken.None);
    }

    private static bool BrokerAccesible()
    {
        try
        {
            using var cliente = new TcpClient();
            var tarea = cliente.ConnectAsync("localhost", 5672);
            return tarea.Wait(TimeSpan.FromSeconds(2)) && cliente.Connected;
        }
        catch
        {
            return false;
        }
    }
}
