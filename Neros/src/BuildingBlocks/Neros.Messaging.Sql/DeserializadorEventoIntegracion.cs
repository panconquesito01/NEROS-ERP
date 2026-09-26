using System.Text.Json;
using Neros.Domain.Integracion;
using Neros.Messaging.Abstractions;

namespace Neros.Messaging.Sql;

public static class DeserializadorEventoIntegracion
{
    private static readonly JsonSerializerOptions Opciones = new() { PropertyNameCaseInsensitive = true };

    public static EventoIntegracionEntrada DesdeSobre(IntegrationEnvelope sobre, string consumidor)
    {
        ArgumentNullException.ThrowIfNull(sobre);
        var carga = DeserializarCarga(sobre.Type, consumidor, sobre.Data);
        return new EventoIntegracionEntrada(
            sobre.MessageId, sobre.Type, sobre.AggregateId, sobre.AggregateVersion,
            sobre.TenantId, sobre.CompanyId, sobre.CorrelationId, sobre.Data.GetRawText(), carga);
    }

    private static object DeserializarCarga(string tipo, string consumidor, JsonElement data)
    {
        if (tipo == MotorEnrutamientoIntegracion.TipoPedidoConfirmado)
            return DeserializarPedido(data);
        if (tipo == MotorEnrutamientoIntegracion.TipoReservaSolicitada)
            return DeserializarReserva(data);
        if (tipo == MotorEnrutamientoIntegracion.TipoRecepcionCompra)
        {
            if (consumidor == MotorEnrutamientoIntegracion.ConsumidorContabilidad)
                return DeserializarRecepcion(data);
            return DeserializarRecepcion(data);
        }
        if (tipo == MotorEnrutamientoIntegracion.TipoContabilizacionSolicitada)
            return JsonSerializer.Deserialize<SolicitudContabilizacionIntegracion>(data.GetRawText(), Opciones)
                   ?? throw new InvalidOperationException("Payload AccountingPostingRequested invalido.");
        if (tipo == MotorEnrutamientoIntegracion.TipoNominaLiquidada)
            return JsonSerializer.Deserialize<ContabilizacionPorNomina>(data.GetRawText(), Opciones)
                   ?? throw new InvalidOperationException("Payload PayrollLiquidationPosted invalido.");
        throw new InvalidOperationException($"Tipo de evento no soportado: {tipo}.");
    }

    private static PedidoConfirmadoIntegracion DeserializarPedido(JsonElement data)
    {
        var dto = JsonSerializer.Deserialize<PedidoDto>(data.GetRawText(), Opciones)
                  ?? throw new InvalidOperationException("Payload SalesOrderConfirmed invalido.");
        var lineas = dto.Lineas?.Select(l => new LineaPedidoParaReserva(l.ProductoReferenciaId, l.BodegaId, l.Cantidad)).ToList()
                     ?? [];
        return new PedidoConfirmadoIntegracion(dto.PedidoId, dto.TenantId, dto.EmpresaId, dto.VersionAgregado, lineas);
    }

    private static ReservaInventarioSolicitada DeserializarReserva(JsonElement data)
    {
        var dto = JsonSerializer.Deserialize<PedidoDto>(data.GetRawText(), Opciones)
                  ?? throw new InvalidOperationException("Payload InventoryReservationRequested invalido.");
        var lineas = dto.Lineas?.Select(l => new LineaPedidoParaReserva(l.ProductoReferenciaId, l.BodegaId, l.Cantidad)).ToList()
                     ?? [];
        return new ReservaInventarioSolicitada(dto.PedidoId, dto.TenantId, dto.EmpresaId, lineas);
    }

    private static RecepcionCompraIntegracion DeserializarRecepcion(JsonElement data)
    {
        var dto = JsonSerializer.Deserialize<RecepcionDto>(data.GetRawText(), Opciones)
                  ?? throw new InvalidOperationException("Payload PurchaseReceiptPosted invalido.");
        var lineas = dto.Lineas?.Select(l => new LineaRecepcionParaInventario(
            l.ProductoReferenciaId, l.BodegaId, l.CantidadRecibida, l.CostoUnitario)).ToList() ?? [];
        return new RecepcionCompraIntegracion(
            dto.RecepcionId, dto.OrdenCompraId, dto.TenantId, dto.EmpresaId, dto.VersionAgregado, lineas);
    }

    private sealed class PedidoDto
    {
        public Guid PedidoId { get; init; }
        public Guid TenantId { get; init; }
        public Guid EmpresaId { get; init; }
        public long VersionAgregado { get; init; }
        public List<LineaPedidoDto>? Lineas { get; init; }
    }

    private sealed class LineaPedidoDto
    {
        public Guid ProductoReferenciaId { get; init; }
        public Guid BodegaId { get; init; }
        public decimal Cantidad { get; init; }
    }

    private sealed class RecepcionDto
    {
        public Guid RecepcionId { get; init; }
        public Guid OrdenCompraId { get; init; }
        public Guid TenantId { get; init; }
        public Guid EmpresaId { get; init; }
        public long VersionAgregado { get; init; }
        public List<LineaRecepcionDto>? Lineas { get; init; }
    }

    private sealed class LineaRecepcionDto
    {
        public Guid ProductoReferenciaId { get; init; }
        public Guid BodegaId { get; init; }
        public decimal CantidadRecibida { get; init; }
        public decimal CostoUnitario { get; init; }
    }
}
