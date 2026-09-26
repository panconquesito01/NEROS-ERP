namespace Neros.Domain.Busqueda;

public static class OrquestadorIndexacionBusqueda
{
    public static (ResultadoIndexacion Resultado, DocumentoIndice? Documento, CheckpointIngesta? Checkpoint, IReadOnlyList<EnlaceVista360> Enlaces)
        Ejecutar(
            IReadOnlySet<Guid> messageIdsConocidos,
            MotorVersionIndice versionIndice,
            CheckpointIngesta? checkpointActual,
            DocumentoIndice? documentoExistente,
            EventoParaIndexacion evento,
            DateTimeOffset indexadoEnUtc)
    {
        ArgumentNullException.ThrowIfNull(messageIdsConocidos);
        ArgumentNullException.ThrowIfNull(versionIndice);
        ArgumentNullException.ThrowIfNull(evento);

        if (messageIdsConocidos.Contains(evento.MessageId))
            return (ResultadoIndexacion.DuplicadoIgnorado, documentoExistente, checkpointActual, []);

        if (evento.EsTombstone)
        {
            if (documentoExistente is null)
                return (ResultadoIndexacion.TombstoneAplicado, null, ActualizarCheckpoint(checkpointActual, evento), []);
            var tombstone = MotorTombstoneBusqueda.Aplicar(documentoExistente, indexadoEnUtc);
            return (ResultadoIndexacion.TombstoneAplicado, tombstone, ActualizarCheckpoint(checkpointActual, evento), []);
        }

        var tipo = evento.TipoEvento switch
        {
            Integracion.MotorEnrutamientoIntegracion.TipoPedidoConfirmado => TiposEntidadIndexable.PedidoVenta,
            _ => evento.TipoEvento
        };

        if (!versionIndice.DebeIndexar(tipo, evento.EntidadId, evento.VersionAgregado))
            return (ResultadoIndexacion.VersionAntiguaIgnorada, documentoExistente, checkpointActual, []);

        var documento = ProyectorEventosBusqueda.Proyectar(evento, indexadoEnUtc);
        if (documento is null)
            return (ResultadoIndexacion.VersionAntiguaIgnorada, documentoExistente, checkpointActual, []);

        var enlaces = ProyectorEventosBusqueda.ProyectarEnlaces(documento, evento);
        return (ResultadoIndexacion.Indexado, documento, ActualizarCheckpoint(checkpointActual, evento), enlaces);
    }

    private static CheckpointIngesta ActualizarCheckpoint(CheckpointIngesta? actual, EventoParaIndexacion evento)
    {
        var instante = evento.OcurrioEnUtc.UtcDateTime;
        if (actual is null)
            return new CheckpointIngesta(evento.TenantId, evento.EmpresaId, evento.FuenteModulo, instante, evento.MessageId);
        if (instante <= actual.UltimoInstanteUtc) return actual;
        return actual with { UltimoInstanteUtc = instante, UltimoEventoId = evento.MessageId };
    }
}
