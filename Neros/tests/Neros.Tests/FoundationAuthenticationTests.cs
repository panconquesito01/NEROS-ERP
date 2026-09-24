using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Neros.ServiceDefaults;
using Xunit;

namespace Neros.Tests;

public sealed class FoundationAuthenticationTests
{
    [Fact]
    public async Task HttpReal_ValidaFirmaEmisorAudienciaVigenciaAmbitoYPermiso()
    {
        using var rsa = RSA.Create(2048);
        using var foreignRsa = RSA.Create(2048);
        var signing = new RsaSecurityKey(rsa) { KeyId = "ephemeral-test" };
        var tenant = Guid.NewGuid();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddNerosHttp();
        builder.Services.AddNerosServiceAuthentication(new Uri("https://identity.example.test"), "organization");
        builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Authority = null;
            options.ConfigurationManager = null;
            options.TokenValidationParameters.ValidIssuer = "https://identity.example.test";
            options.TokenValidationParameters.ValidAudience = "organization";
            options.TokenValidationParameters.IssuerSigningKey = signing;
        });
        builder.Services.AddAuthorization(options => options.AddPolicy("read", policy =>
            policy.RequireNerosPermission("organization.read", "Organization.Company.Read")));
        await using var app = builder.Build();
        app.UseNerosHttp();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapNerosHealth();
        app.MapGet("/api/v1/tenants/{tenantId:guid}", (Guid tenantId, HttpContext http) =>
        {
            if (!SecurityContext.TryFromAuthenticatedPrincipal(http.User, TimeProvider.System, out var security)
                || !security!.Allows("Organization.Company.Read", new TenantContext(tenantId), TimeProvider.System))
                return Results.NotFound();
            return Results.Ok(new { tenantId });
        }).RequireAuthorization("read");
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses.Single()) };

        string Token(string issuer = "https://identity.example.test", string audience = "organization", bool expired = false,
            bool permitted = true, bool scoped = true, SecurityKey? key = null)
        {
            var claims = new List<Claim> { new("sub", "test-user"), new("tenant_id", tenant.ToString("D")) };
            if (permitted) claims.Add(new("permission", "Organization.Company.Read"));
            if (scoped) claims.Add(new("scope", "organization.read"));
            return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(issuer, audience, claims,
                notBefore: DateTime.UtcNow.AddMinutes(-10), expires: DateTime.UtcNow.AddMinutes(expired ? -1 : 5),
                signingCredentials: new SigningCredentials(key ?? signing, SecurityAlgorithms.RsaSha256)));
        }

        async Task<HttpStatusCode> Request(string path, string? token = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Add("X-Tenant-Id", tenant.ToString("D"));
            request.Headers.Add("X-Actor-Id", "platform-admin");
            if (token is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var response = await client.SendAsync(request);
            return response.StatusCode;
        }

        var resource = $"/api/v1/tenants/{tenant:D}";
        Assert.Equal(HttpStatusCode.OK, await Request("/health/live"));
        Assert.Equal(HttpStatusCode.Unauthorized, await Request("/health/ready"));
        Assert.Equal(HttpStatusCode.Unauthorized, await Request(resource));
        Assert.Equal(HttpStatusCode.OK, await Request(resource, Token()));
        Assert.Equal(HttpStatusCode.Forbidden, await Request("/health/ready", Token()));
        Assert.Equal(HttpStatusCode.NotFound, await Request($"/api/v1/tenants/{Guid.NewGuid():D}", Token()));
        Assert.Equal(HttpStatusCode.Unauthorized, await Request(resource, Token(issuer: "https://foreign.example.test")));
        Assert.Equal(HttpStatusCode.Unauthorized, await Request(resource, Token(audience: "finance")));
        Assert.Equal(HttpStatusCode.Unauthorized, await Request(resource, Token(expired: true)));
        Assert.Equal(HttpStatusCode.Unauthorized, await Request(resource, Token(key: new RsaSecurityKey(foreignRsa))));
        Assert.Equal(HttpStatusCode.Forbidden, await Request(resource, Token(permitted: false)));
        Assert.Equal(HttpStatusCode.Forbidden, await Request(resource, Token(scoped: false)));
        await app.StopAsync();
    }
}