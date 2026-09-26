using Neros.Domain.Integracion;

namespace Neros.Messaging.Sql;

public static class RegistroConsumidoresIntegracion
{
    public sealed record DefinicionConsumidor(string Consumidor, string CadenaConexion, string ColaRabbit, string RoutingKey);

    public static IReadOnlyList<DefinicionConsumidor> DefinicionesPorDefecto() =>
    [
        new(MotorEnrutamientoIntegracion.ConsumidorInventarioReservas, "Inventario",
            "neros.inventario.reservas", MotorEnrutamientoIntegracion.TipoPedidoConfirmado),
        new(MotorEnrutamientoIntegracion.ConsumidorInventarioReservas, "Inventario",
            "neros.inventario.reservas", MotorEnrutamientoIntegracion.TipoReservaSolicitada),
        new(MotorEnrutamientoIntegracion.ConsumidorInventarioEntradas, "Inventario",
            "neros.inventario.entradas", MotorEnrutamientoIntegracion.TipoRecepcionCompra),
        new(MotorEnrutamientoIntegracion.ConsumidorContabilidad, "Contabilidad",
            "neros.contabilidad.posting", MotorEnrutamientoIntegracion.TipoRecepcionCompra),
        new(MotorEnrutamientoIntegracion.ConsumidorContabilidad, "Contabilidad",
            "neros.contabilidad.posting", MotorEnrutamientoIntegracion.TipoContabilizacionSolicitada),
        new(MotorEnrutamientoIntegracion.ConsumidorContabilidad, "Contabilidad",
            "neros.contabilidad.posting", MotorEnrutamientoIntegracion.TipoNominaLiquidada),
        new(MotorEnrutamientoIntegracion.ConsumidorBusqueda, "Search",
            "neros.busqueda.indexacion", MotorEnrutamientoIntegracion.TipoPedidoConfirmado)
    ];

    public static ConsumidorIntegracionPersistente Crear(
        string consumidor,
        string cadenaConexion,
        IEscritorIntegracion? escritor = null)
    {
        var inbox = new AlmacenInboxSql(cadenaConexion);
        var ordenador = new OrdenadorVersionAgregado();
        return new ConsumidorIntegracionPersistente(consumidor, cadenaConexion, inbox, ordenador, escritor);
    }

    public static IEscritorIntegracion? ResolverEscritor(string consumidor, IReadOnlyDictionary<string, string> cadenas)
    {
        if (consumidor is MotorEnrutamientoIntegracion.ConsumidorInventarioReservas
            or MotorEnrutamientoIntegracion.ConsumidorInventarioEntradas)
        {
            if (!cadenas.TryGetValue("Inventario", out var inv) || string.IsNullOrWhiteSpace(inv))
                return null;
            return new EscritorInventarioIntegracionSql(inv);
        }
        if (consumidor == MotorEnrutamientoIntegracion.ConsumidorContabilidad)
        {
            if (!cadenas.TryGetValue("Contabilidad", out var cont) || string.IsNullOrWhiteSpace(cont))
                return null;
            return new EscritorContabilidadIntegracionSql(cont);
        }
        if (consumidor == MotorEnrutamientoIntegracion.ConsumidorBusqueda)
        {
            if (!cadenas.TryGetValue("Search", out var search) || string.IsNullOrWhiteSpace(search))
                return null;
            return new EscritorIndexacionBusquedaSql(search);
        }
        return null;
    }
}
