---
description: 'Markdown conventions for Neros documentation, agents, prompts and skills.'
applyTo: '**/*.md'
---

# Markdown - Neros

- Use GitHub-Flavored Markdown.
- Use one H1 per document.
- Use fenced code blocks with language identifiers.
- Use PowerShell examples for local commands.
- Keep agent/skill/prompt frontmatter valid YAML.
- Prefer ASCII unless the file already requires Spanish accents and renders correctly.
- Do not include real secrets, tokens, connection strings or personal paths.
- Documentation for project behavior should describe `Neros`, not the old MVC monolith, unless explicitly marked as migration reference.
- Link to repo files with workspace-relative paths.