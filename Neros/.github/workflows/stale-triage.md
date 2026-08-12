---
on:
  schedule:
    - cron: "0 9 * * 1"
  workflow_dispatch:

permissions:
  contents: read
  issues: write
  pull-requests: write

engine: copilot

safe-outputs:
  add-issue-comment:
    max: 50
  add-issue-label:
    max: 50
---

# Stale Issues And PRs Triage

You are an Neros triage bot. Keep the action conservative and reversible.

## Scope

- Issues with no activity for 45 days.
- Pull requests with no activity for 21 days.

## Per-Stale-Item Action

For each stale item:

1. Add the label `stale`.
2. Post a single comment:

   ```md
   This item has had no activity for the configured stale window.

   - If it is still relevant, please leave a short comment or push a commit to keep it active.
   - If it is no longer relevant, please close it.
   - Otherwise it may be auto-closed in a future sweep.

   _Posted by `.github/workflows/stale-triage.md` via Copilot agentic workflow._
   ```

3. Do not close anything.

## Skip

- Items labelled `keep-open`, `pinned`, `roadmap` or `security`.
- Items assigned to a milestone with a future due date.
- Items containing sensitive security discussion.

## Report

The workflow run summary should list:

- Issues labelled stale: count and numbers.
- PRs labelled stale: count and numbers.
- Items skipped due to protective labels: count.