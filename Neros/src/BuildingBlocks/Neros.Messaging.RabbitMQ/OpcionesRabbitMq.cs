namespace Neros.Messaging.RabbitMQ;

public sealed class OpcionesRabbitMq
{
    public string Host { get; set; } = "localhost";
    public int Puerto { get; set; } = 5672;
    public string Usuario { get; set; } = "guest";
    public string Clave { get; set; } = "guest";
    public string Exchange { get; set; } = "neros.integracion";
}
