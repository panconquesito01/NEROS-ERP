---
description: 'GitHub Actions and agentic workflow conventions for Neros.'
applyTo: '**/.github/workflows/**/*.yml, **/.github/workflows/**/*.yaml, **/.github/workflows/**/*.md'
---

# GitHub Actions - Neros

Use GitHub Actions only when they fit the repository workflow. Do not introduce a parallel deploy/build pipeline without explicit approval.

## Agentic Workflows

- Markdown workflows live in `.github/workflows/*.md`.
- Use least-privilege permissions.
- Use safe outputs for issue/comment creation.
- Do not give automation write access unless the task requires it.

## Standard YAML Workflows

- Pin third-party actions to commit SHA for sensitive workflows.
- Set `permissions:` explicitly.
- Do not echo secrets or connection strings.
- Mask sensitive runtime values.

## Neros Build Commands

```powershell
npm run css:build
dotnet build .\Neros.slnx -v minimal
```

## Caching

- Cache NuGet packages using keys based on `.slnx`, `.csproj`, `packages.lock.json` and `global.json` if present.
- Cache npm using `package-lock.json` or equivalent lock file if present.

## Do Not

- Do not key caches on `Neros.csproj`.
- Do not run untrusted PR code with secrets.
- Do not trigger expensive builds on every personal branch without approval.