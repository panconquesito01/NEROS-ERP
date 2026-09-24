using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly.CircuitBreaker;
using Polly.RateLimiting;
using Polly.Timeout;

namespace Neros.ServiceDefaults;

public static class FoundationTelemetry
{
    public static IHostApplicationBuilder AddNerosTelemetry(this IHostApplicationBuilder builder, string serviceName)
    {
        var export = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation()
                    .AddSource("Neros.Messaging", "Yarp.ReverseProxy");
                if (export) tracing.AddOtlpExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation()
                    .AddMeter("Neros.Messaging", "System.Runtime");
                if (export) metrics.AddOtlpExporter();
            });
        builder.Logging.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
            options.IncludeFormattedMessage = false;
            options.IncludeScopes = false;
            if (export) options.AddOtlpExporter();
        });
        return builder;
    }

    public static IHttpClientBuilder AddNerosResilience(this IHttpClientBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.AddHttpMessageHandler(provider => new CorrelationHandler(provider.GetRequiredService<IHttpContextAccessor>()));
        builder.AddHttpMessageHandler(() => new ResilienceFailureHandler());
        builder.AddStandardResilienceHandler(options =>
        {
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(3);
            options.Retry.MaxRetryAttempts = 2;
            options.Retry.Delay = TimeSpan.FromMilliseconds(100);
            options.Retry.UseJitter = true;
            options.Retry.DisableForUnsafeHttpMethods();
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(10);
            options.RateLimiter.DefaultRateLimiterOptions.PermitLimit = 32;
            options.RateLimiter.DefaultRateLimiterOptions.QueueLimit = 0;
        });
        return builder;
    }

    private sealed class CorrelationHandler(IHttpContextAccessor accessor) : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (accessor.HttpContext is { } context)
            {
                request.Headers.Remove(FoundationHttp.CorrelationHeader);
                request.Headers.Add(FoundationHttp.CorrelationHeader, FoundationHttp.GetCorrelationId(context));
            }
            return base.SendAsync(request, cancellationToken);
        }
    }

    private sealed class ResilienceFailureHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                return await base.SendAsync(request, cancellationToken);
            }
            catch (TimeoutRejectedException)
            {
                throw new TaskCanceledException("El servicio remoto excedio el tiempo permitido.");
            }
            catch (BrokenCircuitException)
            {
                throw new HttpRequestException("El servicio remoto no esta disponible.", null, System.Net.HttpStatusCode.ServiceUnavailable);
            }
            catch (RateLimiterRejectedException)
            {
                throw new HttpRequestException("Se alcanzo el limite de solicitudes remotas.", null, System.Net.HttpStatusCode.ServiceUnavailable);
            }
        }
    }
}