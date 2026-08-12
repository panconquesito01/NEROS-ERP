---
description: 'Report/PDF generation conventions for Neros, including future FastReport or alternative engines.'
applyTo: '**/*Report*.cs, **/*Reporte*.cs, **/*Pdf*.cs, **/*.frx'
---

# Reports And PDF - Neros

Neros may generate invoices, payroll, operational reports and PDFs. Keep report generation behind Application ports or API services; Blazor should request/download results, not compose reports directly.

## Placement

- Contracts define report request/response metadata.
- Application owns the use case and authorization-sensitive decisions.
- Infrastructure/API implementation loads templates, queries data and renders files.
- Blazor displays filters, status and download links.

## Rules

- Do not store generated private documents under public `wwwroot` unless intentionally public.
- Validate report parameters and enforce server-side authorization.
- Avoid loading unbounded data sets into memory.
- Use temp files safely and clean them up.
- Log report name and correlation data, never sensitive payloads.
- Do not remove vendor watermarks or bypass licensing.

## Data

- Prefer projections/DTOs for report data.
- Pre-aggregate large reports in SQL or bounded queries.
- Do not pass EF entities through UI/API boundaries.

## Output

For report work, document:

- Template/source.
- Data source/query boundary.
- Authorization rule.
- File lifetime and storage.
- Validation command or manual check.