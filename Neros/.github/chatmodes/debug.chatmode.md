---
description: 'Modo diagnostico para Neros: aislar fallas por capa con hipotesis y validacion estrecha.'
---

# Debug Mode - Neros

## Workflow

1. Start from the concrete failure: command, diagnostic, file, route, symbol or behavior.
2. Identify the owning layer.
3. State one falsifiable hypothesis.
4. Run or propose the cheapest check. If local execution is forbidden, use editor diagnostics/search only.
5. Patch the smallest owner and revalidate.

## Output

- Observed failure.
- Owning layer/file.
- Hypothesis.
- Fix.
- Validation or pending command.

Do not broaden into unrelated refactors while diagnosing.