using System.Text.Json;
using Neros.Domain.Integracion;

namespace Neros.Domain.Busqueda;

public static class ProyectorEventosBusqueda
{
    private static readonly JsonSerializerOptions Opciones = new() { PropertyNameCaseInsensitive = true };

    public const string PermisoPedidoConsultar = "VENTAS.PEDIDO.CONSULTAR";
    public const string PermisoTerceroConsultar = "TERCEROS.TERCERO.CONSULTAR";
    public const string PermisoOrdenConsultar = "COMPRAS.ORDEN.CONSULTAR";

    public static DocumentoIndice? Proyectar(
        EventoParaIndexacion evento,
        DateTimeOffset indexadoEnUtc)
    {
        ArgumentNullException.ThrowIfNull(evento);
        if (evento.EsTombstone)
            return null;

        return evento.TipoEvento switch
        {
            MotorEnrutamientoIntegracion.TipoPedidoConfirmado => ProyectarPedido(evento, indexadoEnUtc),
            _ => null
        };
    }

    public static IReadOnlyList<EnlaceVista360> ProyectarEnlaces(
        DocumentoIndice documento,
        EventoParaIndexacion evento)
    {
        if (evento.TipoEvento != MotorEnrutamientoIntegracion.TipoPedidoConfirmado)
            return [];
        var dto = JsonSerializer.Deserialize<PedidoPayload>(evento.PayloadJson, Opciones);
        if (dto?.ClienteDocumentoId is null || dto.ClienteDocumentoId == Guid.Empty)
            return [];
        var docCliente = Guid.NewGuid();
        return
        [
            new EnlaceVista360(Guid.NewGuid(), documento.Id, docCliente, TiposRelacionVista360.ClienteDePedido)
        ];
    }

    private static DocumentoIndice ProyectarPedido(EventoParaIndexacion evento, DateTimeOffset indexadoEnUtc)
    {
        var dto = JsonSerializer.Deserialize<PedidoPayload>(evento.PayloadJson, Opciones)
                  ?? throw new InvalidOperationException("Payload de pedido invalido.");
        var titulo = string.IsNullOrWhiteSpace(dto.Referencia)
            ? $"Pedido {dto.PedidoId:N}"
            : dto.Referencia.Trim();
        var texto = $"{titulo} {dto.ClienteNombre}".Trim();
        return new DocumentoIndice(
            Guid.NewGuid(),
            evento.TenantId,
            evento.EmpresaId,
            TiposEntidadIndexable.PedidoVenta,
            evento.EntidadId,
            evento.VersionAgregado,
            titulo,
            dto.ClienteNombre,
            texto,
            PermisoPedidoConsultar,
            evento.FuenteModulo,
            evento.CorrelationId,
            Activo: true,
            TombstoneEnUtc: null,
            indexadoEnUtc);
    }

    private sealed class PedidoPayload
    {
        public Guid PedidoId { get; init; }
        public string? Referencia { get; init; }
        public string? ClienteNombre { get; init; }
        public Guid? ClienteDocumentoId { get; init; }
    }
}
