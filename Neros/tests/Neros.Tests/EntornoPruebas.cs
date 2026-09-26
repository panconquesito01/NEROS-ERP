using System.Net.Http.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neros.Contracts.Autenticacion;
using Neros.Contracts.Privacidad;
using Neros.Persistence;
using Neros.Persistence.Seguridad;
using Xunit;
using Microsoft.AspNetCore.Builder;
using Neros.Gateway;
using Neros.Organization.Api;
using Neros.Organization.Persistence;
using Neros.Terceros.Api;

namespace Neros.Tests;

public sealed class EntornoPruebas : IAsyncLifetime
{
    public FabricaApi Api { get; } = new();
    public string ConexionOrganizacion { get; } = BaseDatosPruebas.NuevaConexion();
    public string ConexionTerceros { get; } = BaseDatosPruebas.NuevaConexion();
    public WebApplication Organization { get; private set; } = null!;
    public WebApplication Terceros { get; private set; } = null!;
    public FabricaBlazor Blazor { get; private set; } = null!;
    public WebApplication Gateway { get; private set; } = null!;
    public HttpClient Http { get; private set; } = null!;
    public HttpClient Web { get; private set; } = null!;
    public Guid EmpresaPrimera { get; } = Guid.NewGuid();
    public Guid EmpresaSegunda { get; } = Guid.NewGuid();
    public Guid EmpresaAjena { get; } = Guid.NewGuid();

    public async Task InitializeAsync()
    {
        await BaseDatosPruebas.DesplegarCompatibilidadPrivacidadYGlobalizacionAsync(Api.Conexion);
        await BaseDatosPruebas.DesplegarAsync(ConexionOrganizacion, "organizacion");
        await BaseDatosPruebas.DesplegarAsync(ConexionTerceros, "terceros");
        Api.ConexionOrganizacion = ConexionOrganizacion;
        Api.UseKestrel(0);
        Api.StartServer();
        var direccionApi = Api.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        var organizationBuilder = GatewayOrganizationTests.CrearBuilder();
        organizationBuilder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Organization"] = ConexionOrganizacion,
            ["Identity:Authority"] = IdentidadPruebas.Emisor,
            ["Identity:BridgeSigningKeyPem"] = IdentidadPruebas.BridgeSigningKeyPem
        });
        Organization = OrganizationHost.Crear(organizationBuilder);
        await Organization.StartAsync();
        var direccionOrganization = Organization.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        var tercerosBuilder = GatewayOrganizationTests.CrearBuilder();
        tercerosBuilder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Terceros"] = ConexionTerceros,
            ["Identity:Authority"] = IdentidadPruebas.Emisor,
            ["Identity:BridgeSigningKeyPem"] = IdentidadPruebas.BridgeSigningKeyPem
        });
        Terceros = TercerosHost.Crear(tercerosBuilder);
        await Terceros.StartAsync();
        var direccionTerceros = Terceros.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        var gatewayBuilder = GatewayOrganizationTests.CrearBuilder();
        gatewayBuilder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Services:Compatibility"] = direccionApi,
            ["Services:Organization"] = direccionOrganization,
            ["Services:Terceros"] = direccionTerceros,
            ["Services:Search"] = "http://127.0.0.1:1/",
            ["Identity:Authority"] = IdentidadPruebas.Emisor,
            ["Identity:BridgeSigningKeyPem"] = IdentidadPruebas.BridgeSigningKeyPem
        });
        Gateway = GatewayHost.Crear(gatewayBuilder);
        await Gateway.StartAsync();
        Http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = GatewayOrganizationTests.Direccion(Gateway) };
        await using var scope = Api.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<NerosDbContext>();
        database.Empresas.AddRange(
            new Empresa { Id = EmpresaPrimera, Codigo = "ND", Nombre = "Neros Distribucion (prueba)", Identificacion = "PRUEBA-001" },
            new Empresa { Id = EmpresaSegunda, Codigo = "NS", Nombre = "Neros Servicios (prueba)", Identificacion = "PRUEBA-002" },
            new Empresa { Id = EmpresaAjena, Codigo = "NA", Nombre = "Empresa ajena (prueba)", Identificacion = "PRUEBA-003" });
        await database.SaveChangesAsync();
        await SincronizarOrganizacionAsync(database);
        Blazor = new FabricaBlazor(Http.BaseAddress!);
        Blazor.UseKestrel(0);
        Blazor.StartServer();
        var direccionWeb = Blazor.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        Web = Blazor.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, BaseAddress = new Uri(direccionWeb) });
    }

    public async Task<CuentaPrueba> CrearCuentaAsync()
    {
        await using var scope = Api.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<NerosDbContext>();
        var usuarios = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        var correo = $"prueba-{Guid.NewGuid():N}@example.test";
        var clave = $"Aa1!{Convert.ToHexString(RandomNumberGenerator.GetBytes(24))}";
        var usuario = new Usuario { UserName = correo, Email = correo, Nombre = "Ana (prueba)", EmailConfirmed = true };
        var resultado = await usuarios.CreateAsync(usuario, clave);
        Assert.True(resultado.Succeeded);
        database.UsuariosEmpresas.AddRange(
            new UsuarioEmpresa { UsuarioId = usuario.Id, EmpresaId = EmpresaPrimera, Rol = "Administrador" },
            new UsuarioEmpresa { UsuarioId = usuario.Id, EmpresaId = EmpresaSegunda, Rol = "Consulta" });
        await database.SaveChangesAsync();
        return new CuentaPrueba(usuario.Id, correo, clave);
    }

    public async Task<AccesoConcedido> EntrarAsync(CuentaPrueba cuenta, bool persistente = false, bool aceptarLegales = true)
    {
        using var respuesta = await Http.PostAsJsonAsync("api/acceso/login", new SolicitudAcceso
        {
            Correo = cuenta.Correo, Clave = cuenta.Clave, Recordarme = persistente
        });
        Assert.True(respuesta.IsSuccessStatusCode, $"Login: {(int)respuesta.StatusCode}");
        var acceso = (await respuesta.Content.ReadFromJsonAsync<AccesoConcedido>())!;
        if (aceptarLegales && acceso.Usuario.DocumentosLegalesPendientes is { Count: > 0 })
        {
            await AceptarLegalesAsync(acceso);
        }
        return acceso;
    }

    public async Task AceptarLegalesAsync(AccesoConcedido acceso)
    {
        using var cliente = ClienteAutorizado(acceso);
        var pendientes = await cliente.GetFromJsonAsync<List<DocumentoLegalPendiente>>("api/privacidad/documentos/pendientes");
        foreach (var pendiente in pendientes ?? [])
        {
            using var respuesta = await cliente.PostAsJsonAsync($"api/privacidad/documentos/{pendiente.Codigo}/aceptar",
                new SolicitudAceptacionLegal(pendiente.VersionId, pendiente.HashContenido));
            respuesta.EnsureSuccessStatusCode();
        }
    }

    private HttpClient ClienteAutorizado(AccesoConcedido acceso)
    {
        var cliente = Api.CreateClient();
        cliente.BaseAddress = Http.BaseAddress;
        cliente.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", acceso.Token);
        return cliente;
    }

    public async Task DisposeAsync()
    {
        Web?.Dispose();
        if (Blazor is not null) await Blazor.DisposeAsync();
        Http?.Dispose();
        if (Gateway is not null)
        {
            await Gateway.StopAsync();
            await Gateway.DisposeAsync();
        }
        if (Organization is not null)
        {
            await Organization.StopAsync();
            await Organization.DisposeAsync();
        }
        if (Terceros is not null)
        {
            await Terceros.StopAsync();
            await Terceros.DisposeAsync();
        }
        await Api.DisposeAsync();
        await BaseDatosPruebas.EliminarAsync(Api.Conexion);
        await BaseDatosPruebas.EliminarAsync(ConexionOrganizacion);
        await BaseDatosPruebas.EliminarAsync(ConexionTerceros);
    }

    private async Task SincronizarOrganizacionAsync(NerosDbContext compatibilidad)
    {
        await using var scope = Organization.Services.CreateAsyncScope();
        var servicio = scope.ServiceProvider.GetRequiredService<ServicioCorrespondencia>();
        var empresas = await compatibilidad.Empresas.AsNoTracking()
            .Where(empresa => empresa.Id == EmpresaPrimera || empresa.Id == EmpresaSegunda).ToListAsync();
        foreach (var empresa in empresas)
        {
            await servicio.UpsertEmpresaAsync(IdentidadPruebas.TenantNerosTest, new EmpresaOrganizacion
            {
                Id = empresa.Id,
                Codigo = empresa.Codigo,
                Nombre = empresa.Nombre,
                Identificacion = empresa.Identificacion,
                Activa = empresa.Activa,
                Pais = "CO",
                MonedaFuncional = "COP",
                ZonaHoraria = "America/Bogota",
                CulturaFormato = "es-CO",
                MarcoContable = "Local"
            }, CancellationToken.None);
        }
    }
}

public sealed record CuentaPrueba(string Id, string Correo, string Clave);

public sealed class FabricaApi : WebApplicationFactory<Neros.Api.Controllers.AccesoController>
{
    public string Conexion { get; } = BaseDatosPruebas.NuevaConexion();
    public string? ConexionOrganizacion { get; set; }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var valores = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Neros"] = Conexion,
            ["ConnectionStrings:Globalizacion"] = Conexion,
            ["TasasCambio:Automatico"] = "false",
            ["TasasCambio:SincronizarAlConsultarSiFaltaHoy"] = "false"
        };
        if (!string.IsNullOrWhiteSpace(ConexionOrganizacion))
            valores["ConnectionStrings:Organization"] = ConexionOrganizacion;
        builder.ConfigureHostConfiguration(configuration => configuration.AddInMemoryCollection(valores));
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseEnvironment("Development").ConfigureLogging(logging => logging.ClearProviders());
}

public sealed class FabricaBlazor(Uri api) : WebApplicationFactory<Neros.Blazor.Components.App>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(configuration => configuration.AddInMemoryCollection(
            new Dictionary<string, string?> { ["Gateway:BaseUrl"] = api.ToString() }));
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseEnvironment("Development").ConfigureLogging(logging => logging.ClearProviders());
}