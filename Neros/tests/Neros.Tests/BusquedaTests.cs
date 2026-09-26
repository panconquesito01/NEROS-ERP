using Neros.Application.Seguridad;
using Neros.Domain.Busqueda;
using Neros.Domain.Integracion;
using Xunit;

namespace Neros.Tests;

public sealed class BusquedaTests
{
    [Fact]
    public void Acl_OcultaDocumentoSinPermiso()
    {
        var tenant = Guid.NewGuid();
        var empresa = Guid.NewGuid();
        var doc = Documento(tenant, empresa, ProyectorEventosBusqueda.PermisoPedidoConsultar);
        var resultado = MotorConsultaBusqueda.Buscar(
            [doc],
            new HashSet<string> { Permisos.TerceroConsultar },
            new CriterioBusqueda(tenant, empresa, "Pedido", 10, null, null),
            []);
        Assert.Empty(resultado.Documentos);
    }

    [Fact]
    public void Tombstone_ExcluyeDocumento()
    {
        var tenant = Guid.NewGuid();
        var empresa = Guid.NewGuid();
        var doc = MotorTombstoneBusqueda.Aplicar(
            Documento(tenant, empresa, ProyectorEventosBusqueda.PermisoPedidoConsultar),
            DateTimeOffset.UtcNow);
        var permisos = new HashSet<string> { Permisos.VentasPedidoConsultar };
        var resultado = MotorConsultaBusqueda.Buscar(
            [doc], permisos, new CriterioBusqueda(tenant, empresa, "", 10, null, null), []);
        Assert.Empty(resultado.Documentos);
    }

    [Fact]
    public void Consulta_FiltraAntesDePaginar()
    {
        var tenant = Guid.NewGuid();
        var empresa = Guid.NewGuid();
        var permisos = new HashSet<string> { Permisos.VentasPedidoConsultar, Permisos.ComprasOrdenConsultar };
        var docs = new List<DocumentoIndice>
        {
            Documento(tenant, empresa, ProyectorEventosBusqueda.PermisoPedidoConsultar, "Alpha"),
            Documento(tenant, empresa, ProyectorEventosBusqueda.PermisoOrdenConsultar, "Beta"),
            Documento(tenant, empresa, ProyectorEventosBusqueda.PermisoPedidoConsultar, "Gamma")
        };
        var pagina1 = MotorConsultaBusqueda.Buscar(
            docs, permisos, new CriterioBusqueda(tenant, empresa, "", 2, null, null), []);
        Assert.Equal(2, pagina1.Documentos.Count);
        Assert.NotNull(pagina1.SiguienteCursorTitulo);
        var pagina2 = MotorConsultaBusqueda.Buscar(
            docs,
            permisos,
            new CriterioBusqueda(tenant, empresa, "", 2, pagina1.SiguienteCursorTitulo, pagina1.SiguienteCursorId),
            []);
        Assert.Single(pagina2.Documentos);
    }

    [Fact]
    public void Indexacion_ProyectaPedidoConfirmado()
    {
        var tenant = Guid.NewGuid();
        var empresa = Guid.NewGuid();
        var pedidoId = Guid.NewGuid();
        var payload = $$"""
            {"pedidoId":"{{pedidoId}}","referencia":"PED-100","clienteNombre":"ACME SAS"}
            """;
        var evento = new EventoParaIndexacion(
            Guid.NewGuid(), tenant, empresa, "Ventas",
            MotorEnrutamientoIntegracion.TipoPedidoConfirmado, pedidoId.ToString(), 1,
            DateTimeOffset.UtcNow, Guid.NewGuid(), false, payload);
        var (resultado, doc, _, _) = OrquestadorIndexacionBusqueda.Ejecutar(
            new HashSet<Guid>(), new MotorVersionIndice(), null, null, evento, DateTimeOffset.UtcNow);
        Assert.Equal(ResultadoIndexacion.Indexado, resultado);
        Assert.NotNull(doc);
        Assert.Equal(TiposEntidadIndexable.PedidoVenta, doc!.TipoEntidad);
        Assert.Equal(ProyectorEventosBusqueda.PermisoPedidoConsultar, doc.PermisoRequerido);
    }

    private static DocumentoIndice Documento(Guid tenant, Guid empresa, string permiso, string titulo = "Pedido demo") =>
        new(
            Guid.NewGuid(), tenant, empresa, TiposEntidadIndexable.PedidoVenta, Guid.NewGuid().ToString(), 1,
            titulo, null, titulo, permiso, "Ventas", Guid.NewGuid(), true, null, DateTimeOffset.UtcNow);
}
