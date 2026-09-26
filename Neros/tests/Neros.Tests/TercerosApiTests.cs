using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Neros.Contracts.Autenticacion;
using Neros.Contracts.Terceros;
using Neros.Domain.Terceros;
using Xunit;

namespace Neros.Tests;

public sealed class TercerosApiTests(EntornoPruebas entorno) : IClassFixture<EntornoPruebas>
{
    [Fact]
    public async Task CrudBusquedaHistorialYValidacionIdentificacionAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        var acceso = await entorno.EntrarAsync(cuenta);
        using var cliente = Cliente(acceso);
        var nitBase = "890903938";
        var dv = ValidadorIdentificacionColombia.CalcularDigitoVerificacionNit(nitBase);
        var solicitud = new SolicitudCrearTercero(
            "Organizacion", "Proveedor Demo SAS", "Demo",
            [new IdentificacionTercero("CO", "NIT", nitBase, dv, true)],
            ["Proveedor", "Cliente"]);
        using var crear = await cliente.PostAsJsonAsync("api/v1/terceros", solicitud);
        var cuerpoError = await crear.Content.ReadAsStringAsync();
        Assert.True(crear.StatusCode == HttpStatusCode.Created, cuerpoError);
        var creado = await crear.Content.ReadFromJsonAsync<TerceroDetalle>();
        Assert.NotNull(creado);
        using var buscar = await cliente.GetAsync("api/v1/terceros?q=Demo");
        Assert.Equal(HttpStatusCode.OK, buscar.StatusCode);
        var pagina = await buscar.Content.ReadFromJsonAsync<PaginaTerceros>();
        Assert.Contains(pagina!.Elementos, t => t.Id == creado!.Id);
        var actualizar = new SolicitudActualizarTercero(
            "Proveedor Demo SAS Actualizado", "Demo", true,
            [new IdentificacionTercero("CO", "NIT", nitBase, dv, true)],
            ["Proveedor"]);
        using var put = await cliente.PutAsJsonAsync($"api/v1/terceros/{creado!.Id:D}", actualizar);
        Assert.True(put.StatusCode == HttpStatusCode.OK, await put.Content.ReadAsStringAsync());
        using var historial = await cliente.GetAsync($"api/v1/terceros/{creado.Id:D}/historial");
        Assert.Equal(HttpStatusCode.OK, historial.StatusCode);
        var versiones = await historial.Content.ReadFromJsonAsync<List<VersionHistoricaTercero>>();
        Assert.True(versiones!.Count >= 2);
        Assert.Contains(versiones, v => v.RazonSocial.Contains("Actualizado"));
        Assert.Contains(versiones, v => v.RazonSocial == "Proveedor Demo SAS");
        var invalida = solicitud with
        {
            RazonSocial = "Otro",
            Identificaciones = [new IdentificacionTercero("CO", "NIT", "123", '0', true)]
        };
        using var rechazo = await cliente.PostAsJsonAsync("api/v1/terceros", invalida);
        Assert.Equal(HttpStatusCode.BadRequest, rechazo.StatusCode);
    }

    [Fact]
    public async Task ConsultaSinPermisoEscrituraRecibe403AlCrearAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        var acceso = await entorno.EntrarAsync(cuenta);
        using var cliente = Cliente(acceso, entorno.EmpresaSegunda);
        var solicitud = new SolicitudCrearTercero(
            "Persona", "Consulta Solo", null,
            [new IdentificacionTercero("CO", "CC", "1234567890", null, true)],
            ["Cliente"]);
        using var crear = await cliente.PostAsJsonAsync("api/v1/terceros", solicitud);
        Assert.Equal(HttpStatusCode.Forbidden, crear.StatusCode);
        using var listar = await cliente.GetAsync("api/v1/terceros");
        Assert.Equal(HttpStatusCode.OK, listar.StatusCode);
    }

    private HttpClient Cliente(AccesoConcedido acceso, Guid? empresaId = null)
    {
        var cliente = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = entorno.Http.BaseAddress };
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", acceso.Token);
        cliente.DefaultRequestHeaders.Add(CabecerasCliente.Empresa, (empresaId ?? entorno.EmpresaPrimera).ToString("D"));
        return cliente;
    }
}
