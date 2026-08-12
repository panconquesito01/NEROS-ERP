---
description: 'How to write and maintain local Agent Skills (skills/<name>/SKILL.md) for Neros.'
applyTo: '**/skills/**/SKILL.md'
---

# Authoring Agent Skills - Neros

Local skills live under `skills/<skill-name>/SKILL.md`. They are workflow playbooks for recurring Neros tasks.

## Folder Layout

```text
skills/
`-- <skill-name>/
    |-- SKILL.md
    |-- assets/       # optional templates/examples
    |-- references/   # optional supporting docs
    `-- scripts/      # optional scripts, only when truly needed
```

## Required Front Matter

```md
---
name: <skill-name>
description: "Use when: <specific trigger, domain, and expected workflow>"
---
```

- `name` must match the folder name.
- `description` must explain when to invoke the skill.
- Quote descriptions, especially if they contain colons.

## Body Structure

1. H1 title.
2. When to use.
3. Neros-specific rules.
4. Workflow.
5. Output contract.
6. Anti-patterns.
7. Validation.

## Neros Defaults

- Solution: `Neros.slnx`.
- Frontend: Blazor Web App + Tailwind CSS.
- Backend: ASP.NET Core Web API -> Application -> Domain.
- Contracts: `Neros.Contracts`; shared primitives: `Neros.Shared`.
- Database: SQL Server with manual scripts in `database/scripts/`.
- Graphify: use for architecture and impact questions when command execution is allowed.

## Commands To Reference

```powershell
npm run css:build
dotnet build .\Neros.slnx -v minimal
graphify update .
```

Do not tell a skill to execute commands when the user has prohibited local execution.

## Anti-patterns

- Hard-coded personal paths.
- `Neros.csproj` as the solution target.
- MVC Areas, `Servicios/*`, `EQContext` or Bootstrap/jQuery as target architecture.
- Vague descriptions such as "helps with code".

## Validation Checklist

- [ ] Folder name is kebab-case.
- [ ] `name:` matches folder name.
- [ ] `description:` is quoted and discoverable.
- [ ] No secrets or personal paths.
- [ ] Commands use `Neros.slnx`.
- [ ] Skill distinguishes current Neros architecture from legacy reference.