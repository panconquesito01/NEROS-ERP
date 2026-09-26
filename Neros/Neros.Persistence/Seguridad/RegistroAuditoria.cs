using Neros.Application.Seguridad;

namespace Neros.Persistence.Seguridad;

/// <summary>Agrega eventos a <c>auditoria.Evento</c> dentro de la unidad de trabajo del llamador; se guardan con su SaveChanges.</summary>
public sealed class RegistroAuditoria(NerosDbContext database, TimeProvider reloj)
{
    public const string Correcto = "Correcto";
    public const string Rechazado = "Rechazado";

    public void Agregar(string accion, string resultado, string? actorId, ContextoCliente cliente,
        string? entidad = null, string? entidadId = null, string? detalle = null) =>
        database.EventosAuditoria.Add(new EventoAuditoria
        {
            FechaUtc = reloj.GetUtcNow().UtcDateTime,
            Modulo = "Seguridad",
            Accion = accion,
            Resultado = resultado,
            ActorId = actorId,
            Entidad = entidad,
            EntidadId = entidadId,
            Ip = cliente.Ip,
            AgenteUsuario = cliente.AgenteUsuario,
            CorrelationId = cliente.CorrelationId,
            Detalle = detalle
        });
}
