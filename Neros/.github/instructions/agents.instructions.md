---
description: 'How to write and maintain agent definitions for Neros (.vscode, .cursor, .codex, .antigravity, chatmodes).'
applyTo: '**/.vscode/agents/**/*.md, **/.cursor/agents/**/*.md, **/.codex/**/*.md, **/.antigravity/**/*.md, **/.github/chatmodes/**/*.md, **/agents/**/*.agent.md'
---

# Authoring Agents - Neros

Agent files must reinforce the same project model as `AGENTS.md`: .NET 10, Blazor Web App, API, Clean Architecture, Tailwind CSS, SQL Server with manual scripts, Identity, SignalR and Graphify.

## File Locations

| Client | Folder | File extension |
|--------|--------|----------------|
| VS Code | `.vscode/agents/` | `<name>.md` |
| VS Code chat modes | `.github/chatmodes/` | `<name>.chatmode.md` |
| Cursor | `.cursor/agents/` | `<name>.md` |
| Codex bridge | `.codex/` and `CODEX.md` | `*.md` |
| Antigravity bridge | `.antigravity/` and `ANTIGRAVITY.md` | `*.md` |
| Optional generic agents | `agents/` | `<name>.agent.md` |

Keep parallel files in sync. If you change `.vscode/agents/dev.md`, update `.cursor/agents/dev.md`.
Codex and Antigravity should point to the canonical VS Code/Cursor profiles unless their runtime requires a different format.

## Front Matter

```md
---
name: <kebab-case-name>
description: "Use when: <clear trigger and scope>"
tools: [optional list]
---
```

- `name` matches the file name without extension.
- `description` is the discovery surface. Mention trigger phrases and target layers.
- Quote descriptions containing colons or commas.

## Required Sections

Every Neros agent should include:

1. Mission or purpose.
2. Target layers/files.
3. Rules and non-goals.
4. Workflow.
5. Validation commands or pending validation behavior.
6. Security/data risks to watch.

## Neros Defaults

- Workspace root is the current `Neros` folder inside this repository.
- Solution is `Neros.slnx`.
- Do not hard-code personal paths.
- Do not use `Neros.Next`, Aurosoft, Equaltech or old monolith names as target architecture.
- Legacy code is functional reference only.

## Skills

Reference local skills by folder/name, for example `skills/diagnose/SKILL.md`. If a new skill is added or renamed, update VS Code and Cursor agents that mention it.

## Tooling Rules

- Agents that modify files need edit/search capability.
- Read-only agents should stay read-only.
- If the user says not to execute local commands, the agent must not run builds, tests, npm, dotnet, graphify or servers.

## Validation Checklist

- [ ] File name matches `name:`.
- [ ] Frontmatter parses.
- [ ] Description is specific and discoverable.
- [ ] No secrets, tokens, connection strings or personal paths.
- [ ] Parallel VS Code/Cursor file updated when applicable.
- [ ] Neros layer rules are present.