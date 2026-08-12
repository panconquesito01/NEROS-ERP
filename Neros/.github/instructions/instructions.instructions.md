---
description: 'How to write and maintain .instructions.md files in this repo.'
applyTo: '**/.github/instructions/**/*.md'
---

# Authoring Instructions Files - Neros

These rules apply when creating or editing `*.instructions.md` under `.github/instructions/`.

## File Naming

- Lowercase, hyphen-separated: `csharp-async.instructions.md`.
- Suffix is always `.instructions.md`.

## Front Matter

```md
---
description: '<one-sentence, what this set of rules covers>'
applyTo: '<glob, comma-separated for multiple>'
---
```

- `description` is required and non-empty.
- `applyTo` is required and should be as narrow as practical.
- Do not include `name`; the file path is the identity.

## Good applyTo Examples

- `'**/*.cs'`
- `'**/*.razor'`
- `'**/Neros.Blazor/**/*.razor, **/Neros.Blazor/**/*.css'`
- `'**'` only for true cross-cutting rules such as security.

## Body Structure

1. H1 title, short and topical.
2. Scope paragraph when useful.
3. Specific rules by topic.
4. What not to touch when relevant.
5. Validation block when a deterministic check exists.

## Neros Content Rules

- Mention `Neros.slnx`, not a single-project build target.
- Model the new architecture: Blazor, Api, Application, Domain, Contracts, Shared.
- Mark legacy MVC/Servicios/EQContext guidance as migration reference only.
- Prefer PowerShell examples for commands.
- No real secrets, tokens, connection strings or personal absolute paths.

## When To Create A New File

- A new technology enters the stack.
- A rule applies to a distinct file family.
- An existing instruction grows too broad or overloaded.

## Validation Checklist

- [ ] YAML frontmatter parses.
- [ ] `applyTo` is not broader than necessary.
- [ ] No mojibake or broken encoding.
- [ ] No legacy architecture as target guidance.
- [ ] Commands are Neros commands.