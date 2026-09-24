using System.Net;
using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neros.ServiceDefaults;
using Xunit;

namespace Neros.Tests;

public sealed class FoundationHttpTests
{
    [Fact]
    public async Task ClienteReal_ReintentaConsultaPeroNoComando()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        var queries = 0;
        var commands = 0;
        await using var app = builder.Build();
        app.MapGet("/query", () =>
        {
            Interlocked.Increment(ref queries);
            return Microsoft.AspNetCore.Http.Results.StatusCode(503);
        });
        app.MapPost("/command", () =>
        {
            Interlocked.Increment(ref commands);
            return Microsoft.AspNetCore.Http.Results.StatusCode(503);
        });
        app.MapPost("/slow", async (Microsoft.AspNetCore.Http.HttpContext context) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(6), context.RequestAborted);
            return Microsoft.AspNetCore.Http.Results.Ok();
        });
        await app.StartAsync();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpClient("tested", client => client.BaseAddress = new Uri(app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses.Single())).AddNerosResilience();
        await using var provider = services.BuildServiceProvider();
        using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("tested");
        using var query = await client.GetAsync("/query");
        using var command = await client.PostAsync("/command", new StringContent("{}"));
        Assert.Equal(3, queries);
        Assert.Equal(1, commands);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, command.StatusCode);
        await Assert.ThrowsAsync<TaskCanceledException>(() => client.PostAsync("/slow", new StringContent("{}")));
        await app.StopAsync();
    }

    [Theory]
    [InlineData("not-a-correlation-id")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("c98fd381-90a5-4d7a-a8c7-d09e5843db7c")]
    public async Task HttpReal_SanitizaExcepcionYNormalizaCorrelacion(string supplied)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        using var logs = new CapturedLogs();
        builder.Logging.AddProvider(logs);
        builder.Services.AddNerosHttp();
        await using var app = builder.Build();
        app.UseNerosHttp();
        app.MapGet("/failure", (Func<string>)(() => throw new InvalidOperationException("detalle-privado-no-publicable")));
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses.Single()) };
        client.DefaultRequestHeaders.Add(FoundationHttp.CorrelationHeader, supplied);
        using var response = await client.GetAsync("/failure");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("detalle-privado", content);
        Assert.DoesNotContain("InvalidOperationException", content);
        using var json = JsonDocument.Parse(content);
        var correlation = response.Headers.GetValues(FoundationHttp.CorrelationHeader).Single();
        Assert.True(Guid.TryParseExact(correlation, "D", out var parsed) && parsed != Guid.Empty);
        Assert.Equal(correlation, json.RootElement.GetProperty("correlationId").GetString());
        Assert.Equal("unexpected_error", json.RootElement.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("traceId").GetString()));
        if (supplied.StartsWith("c98")) Assert.Equal(supplied, correlation);
        else Assert.NotEqual(supplied, correlation);
        using var htmlRequest = new HttpRequestMessage(HttpMethod.Get, "/failure");
        htmlRequest.Headers.Accept.ParseAdd("text/html");
        using var htmlResponse = await client.SendAsync(htmlRequest);
        Assert.Equal(HttpStatusCode.InternalServerError, htmlResponse.StatusCode);
        Assert.Equal("application/problem+json", htmlResponse.Content.Headers.ContentType?.MediaType);
        Assert.DoesNotContain("detalle-privado", await htmlResponse.Content.ReadAsStringAsync());
        await app.StopAsync();
        Assert.Contains(logs.Messages, message => message.Contains("unexpected_error", StringComparison.Ordinal));
        Assert.DoesNotContain(logs.Messages, message => message.Contains("detalle-privado", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(400, "validation_failed")]
    [InlineData(401, "authentication_required")]
    [InlineData(403, "permission_denied")]
    [InlineData(404, "resource_not_found")]
    [InlineData(409, "conflict")]
    [InlineData(412, "version_conflict")]
    [InlineData(429, "quota_exceeded")]
    [InlineData(503, "dependency_unavailable")]
    public async Task HttpReal_EstadoVacioTieneCodigoYCorrelacion(int status, string code)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddNerosHttp();
        await using var app = builder.Build();
        app.UseNerosHttp();
        app.MapGet("/status", () => Microsoft.AspNetCore.Http.Results.StatusCode(status));
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses.Single()) };
        using var response = await client.GetAsync("/status");
        Assert.Equal(status, (int)response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(code, json.RootElement.GetProperty("code").GetString());
        Assert.Equal(response.Headers.GetValues(FoundationHttp.CorrelationHeader).Single(),
            json.RootElement.GetProperty("correlationId").GetString());
        await app.StopAsync();
    }

    private sealed class CapturedLogs : ILoggerProvider
    {
        public ConcurrentQueue<string> Messages { get; } = new();
        public ILogger CreateLogger(string categoryName) => new CapturedLogger(Messages);
        public void Dispose() { }

        private sealed class CapturedLogger(ConcurrentQueue<string> messages) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter) => messages.Enqueue(formatter(state, exception) + exception);
        }
    }
}