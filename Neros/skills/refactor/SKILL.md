---
name: refactor
description: "Use when refactoring Neros code while preserving behavior: layer extraction, Blazor component cleanup, API/Application split, contract cleanup, async cleanup, or migration from legacy patterns."
---

# Refactor - Neros

## Principle

Preserve behavior, reduce coupling, and move code toward the Neros layer model without broad rewrites.

## Refactor Targets

- Blazor component with business logic -> Application use case + clean component state.
- API controller with workflow logic -> Application service/use case.
- Shared anonymous objects -> Contract DTOs.
- Domain rule hidden in UI/API -> Domain method/value object.
- Legacy MVC/Servicios pattern -> new vertical slice by layer.

## Workflow

1. Capture current behavior and validation signal.
2. Identify the smallest boundary to improve.
3. Move code one layer at a time.
4. Keep public contracts stable unless the task requires changing them.
5. Validate after each meaningful slice.

## Refactor Recipes

### Extract Application Use Case

1. Identify business action and inputs.
2. Create request/response DTOs if crossing API/Blazor.
3. Move orchestration into Application.
4. Keep controller/page as adapter.
5. Add ports for infrastructure dependencies.

### Split Blazor Component

1. Separate page shell, form state and repeated display components.
2. Keep data loading/saving in a typed client/service.
3. Preserve validation and loading/error states.
4. Avoid inventing a global state container unless repeated workflows need it.

### Replace Legacy Pattern

1. Summarize old functional rule.
2. Define new layer mapping.
3. Move rule to Domain/Application.
4. Replace UI with Blazor/Tailwind workflow.
5. Leave compatibility notes if old route/data is still referenced.

## Common Refactor Smells

| Smell | Refactor Direction |
|---|---|
| Long controller action | Application use case |
| Repeated validation in UI/API | Domain/Application validation |
| Anonymous object returned from API | Contract DTO |
| Component with mixed data fetching and complex markup | Page + child components + client service |
| Direct infrastructure call in Application | Application port + infrastructure implementation |
| Copy-pasted Tailwind blocks | Component or local helper only when repeated meaningfully |

## Do Not

- Mix refactor with unrelated feature work.
- Reformat entire files without need.
- Change database schema without a script plan.
- Introduce new abstractions without reducing real duplication or coupling.

## Checks

- Direction of dependencies remains valid.
- Blazor does not gain data access.
- Domain stays pure.
- API remains thin.
- DTOs are explicit.
- Tailwind/UI behavior remains intact.

## Safety Rails

- Do not change public route/contract names unless requested.
- Do not move files across projects without checking project references.
- Do not delete legacy reference material unless asked; mark it as reference instead.
- Do not combine schema changes with broad code cleanup.

## Output

```text
Current smell:
Target shape:
Steps completed:
Behavior preserved by:
Validation:
Remaining risk:
```