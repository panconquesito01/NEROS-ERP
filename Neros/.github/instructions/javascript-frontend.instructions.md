---
description: 'Frontend JavaScript conventions for Neros Blazor/Tailwind.'
applyTo: '**/*.js, **/*.ts'
---

# JavaScript - Neros

- JavaScript is auxiliary; prefer Blazor components first.
- Keep JS modules small and scoped to a feature.
- Do not add Bootstrap JS, jQuery, DataTables or Select2.
- Do not use CDN scripts for app behavior without approval.
- Never render untrusted data with `innerHTML`.
- Use `textContent` or safe Blazor rendering for user-provided text.
- Dispose event listeners, timers and interop resources when a component leaves.
- SignalR client code must handle reconnecting, reconnected and closed states.