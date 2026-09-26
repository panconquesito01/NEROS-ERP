using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Neros.Contracts.Busqueda;
using Neros.Contracts.Seguridad;
using Neros.Domain.Busqueda;
using Neros.Search.Api;
using Xunit;

namespace Neros.Tests;

public sealed class SearchApiTests
{
    private const string Issuer = "https://identity.example.test";

    [Fact]
    public async Task SearchApi_FiltraPorAclAsync()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexion, "busqueda");
        var tenant = Guid.NewGuid();
        var empresa = Guid.NewGuid();
        await InsertarDocumentoAsync(conexion, Guid.NewGuid(), tenant, empresa,
            ProyectorEventosBusqueda.PermisoPedidoConsultar, "Pedido Alpha");

        using var rsa = RSA.Create(2048);
        var signing = new RsaSecurityKey(rsa) { KeyId = "search-test" };
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Search"] = conexion,
            ["Identity:Authority"] = Issuer,
            ["Identity:BridgeSigningKeyPem"] = rsa.ExportPkcs8PrivateKeyPem()
        });
        builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Authority = null;
            options.ConfigurationManager = null;
            options.TokenValidationParameters.ValidIssuer = Issuer;
            options.TokenValidationParameters.ValidAudience = SearchHost.Audience;
            options.TokenValidationParameters.IssuerSigningKey = signing;
        });
        await using var app = SearchHost.Crear(builder);
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = GatewayOrganizationTests.Direccion(app) };

        var jwtSinVentas = Token(signing, tenant, empresa,
            PermisosBusqueda.Consultar, CodigosPermiso.ContabilidadComprobanteConsultar);
        using var req1 = new HttpRequestMessage(HttpMethod.Get, "/api/v1/search?q=Alpha");
        req1.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtSinVentas);
        var resp1 = await client.SendAsync(req1);
        Assert.Equal(HttpStatusCode.OK, resp1.StatusCode);
        var sinVentas = await resp1.Content.ReadFromJsonAsync<PaginaBusquedaDto>();
        Assert.NotNull(sinVentas);
        Assert.Empty(sinVentas!.Elementos);

        var jwtConVentas = Token(signing, tenant, empresa,
            PermisosBusqueda.Consultar, CodigosPermiso.VentasPedidoConsultar);
        using var req2 = new HttpRequestMessage(HttpMethod.Get, "/api/v1/search?q=Alpha");
        req2.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtConVentas);
        var conVentas = await (await client.SendAsync(req2)).Content.ReadFromJsonAsync<PaginaBusquedaDto>();
        Assert.Single(conVentas!.Elementos);

        await BaseDatosPruebas.EliminarAsync(conexion);
    }

    private static string Token(RsaSecurityKey signing, Guid tenant, Guid empresa, params string[] permisos)
    {
        var exp = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();
        var claims = new List<Claim>
        {
            new("sub", "test-actor"),
            new("tenant_id", tenant.ToString("D")),
            new("company_id", empresa.ToString("D")),
            new("exp", exp.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new("scope", "search.read")
        };
        claims.AddRange(permisos.Select(p => new Claim("permission", p)));
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            Issuer, SearchHost.Audience, claims,
            notBefore: DateTime.UtcNow.AddMinutes(-1), expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(signing, SecurityAlgorithms.RsaSha256)));
    }

    private static async Task InsertarDocumentoAsync(
        string conexion, Guid id, Guid tenant, Guid empresa, string permiso, string titulo)
    {
        await using var sql = new SqlConnection(conexion);
        await sql.OpenAsync();
        await using var cmd = sql.CreateCommand();
        cmd.CommandText = """
            INSERT INTO [busqueda].[DocumentoIndice]
                ([Id],[TenantId],[EmpresaId],[TipoEntidad],[EntidadId],[VersionIndice],[Titulo],[Resumen],[TextoBusqueda],
                 [PermisoRequerido],[OrigenModulo],[CorrelationId],[Activo])
            VALUES (@Id,@TenantId,@EmpresaId,@Tipo,@Entidad,1,@Titulo,NULL,@Titulo,@Permiso,'Ventas',@Corr,1);
            """;
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@TenantId", tenant);
        cmd.Parameters.AddWithValue("@EmpresaId", empresa);
        cmd.Parameters.AddWithValue("@Tipo", Neros.Domain.Busqueda.TiposEntidadIndexable.PedidoVenta);
        cmd.Parameters.AddWithValue("@Entidad", Guid.NewGuid().ToString());
        cmd.Parameters.AddWithValue("@Titulo", titulo);
        cmd.Parameters.AddWithValue("@Permiso", permiso);
        cmd.Parameters.AddWithValue("@Corr", Guid.NewGuid());
        await cmd.ExecuteNonQueryAsync();
    }
}
