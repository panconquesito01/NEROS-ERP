using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neros.ServiceDefaults;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Xunit;

namespace Neros.Tests;

public sealed class FoundationTelemetryTests
{
    [Fact]
    public async Task DosHostsHttp_PropaganCorrelacionYExportanMismaTraza()
    {
        var spans = new ConcurrentBag<SpanEvidence>();
        var downstreamBuilder = WebApplication.CreateBuilder();
        downstreamBuilder.WebHost.UseUrls("http://127.0.0.1:0");
        downstreamBuilder.Logging.ClearProviders();
        downstreamBuilder.Services.AddNerosHttp();
        downstreamBuilder.AddNerosTelemetry("test-downstream");
        downstreamBuilder.Services.AddOpenTelemetry().WithTracing(tracing =>
            tracing.AddProcessor(new SimpleActivityExportProcessor(new EvidenceExporter(spans))));
        await using var downstream = downstreamBuilder.Build();
        downstream.UseNerosHttp();
        downstream.MapGet("/observed", (HttpContext context) => Results.Json(new
        {
            correlationId = FoundationHttp.GetCorrelationId(context),
            traceId = Activity.Current!.TraceId.ToString()
        }));
        await downstream.StartAsync();

        var upstreamBuilder = WebApplication.CreateBuilder();
        upstreamBuilder.WebHost.UseUrls("http://127.0.0.1:0");
        upstreamBuilder.Logging.ClearProviders();
        upstreamBuilder.Services.AddNerosHttp();
        upstreamBuilder.AddNerosTelemetry("test-upstream");
        upstreamBuilder.Services.AddOpenTelemetry().WithTracing(tracing =>
            tracing.AddProcessor(new SimpleActivityExportProcessor(new EvidenceExporter(spans))));
        upstreamBuilder.Services.AddHttpClient("downstream", client => client.BaseAddress = Address(downstream)).AddNerosResilience();
        await using var upstream = upstreamBuilder.Build();
        upstream.UseNerosHttp();
        upstream.MapGet("/relay", async (IHttpClientFactory clients, HttpContext context) =>
        {
            using var http = clients.CreateClient("downstream");
            return Results.Content(await http.GetStringAsync("/observed", context.RequestAborted), "application/json");
        });
        await upstream.StartAsync();

        using var client = new HttpClient { BaseAddress = Address(upstream) };
        var correlationId = Guid.NewGuid().ToString("D");
        client.DefaultRequestHeaders.Add(FoundationHttp.CorrelationHeader, correlationId);
        using var response = await client.GetAsync("/relay");
        response.EnsureSuccessStatusCode();
        using var result = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var traceId = result.RootElement.GetProperty("traceId").GetString();
        Assert.Equal(correlationId, result.RootElement.GetProperty("correlationId").GetString());
        await upstream.StopAsync();
        await downstream.StopAsync();
        upstream.Services.GetRequiredService<TracerProvider>().ForceFlush();
        downstream.Services.GetRequiredService<TracerProvider>().ForceFlush();
        Assert.Contains(spans, span => span.TraceId == traceId && span.Kind == ActivityKind.Client);
        Assert.True(spans.Where(span => span.TraceId == traceId && span.Kind == ActivityKind.Server)
            .Select(span => span.SpanId).Distinct().Count() >= 2);
    }

    private static Uri Address(WebApplication app) => new(app.Services.GetRequiredService<IServer>()
        .Features.Get<IServerAddressesFeature>()!.Addresses.Single());

    internal sealed record SpanEvidence(string TraceId, string SpanId, ActivityKind Kind, string? CorrelationId = null);

    internal sealed class EvidenceExporter(ConcurrentBag<SpanEvidence> spans) : BaseExporter<Activity>
    {
        public override ExportResult Export(in Batch<Activity> batch)
        {
            foreach (var activity in batch)
                spans.Add(new SpanEvidence(activity.TraceId.ToString(), activity.SpanId.ToString(), activity.Kind,
                    activity.GetTagItem("neros.correlation_id") as string));
            return ExportResult.Success;
        }
    }
}