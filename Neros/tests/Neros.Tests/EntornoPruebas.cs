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
using Neros.Persistence;
using Neros.Persistence.Seguridad;
using Xunit;
using Microsoft.AspNetCore.Builder;
using Neros.Gateway;

namespace Neros.Tests;

public sealed class EntornoPruebas : IAsyncLifetime
{
    public FabricaApi Api { get; } = new();
    public FabricaBlazor Blazor { get; private set; } = null!;
    public WebApplication Gateway { get; private set; } = null!;
    public HttpClient Http { get; private set; } = null!;
    public HttpClient Web { get; private set; } = null!;
    public Guid EmpresaPrimera { get; } = Guid.NewGuid();
    public Guid EmpresaSegunda { get; } = Guid.NewGuid();
    public Guid EmpresaAjena { get; } = Guid.NewGuid();

    public async Task InitializeAsync()
    {
        Api.UseKestrel(0);
        Api.StartServer();
        var direccionApi = Api.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        var gatewayBuilder = GatewayOrganizationTests.CrearBuilder();
        gatewayBuilder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Services:Compatibility"] = direccionApi,
            ["Services:Organization"] = "http://127.0.0.1:1/"
        });
        Gateway = GatewayHost.Crear(gatewayBuilder);
        await Gateway.StartAsync();
        Http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = GatewayOrganizationTests.Direccion(Gateway) };
        await using var scope = Api.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<NerosDbContext>();
        await database.Database.EnsureCreatedAsync();
        database.Empresas.AddRange(
            new Empresa { Id = EmpresaPrimera, Codigo = "ND", Nombre = "Neros Distribucion (prueba)", Identificacion = "PRUEBA-001" },
            new Empresa { Id = EmpresaSegunda, Codigo = "NS", Nombre = "Neros Servicios (prueba)", Identificacion = "PRUEBA-002" },
            new Empresa { Id = EmpresaAjena, Codigo = "NA", Nombre = "Empresa ajena (prueba)", Identificacion = "PRUEBA-003" });
        await database.SaveChangesAsync();
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

    public async Task<AccesoConcedido> EntrarAsync(CuentaPrueba cuenta, bool persistente = false)
    {
        using var respuesta = await Http.PostAsJsonAsync("api/acceso/login", new SolicitudAcceso
        {
            Correo = cuenta.Correo, Clave = cuenta.Clave, Recordarme = persistente
        });
        Assert.True(respuesta.IsSuccessStatusCode, $"Login: {(int)respuesta.StatusCode}");
        return (await respuesta.Content.ReadFromJsonAsync<AccesoConcedido>())!;
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
        await using (var scope = Api.Services.CreateAsyncScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<NerosDbContext>();
            Assert.StartsWith("NerosTests_", database.Database.GetDbConnection().Database);
            await database.Database.EnsureDeletedAsync();
        }
        await Api.DisposeAsync();
    }
}

public sealed record CuentaPrueba(string Id, string Correo, string Clave);

public sealed class FabricaApi : WebApplicationFactory<Neros.Api.Controllers.AccesoController>
{
    private readonly string conexion = new SqlConnectionStringBuilder
    {
        DataSource = Environment.GetEnvironmentVariable("NEROS_TEST_SERVER") ?? "localhost",
        InitialCatalog = $"NerosTests_{Guid.NewGuid():N}",
        IntegratedSecurity = true, Encrypt = true, TrustServerCertificate = true
    }.ConnectionString;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(configuration => configuration.AddInMemoryCollection(
            new Dictionary<string, string?> { ["ConnectionStrings:Neros"] = conexion }));
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