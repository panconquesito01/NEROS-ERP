namespace Neros.Application.Seguridad;

/// <summary>Datos del cliente para sesiones y auditoria. IP y agente son datos personales informativos.</summary>
public sealed record ContextoCliente(string? Ip, string? AgenteUsuario, string? CorrelationId)
{
    public static ContextoCliente Vacio { get; } = new(null, null, null);
}
