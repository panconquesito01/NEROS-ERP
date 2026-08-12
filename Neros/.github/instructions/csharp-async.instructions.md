---
description: 'C# async/await best practices for Neros.'
applyTo: '**/*.cs'
---

# C# Async - Neros

## Names

- Suffix async methods with `Async` when an async and sync form can coexist.
- Prefer action-oriented Spanish names for business use cases.

## Return Types

- Returns a value: `Task<T>`.
- Returns nothing: `Task`.
- Frequent synchronous completion on hot paths: consider `ValueTask<T>` only with a clear reason.
- No `async void` except required event-handler patterns.

## Banned

- `.Result`
- `.Wait()`
- `.GetAwaiter().GetResult()`
- Sync-over-async wrappers around HTTP, EF Core, filesystem or SignalR.

## By Layer

- Blazor: show loading/error state and keep event handlers small.
- API: pass request cancellation to Application where useful.
- Application: own cancellation-aware use cases and transaction intent.
- Domain: keep pure rules synchronous unless there is a real I/O boundary elsewhere.
- SignalR/jobs: await sends/work, observe exceptions and support cancellation.

## Parallelism

- Use `Task.WhenAll` only for independent work.
- Do not share DbContext, transactions, aggregate instances or mutable component state across parallel awaits.
- Bound concurrency for large batches.

## Cancellation

- Accept and pass `CancellationToken` through I/O operations.
- Jobs and long-running operations should stop cooperatively.

## Exceptions

- Do not swallow exceptions.
- Log at boundaries with context, never secrets.
- Fire-and-forget work must observe/log failures.