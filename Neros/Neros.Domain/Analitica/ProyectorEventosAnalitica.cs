using System.Text.Json;
using Neros.Domain.Integracion;

namespace Neros.Domain.Analitica;

/// <summary>Proyecta eventos de integracion a hechos operativos (sin acceder a OLTP).</summary>
public static class ProyectorEventosAnalitica
{
    private static readonly JsonSerializerOptions Opciones = new() { PropertyNameCaseInsensitive = true };

    public static IReadOnlyList<HechoOperativo> Proyectar(EventoIngestaRegistrado registro)
    {
        ArgumentNullException.ThrowIfNull(registro);
        var evento = registro.Evento;
        var periodo = DateOnly.FromDateTime(evento.OcurrioEnUtc.UtcDateTime);
        return evento.TipoEvento switch
        {
            MotorEnrutamientoIntegracion.TipoPedidoConfirmado => ProyectarPedido(registro.Id, evento, periodo),
            MotorEnrutamientoIntegracion.TipoRecepcionCompra => ProyectarRecepcion(registro.Id, evento, periodo),
            MotorEnrutamientoIntegracion.TipoNominaLiquidada => ProyectarNomina(registro.Id, evento, periodo),
            _ => []
        };
    }

    private static IReadOnlyList<HechoOperativo> ProyectarPedido(Guid eventoId, EventoParaIngesta evento, DateOnly periodo)
    {
        var dto = JsonSerializer.Deserialize<PedidoPayload>(evento.PayloadJson, Opciones);
        if (dto?.Lineas is null || dto.Lineas.Count == 0) return [];
        decimal cantidad = 0;
        foreach (var linea in dto.Lineas)
            cantidad += linea.Cantidad;
        var importe = dto.TotalNeto ?? 0m;
        return
        [
            new HechoOperativo(Guid.NewGuid(), evento.TenantId, evento.EmpresaId, TiposHechoAnalitico.PedidoConfirmado,
                periodo, importe, cantidad, dto.PedidoId.ToString(), eventoId)
        ];
    }

    private static IReadOnlyList<HechoOperativo> ProyectarRecepcion(Guid eventoId, EventoParaIngesta evento, DateOnly periodo)
    {
        var dto = JsonSerializer.Deserialize<RecepcionPayload>(evento.PayloadJson, Opciones);
        if (dto?.Lineas is null || dto.Lineas.Count == 0) return [];
        decimal total = 0;
        decimal cantidad = 0;
        foreach (var linea in dto.Lineas)
        {
            cantidad += linea.CantidadRecibida;
            total += linea.CantidadRecibida * linea.CostoUnitario;
        }
        return
        [
            new HechoOperativo(Guid.NewGuid(), evento.TenantId, evento.EmpresaId, TiposHechoAnalitico.RecepcionCompra,
                periodo, total, cantidad, dto.RecepcionId.ToString(), eventoId)
        ];
    }

    private static IReadOnlyList<HechoOperativo> ProyectarNomina(Guid eventoId, EventoParaIngesta evento, DateOnly periodo)
    {
        var dto = JsonSerializer.Deserialize<NominaPayload>(evento.PayloadJson, Opciones);
        if (dto is null) return [];
        return
        [
            new HechoOperativo(Guid.NewGuid(), evento.TenantId, evento.EmpresaId, TiposHechoAnalitico.NominaLiquidada,
                periodo, dto.NetoPagar, null, dto.LiquidacionId.ToString(), eventoId)
        ];
    }

    private sealed class PedidoPayload
    {
        public Guid PedidoId { get; init; }
        public decimal? TotalNeto { get; init; }
        public List<LineaPedidoPayload>? Lineas { get; init; }
    }

    private sealed class LineaPedidoPayload
    {
        public decimal Cantidad { get; init; }
    }

    private sealed class RecepcionPayload
    {
        public Guid RecepcionId { get; init; }
        public List<LineaRecepcionPayload>? Lineas { get; init; }
    }

    private sealed class LineaRecepcionPayload
    {
        public decimal CantidadRecibida { get; init; }
        public decimal CostoUnitario { get; init; }
    }

    private sealed class NominaPayload
    {
        public Guid LiquidacionId { get; init; }
        public decimal NetoPagar { get; init; }
    }
}
