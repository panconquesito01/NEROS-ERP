---
description: 'Blazor Razor component conventions for Neros.'
applyTo: '**/*.razor'
---

# Blazor - Neros

- Use Blazor Web App components, not MVC Razor Areas.
- Use Tailwind CSS utilities and classes from `Neros.Blazor/Styles/tailwind.css`.
- Do not add Bootstrap, jQuery, DataTables or Select2.
- Keep components focused on UI state and user interaction.
- Do not inject DbContext or repositories into components.
- Use contracts from `Neros.Contracts` and client services for API calls.
- Keep text in Spanish for ERP-facing UI.

## UX Rules

- Include loading, empty, validation and error states for data-driven UI.
- Keep ERP screens dense, scannable and work-focused.
- Avoid visible text explaining implementation internals.
- Avoid routes demo, dead links and template pages in production navigation.

## Forms

- Prefer `EditForm` with validation messages.
- Disable submit actions while saving.
- Surface server validation errors near the relevant field when possible.

## After Editing

```powershell
npm run css:build
dotnet build .\Neros.slnx -v minimal
```