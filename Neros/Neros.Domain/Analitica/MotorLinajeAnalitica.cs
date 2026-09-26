namespace Neros.Domain.Analitica;

/// <summary>Validaciones minimas de linaje antes de persistir en warehouse.</summary>
public static class MotorLinajeAnalitica
{
    public static void ValidarEvento(EventoParaIngesta evento)
    {
        ArgumentNullException.ThrowIfNull(evento);
        if (evento.MessageId == Guid.Empty || evento.TenantId == Guid.Empty || evento.EmpresaId == Guid.Empty
            || evento.CorrelationId == Guid.Empty)
            throw new ArgumentException("Identificadores de linaje incompletos.");
        if (string.IsNullOrWhiteSpace(evento.TipoEvento) || string.IsNullOrWhiteSpace(evento.AggregateId))
            throw new ArgumentException("Tipo y agregado requeridos.");
        if (string.IsNullOrWhiteSpace(evento.PayloadJson) || !evento.PayloadJson.TrimStart().StartsWith('{'))
            throw new ArgumentException("Payload JSON de objeto requerido.");
    }
}
