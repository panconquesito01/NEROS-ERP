namespace Neros.Domain.Analitica;

public static class OrquestadorIngestaAnalitica
{
    public static (ResultadoIngesta Resultado, MarcaAguaIngesta? Marca, EventoIngestaRegistrado? Registro, IReadOnlyList<HechoOperativo> Hechos) Ejecutar(
        IReadOnlySet<Guid> messageIdsConocidos,
        MarcaAguaIngesta? marcaActual,
        EventoParaIngesta evento,
        TimeSpan? toleranciaTardio = null)
    {
        MotorLinajeAnalitica.ValidarEvento(evento);
        var (resultado, marca, registro) = MotorIngestaEventos.Procesar(
            messageIdsConocidos, marcaActual, evento, toleranciaTardio);
        if (resultado != ResultadoIngesta.Ingerido || registro is null)
            return (resultado, marca, registro, []);

        var hechos = ProyectorEventosAnalitica.Proyectar(registro);
        return (resultado, marca, registro, hechos);
    }
}
