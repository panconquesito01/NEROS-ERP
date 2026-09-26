using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Neros.Organization.Api;
using Neros.Gateway;
using Neros.ServiceDefaults;
using Microsoft.AspNetCore.Http;
using Xunit;
using System.Collections.Concurrent;
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace Neros.Tests;

public sealed class GatewayOrganizationTests
{
    private const string Issuer = "https://identity.example.test";

    [Fact]
    public async Task OrganizationReal_ExigeContextoFirmadoYSinHeadersDeIdentidad()
    {
        var conexionOrganizacion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexionOrganizacion, "organizacion");
        using var rsa = RSA.Create(2048);
        var signing = new RsaSecurityKey(rsa) { KeyId = "organization-test" };
        var builder = CrearBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Organization"] = conexionOrganizacion
        });
        ConfigurarFirma(builder, signing);
        await using var app = OrganizationHost.Crear(builder);
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = Direccion(app) };
        using var anonymous = await client.GetAsync("/api/v1/organization/context");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        var tenant = Guid.NewGuid();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/organization/context");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token(signing, tenant));
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var context = await response.Content.ReadFromJsonAsync<ContextoAutorizado>();
        Assert.Equal(tenant, context!.TenantId);
        Assert.Equal("test-actor", context.ActorId);
        using var forged = new HttpRequestMessage(HttpMethod.Get, "/api/v1/organization/context");
        forged.Headers.Authorization = request.Headers.Authorization;
        forged.Headers.Add("X-Tenant-Id", Guid.NewGuid().ToString("D"));
        using var rejected = await client.SendAsync(forged);
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        await app.StopAsync();
        await BaseDatosPruebas.EliminarAsync(conexionOrganizacion);
    }

    [Fact]
    public async Task GatewayReal_ConservaTokenYCorrelacionSinConfiarEnHeaders()
    {
        var conexionOrganizacion = BaseDatosPruebas.NuevaConexion();
        await BaseDatosPruebas.DesplegarAsync(conexionOrganizacion, "organizacion");
        var spans = new ConcurrentBag<FoundationTelemetryTests.SpanEvidence>();
        using var rsa = RSA.Create(2048);
        var signing = new RsaSecurityKey(rsa) { KeyId = "gateway-test" };
        var organizationBuilder = CrearBuilder();
        organizationBuilder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Organization"] = conexionOrganizacion
        });
        ConfigurarFirma(organizationBuilder, signing);
        organizationBuilder.Services.AddOpenTelemetry().WithTracing(tracing => tracing.AddProcessor(
            new SimpleActivityExportProcessor(new FoundationTelemetryTests.EvidenceExporter(spans))));
        await using var organization = OrganizationHost.Crear(organizationBuilder);
        await organization.StartAsync();
        await using var legacy = CrearBuilder().Build();
        legacy.MapGet("/api/acceso/inspect", (HttpContext context) => Results.Ok(new
        {
            tenantHeader = context.Request.Headers.ContainsKey("X-Tenant-Id"),
            cookie = context.Request.Headers.ContainsKey("Cookie"),
            authorization = context.Request.Headers.Authorization.ToString(),
            correlation = context.Request.Headers[FoundationHttp.CorrelationHeader].ToString()
        }));
        await legacy.StartAsync();
        var gatewayBuilder = CrearBuilder();
        ConfigurarFirma(gatewayBuilder, signing);
        gatewayBuilder.Services.AddOpenTelemetry().WithTracing(tracing => tracing.AddProcessor(
            new SimpleActivityExportProcessor(new FoundationTelemetryTests.EvidenceExporter(spans))));
        gatewayBuilder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Services:Organization"] = Direccion(organization).ToString(),
            ["Services:Compatibility"] = Direccion(legacy).ToString(),
            ["Services:Terceros"] = "http://127.0.0.1:1/"
        });
        await using var gateway = GatewayHost.Crear(gatewayBuilder);
        await gateway.StartAsync();
        using var client = new HttpClient { BaseAddress = Direccion(gateway) };
        using var live = await client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, live.StatusCode);
        using var readiness = await client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.Unauthorized, readiness.StatusCode);
        var tenant = Guid.NewGuid();
        var correlation = Guid.NewGuid().ToString("D");
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/organization/context");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token(signing, tenant));
        request.Headers.Add("X-Tenant-Id", Guid.NewGuid().ToString("D"));
        request.Headers.Add(FoundationHttp.CorrelationHeader, correlation);
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(tenant, (await response.Content.ReadFromJsonAsync<ContextoAutorizado>())!.TenantId);
        Assert.Equal(correlation, response.Headers.GetValues(FoundationHttp.CorrelationHeader).Single());
        using var businessReadiness = new HttpRequestMessage(HttpMethod.Get, "/health/ready");
        businessReadiness.Headers.Authorization = request.Headers.Authorization;
        using var forbiddenReadiness = await client.SendAsync(businessReadiness);
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenReadiness.StatusCode);
        using var compatibility = new HttpRequestMessage(HttpMethod.Get, "/api/acceso/inspect");
        compatibility.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "opaque-test-only");
        compatibility.Headers.Add("X-Tenant-Id", tenant.ToString("D"));
        compatibility.Headers.Add("Cookie", "test=not-forwarded");
        compatibility.Headers.Add(FoundationHttp.CorrelationHeader, correlation);
        using var forwarded = await client.SendAsync(compatibility);
        Assert.Equal(HttpStatusCode.OK, forwarded.StatusCode);
        using var json = System.Text.Json.JsonDocument.Parse(await forwarded.Content.ReadAsStringAsync());
        Assert.False(json.RootElement.GetProperty("tenantHeader").GetBoolean());
        Assert.False(json.RootElement.GetProperty("cookie").GetBoolean());
        Assert.Equal("Bearer opaque-test-only", json.RootElement.GetProperty("authorization").GetString());
        Assert.Equal(correlation, json.RootElement.GetProperty("correlation").GetString());
        using var anonymous = await client.GetAsync("/api/v1/organization/context");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        foreach (var token in new[] { Token(signing, tenant, audience: "finance"), Token(signing, tenant, permitted: false) })
        {
            using var invalid = new HttpRequestMessage(HttpMethod.Get, "/api/v1/organization/context");
            invalid.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var rejected = await client.SendAsync(invalid);
            Assert.True(rejected.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
        }
        await organization.StopAsync();
        using var unavailableRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/organization/context");
        unavailableRequest.Headers.Authorization = request.Headers.Authorization;
        using var unavailable = await client.SendAsync(unavailableRequest);
        Assert.Equal(HttpStatusCode.BadGateway, unavailable.StatusCode);
        Assert.Equal("application/problem+json", unavailable.Content.Headers.ContentType?.MediaType);
        Assert.DoesNotContain("Exception", await unavailable.Content.ReadAsStringAsync());
        await gateway.StopAsync();
        await legacy.StopAsync();
        gateway.Services.GetRequiredService<TracerProvider>().ForceFlush();
        organization.Services.GetRequiredService<TracerProvider>().ForceFlush();
        var linked = spans.Where(span => span.Kind == ActivityKind.Server && span.CorrelationId == correlation)
            .GroupBy(span => span.TraceId).FirstOrDefault(group => group.Select(span => span.SpanId).Distinct().Count() >= 2);
        Assert.NotNull(linked);
        Assert.Contains(spans, span => span.TraceId == linked.Key && span.Kind == ActivityKind.Client);
        await BaseDatosPruebas.EliminarAsync(conexionOrganizacion);
    }

    [Fact]
    public async Task GatewayReal_IdentityIndisponibleNoAfectaCompatibilidadNiAutorizaOrganization()
    {
        await using var legacy = CrearBuilder().Build();
        legacy.MapGet("/api/acceso/inspect", () => Results.Ok());
        await legacy.StartAsync();
        using var identity = new IdentityIndisponible();
        var builder = CrearBuilder();
        builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Backchannel = new HttpClient(identity) { Timeout = TimeSpan.FromSeconds(2) };
        });
        ConfigurarDestinos(builder, Direccion(legacy));
        await using var gateway = GatewayHost.Crear(builder);
        await gateway.StartAsync();
        using var client = new HttpClient { BaseAddress = Direccion(gateway) };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "opaque-test-only");
        using var compatible = await client.GetAsync("/api/acceso/inspect");
        Assert.Equal(HttpStatusCode.OK, compatible.StatusCode);
        Assert.Equal(0, identity.Requests);
        using var rsa = RSA.Create(2048);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token(new RsaSecurityKey(rsa), Guid.NewGuid()));
        using var unavailable = await client.GetAsync("/api/v1/organization/context");
        Assert.Equal(HttpStatusCode.Unauthorized, unavailable.StatusCode);
        Assert.True(identity.Requests > 0);
        await gateway.StopAsync();
        await legacy.StopAsync();
    }

    [Fact]
    public async Task GatewayReal_RechazaExcesoDeConcurrenciaSinCola()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var requests = 0;
        await using var legacy = CrearBuilder().Build();
        legacy.MapGet("/api/acceso/wait", async (HttpContext context) =>
        {
            if (Interlocked.Increment(ref requests) == 32) entered.TrySetResult();
            await release.Task.WaitAsync(context.RequestAborted);
            return Results.Ok();
        });
        await legacy.StartAsync();
        var builder = CrearBuilder();
        ConfigurarDestinos(builder, Direccion(legacy));
        await using var gateway = GatewayHost.Crear(builder);
        await gateway.StartAsync();
        using var client = new HttpClient { BaseAddress = Direccion(gateway), Timeout = TimeSpan.FromSeconds(15) };
        var pending = Enumerable.Range(0, 32).Select(_ => client.GetAsync("/api/acceso/wait")).ToArray();
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            using var excess = await client.GetAsync("/api/acceso/wait");
            Assert.Equal(HttpStatusCode.TooManyRequests, excess.StatusCode);
        }
        finally
        {
            release.TrySetResult();
            foreach (var response in await Task.WhenAll(pending)) response.Dispose();
        }
        await gateway.StopAsync();
        await legacy.StopAsync();
    }

    private sealed class IdentityIndisponible : HttpMessageHandler
    {
        private int requests;
        public int Requests => Volatile.Read(ref requests);
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref requests);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        }
    }

    private static void ConfigurarDestinos(WebApplicationBuilder builder, Uri legacy) =>
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Services:Compatibility"] = legacy.ToString(),
            ["Services:Organization"] = "http://127.0.0.1:1/",
            ["Services:Terceros"] = "http://127.0.0.1:1/",
            ["Services:Search"] = "http://127.0.0.1:1/"
        });

    internal static WebApplicationBuilder CrearBuilder()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["Identity:Authority"] = Issuer });
        return builder;
    }

    private static void ConfigurarFirma(WebApplicationBuilder builder, SecurityKey signing) =>
        builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Authority = null;
            options.ConfigurationManager = null;
            options.TokenValidationParameters.ValidIssuer = Issuer;
            options.TokenValidationParameters.ValidAudience = OrganizationHost.Audience;
            options.TokenValidationParameters.IssuerSigningKey = signing;
        });

    private static string Token(SecurityKey signing, Guid tenant, string audience = OrganizationHost.Audience,
        bool permitted = true) => new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(Issuer, audience,
            new Claim[] { new("sub", "test-actor"), new("tenant_id", tenant.ToString("D")), new("scope", "organization.read"),
                new("permission", permitted ? OrganizationHost.ContextPermission : "Organization.Other.Read") },
            notBefore: DateTime.UtcNow.AddMinutes(-1), expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(signing, SecurityAlgorithms.RsaSha256)));

    internal static Uri Direccion(WebApplication app) => new(app.Services.GetRequiredService<IServer>()
        .Features.Get<IServerAddressesFeature>()!.Addresses.Single());
}