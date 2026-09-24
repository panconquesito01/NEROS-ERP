using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Neros.ServiceDefaults;

public static class FoundationHttp
{
    public const string CorrelationHeader = "X-Correlation-Id";
    private static readonly object CorrelationKey = new();

    public static IServiceCollection AddNerosHttp(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Extensions.TryAdd("code", context.ProblemDetails.Status switch
            {
                400 => "validation_failed",
                401 => "authentication_required",
                403 => "permission_denied",
                404 => "resource_not_found",
                409 => "conflict",
                412 => "version_conflict",
                429 => "quota_exceeded",
                503 => "dependency_unavailable",
                _ => "unexpected_error"
            });
            context.ProblemDetails.Extensions["correlationId"] = GetCorrelationId(context.HttpContext);
            context.ProblemDetails.Extensions["traceId"] = Activity.Current?.TraceId.ToString()
                ?? context.HttpContext.TraceIdentifier;
        });
        services.AddExceptionHandler<UnhandledExceptionHandler>();
        services.AddHealthChecks();
        services.AddAuthorization(options => options.AddPolicy("Neros.Health", policy =>
            policy.RequireAuthenticatedUser().RequireClaim("permission", "Platform.Health.Read")));
        return services;
    }

    public static IApplicationBuilder UseNerosHttp(this IApplicationBuilder app)
    {
        app.UseNerosCorrelation();
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        return app;
    }

    public static IApplicationBuilder UseNerosCorrelation(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var header = context.Request.Headers[CorrelationHeader];
            var correlationId = header.Count == 1 && Guid.TryParseExact(header[0], "D", out var supplied)
                && supplied != Guid.Empty ? supplied.ToString("D") : Guid.NewGuid().ToString("D");
            context.Items[CorrelationKey] = correlationId;
            Activity.Current?.SetTag("neros.correlation_id", correlationId);
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationHeader] = correlationId;
                context.Response.Headers.CacheControl = "no-store";
                context.Response.Headers.XContentTypeOptions = "nosniff";
                return Task.CompletedTask;
            });
            await next(context);
        });
        return app;
    }

    public static WebApplication MapNerosHealth(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteHealthAsync
        }).AllowAnonymous();
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            ResponseWriter = WriteHealthAsync
        }).RequireAuthorization("Neros.Health");
        return app;
    }

    public static string GetCorrelationId(HttpContext context) =>
        context.Items.TryGetValue(CorrelationKey, out var value) && value is string correlationId
            ? correlationId : context.TraceIdentifier;

    private static Task WriteHealthAsync(HttpContext context, HealthReport report) =>
        context.Response.WriteAsJsonAsync(new { status = report.Status.ToString() }, context.RequestAborted);

    private sealed class UnhandledExceptionHandler(IProblemDetailsService problems,
        ILogger<UnhandledExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
            {
                context.Response.StatusCode = 499;
                return true;
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            logger.LogError("Error HTTP no controlado. Code={Code} CorrelationId={CorrelationId}",
                "unexpected_error", GetCorrelationId(context));
            var details = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "No se pudo completar la operacion.",
                Extensions = { ["code"] = "unexpected_error" }
            };
            if (!await problems.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = details
            }))
            {
                details.Extensions["correlationId"] = GetCorrelationId(context);
                details.Extensions["traceId"] = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
                await context.Response.WriteAsJsonAsync(details, options: (System.Text.Json.JsonSerializerOptions?)null,
                    contentType: "application/problem+json",
                    cancellationToken: cancellationToken);
            }
            return true;
        }
    }
}