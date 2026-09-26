namespace Neros.Domain.Analitica;

public static class MotorIngestaEventos
{
    public static readonly TimeSpan ToleranciaTardioPorDefecto = TimeSpan.FromDays(7);

    public static (ResultadoIngesta Resultado, MarcaAguaIngesta? Marca, EventoIngestaRegistrado? Registro) Procesar(
        IReadOnlySet<Guid> messageIdsConocidos,
        MarcaAguaIngesta? marcaActual,
        EventoParaIngesta evento,
        TimeSpan? toleranciaTardio = null)
    {
        ArgumentNullException.ThrowIfNull(messageIdsConocidos);
        ArgumentNullException.ThrowIfNull(evento);
        if (messageIdsConocidos.Contains(evento.MessageId))
            return (ResultadoIngesta.DuplicadoIgnorado, marcaActual, null);

        var (aceptar, esTardio, marca) = MotorMarcaAguaIngesta.Evaluar(
            marcaActual, evento, toleranciaTardio ?? ToleranciaTardioPorDefecto);
        if (!aceptar)
            return (ResultadoIngesta.RechazadoTardio, marcaActual, null);

        var registro = new EventoIngestaRegistrado(Guid.NewGuid(), evento, esTardio);
        return (ResultadoIngesta.Ingerido, marca, registro);
    }
}
