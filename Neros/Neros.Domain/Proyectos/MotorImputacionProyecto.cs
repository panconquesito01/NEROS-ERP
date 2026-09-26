namespace Neros.Domain.Proyectos;

public static class MotorImputacionProyecto
{
    public static void ValidarReferencia(ProyectoContexto proyecto, ReferenciaMovimientoProyectoEntrada entrada)
    {
        ArgumentNullException.ThrowIfNull(proyecto);
        ArgumentNullException.ThrowIfNull(entrada);
        if (entrada.ImporteAsignado < 0) throw new ArgumentOutOfRangeException(nameof(entrada));
        if (entrada.DocumentoOrigenId == Guid.Empty) throw new ArgumentException("Documento origen requerido.");
        if (string.IsNullOrWhiteSpace(entrada.TipoDocumento))
            throw new ArgumentException("Tipo de documento requerido.", nameof(entrada));
        MaquinaEstadosProyecto.ValidarAceptaImputaciones(proyecto.Estado);
        if (entrada.FechaNegocio < proyecto.FechaInicioPlan)
            throw new InvalidOperationException("La fecha de negocio es anterior al inicio del proyecto.");
        if (proyecto.FechaFinPlan is { } fin && entrada.FechaNegocio > fin)
            throw new InvalidOperationException("La fecha de negocio es posterior al fin planificado del proyecto.");
    }

    public static ResultadoImputacionProyecto CalcularSaldo(
        ProyectoContexto proyecto,
        decimal totalImputadoActual,
        decimal nuevoImporte)
    {
        if (nuevoImporte < 0) throw new ArgumentOutOfRangeException(nameof(nuevoImporte));
        var total = totalImputadoActual + nuevoImporte;
        var restante = proyecto.PresupuestoTotal - total;
        return new ResultadoImputacionProyecto(total, restante, restante < 0);
    }
}
