namespace Neros.Domain.Integracion;

public enum ClaseEfectoIntegracion
{
    ReservaInventario,
    MovimientoInventario,
    ReglasContables,
    ContabilizacionPendiente,
    IndexacionBusqueda
}

public sealed record EfectoIntegracionProcesado(ClaseEfectoIntegracion Clase, string Resumen);
