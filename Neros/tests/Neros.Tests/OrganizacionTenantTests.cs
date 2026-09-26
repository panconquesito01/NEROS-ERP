using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Neros.Contracts.Autenticacion;
using Neros.Contracts.Organizacion;
using Neros.Organization.Api;
using Neros.Organization.Persistence;
using Neros.ServiceDefaults;
using Xunit;

namespace Neros.Tests;

public sealed class OrganizacionTenantTests(EntornoPruebas entorno) : IClassFixture<EntornoPruebas>
{
    [Fact]
    public async Task CorrespondenciaPublicaDevuelveTenantYRegionalAsync()
    {
        var correspondencia = await entorno.Http.GetFromJsonAsync<CorrespondenciaEmpresa>(
            $"api/v1/organization/correspondencia/{entorno.EmpresaPrimera:D}");
        Assert.NotNull(correspondencia);
        Assert.Equal(IdentidadPruebas.TenantNerosTest, correspondencia!.TenantId);
        Assert.Equal("NEROS-TEST", correspondencia.CodigoTenant);
        Assert.Equal("CO", correspondencia.Regional.Pais);
        Assert.Equal("COP", correspondencia.Regional.MonedaFuncional);
    }

    [Fact]
    public async Task PuenteSesionEmiteJwtYOrganizationDevuelveContextoConTenantAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        var acceso = await entorno.EntrarAsync(cuenta);
        using var cliente = Cliente(acceso);
        using var peticion = new HttpRequestMessage(HttpMethod.Get, "api/v1/organization/context");
        peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", acceso.Token);
        peticion.Headers.Add(CabecerasCliente.Empresa, entorno.EmpresaPrimera.ToString("D"));
        using var respuesta = await cliente.SendAsync(peticion);
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var contexto = await respuesta.Content.ReadFromJsonAsync<ContextoAutorizado>();
        Assert.NotNull(contexto);
        Assert.Equal(cuenta.Id, contexto!.ActorId);
        Assert.Equal(IdentidadPruebas.TenantNerosTest, contexto.TenantId);
        Assert.Equal(entorno.EmpresaPrimera, contexto.CompanyId);
        Assert.Equal("America/Bogota", contexto.Regional!.ZonaHoraria);
    }

    [Fact]
    public async Task PuenteRechazaEmpresaSinCorrespondenciaAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        var acceso = await entorno.EntrarAsync(cuenta);
        using var cliente = Cliente(acceso);
        using var peticion = new HttpRequestMessage(HttpMethod.Get, "api/v1/organization/context");
        peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", acceso.Token);
        peticion.Headers.Add(CabecerasCliente.Empresa, entorno.EmpresaAjena.ToString("D"));
        using var respuesta = await cliente.SendAsync(peticion);
        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    private HttpClient Cliente(AccesoConcedido acceso)
    {
        var cliente = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = entorno.Http.BaseAddress };
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", acceso.Token);
        return cliente;
    }
}
