---
name: csharp-async
description: "Use for async/await decisions in Neros .NET 10 code: API endpoints, Application use cases, Blazor event handlers, SignalR hubs, jobs, cancellation, streaming, and concurrency."
---

# C# Async - Neros

## Defaults

- Prefer async all the way for I/O, database, HTTP, file, SignalR and job work.
- Accept `CancellationToken` in API/Application operations that can be cancelled.
- Do not block async code with `.Result`, `.Wait()` or sync-over-async.
- Do not share non-thread-safe services, DbContext instances or mutable state across parallel awaits.

## By Layer

- **Blazor**: keep event handlers short; show loading/error state; avoid fire-and-forget unless errors are observed.
- **Api**: pass `HttpContext.RequestAborted` into Application when appropriate.
- **Application**: expose cancellable use cases; keep transaction boundaries explicit.
- **Domain**: usually synchronous; keep pure rules deterministic.
- **SignalR**: await sends, authorize hub methods, keep payloads small.
- **Jobs**: use scoped services per execution, cancellation-aware loops and idempotent retries.

## ConfigureAwait

- Application/library-style code may use `.ConfigureAwait(false)` if the local style does.
- Blazor components and UI event handlers should not use it by default.

## Parallelism

- Use `Task.WhenAll` only for independent operations.
- Do not parallelize operations that share transactions, DbContext or mutable aggregates.
- Bound concurrency when processing many items.

## Examples

```csharp
// Good: cancellation flows from API to Application.
public async Task<IResult> ConsultarAsync(
	ConsultarPedidosRequest request,
	IPedidosUseCase useCase,
	CancellationToken cancellationToken)
{
	var resultado = await useCase.EjecutarAsync(request, cancellationToken);
	return Results.Ok(resultado);
}
```

```csharp
// Bad: sync-over-async can deadlock/starve threads.
var pedidos = useCase.EjecutarAsync(request, CancellationToken.None).Result;
```

```csharp
// Good: independent HTTP calls with bounded intent.
var clienteTask = clientesClient.ObtenerAsync(clienteId, ct);
var carteraTask = carteraClient.ObtenerResumenAsync(clienteId, ct);
await Task.WhenAll(clienteTask, carteraTask);
```

```csharp
// Bad: parallel work shares mutable persistence state.
await Task.WhenAll(items.Select(item => db.AddAsync(item, ct).AsTask()));
```

## Common Mistakes And Fixes

| Mistake | Fix |
|---|---|
| Fire-and-forget in Blazor submit | Await operation and show loading/error state |
| Hub method broadcasts without awaiting | Await `Clients...SendAsync` and handle exceptions |
| Background loop ignores cancellation | Check token and pass it to awaited I/O |
| `Task.Run` around I/O | Use native async API instead |
| Parallel EF operations on one context | Use sequential operations or separate scopes carefully |
| Swallowed exception in job | Log with context and decide retry/dead-letter strategy |

## Timeout And Retry Guidance

- Prefer cancellation tokens and explicit timeouts at HTTP/client boundaries.
- Retry only idempotent operations or operations with idempotency keys.
- Do not retry inside a database transaction that may have partial side effects.
- Use backoff for external integrations.

## Review Checklist

- Cancellation token flows from API/job to Application.
- Exceptions are not swallowed.
- No fire-and-forget without logging/observation.
- No async void except UI event interop patterns that require it.
- No hidden race in component state or hub groups.

## Escalate To

- `security-review` if async retry can duplicate sensitive operations.
- `ef-core` if concurrency involves DbContext/transactions.
- `diagnose` if a deadlock, timeout or race is already observed.