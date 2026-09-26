namespace Neros.Domain.Analitica;

/// <summary>Watermarks y eventos tardios (D-17a).</summary>
public static class MotorMarcaAguaIngesta
{
    public static (bool Aceptar, bool EsTardio, MarcaAguaIngesta MarcaActualizada) Evaluar(
        MarcaAguaIngesta? marcaActual,
        EventoParaIngesta evento,
        TimeSpan toleranciaTardio)
    {
        ArgumentNullException.ThrowIfNull(evento);
        if (string.IsNullOrWhiteSpace(evento.FuenteModulo))
            throw new ArgumentException("Fuente requerida.", nameof(evento));
        if (evento.OcurrioEnUtc.Offset != TimeSpan.Zero)
            throw new ArgumentException("Instante UTC requerido.", nameof(evento));

        var instante = evento.OcurrioEnUtc.UtcDateTime;
        if (marcaActual is null)
        {
            return (true, false, new MarcaAguaIngesta(
                evento.TenantId, evento.EmpresaId, evento.FuenteModulo, instante, evento.MessageId));
        }

        if (instante > marcaActual.UltimoInstanteUtc)
        {
            return (true, false, marcaActual with
            {
                UltimoInstanteUtc = instante,
                UltimoEventoId = evento.MessageId
            });
        }

        if (marcaActual.UltimoInstanteUtc - instante <= toleranciaTardio)
            return (true, true, marcaActual);

        return (false, true, marcaActual);
    }
}
