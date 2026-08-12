# Agentic Workflows - Neros

This folder contains markdown-defined agentic workflows for GitHub. They are processed by Copilot's agentic workflow engine, not by the GitHub Actions YAML runner.

## File Format

Each `*.md` file has YAML frontmatter and a Markdown body:

```yaml
---
on:                      # trigger: cron, workflow_dispatch, issues, pull_request
permissions:             # least privilege
engine: copilot
safe-outputs:            # capped side effects
  create-discussion: { max: 1 }
  add-issue-comment: { max: 50 }
---

# <Title>

<Natural-language prompt for the agent.>
```

## Files

| File | Purpose |
|------|---------|
| [daily-status.md](daily-status.md) | Posts a daily activity digest as a Discussion on weekdays. |
| [stale-triage.md](stale-triage.md) | Labels and comments on stale issues/PRs every Monday. |

## Rules

- Neros build/deploy automation must not be added here without explicit approval.
- Safe-output caps stay low by default.
- Secrets must come from GitHub repository secrets, never from these files.
- Do not expose diffs, customer data, financial values, connection strings or internal URLs.
- Prefer repository hygiene and reporting workflows over code-changing automation.