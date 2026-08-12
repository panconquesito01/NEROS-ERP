# VS Code Agents - Neros

Agentes locales para trabajar en `Neros`. Todos deben seguir `../AGENTS.md`, `../CONVENTIONS.md` y las skills locales en `../skills/`.

## Flujo recomendado

```text
/planner -> /dev -> /qa
```

Usar especialistas cuando la tarea tenga propietario claro:

- `backend`: API, Application, Domain, contratos, seguridad, persistencia futura e integraciones.
- `frontend`: Blazor Web App, Tailwind CSS, componentes, layouts, accesibilidad y UX de ERP.
- `planner`: alcance, arquitectura, riesgos, plan por capas y validaciones.
- `qa`: revision final, regresiones, seguridad, build, Tailwind y Graphify.

## Puentes Compatibles

- Codex: `../CODEX.md` y `../.codex/AGENTS.md`.
- Antigravity: `../ANTIGRAVITY.md` y `../.antigravity/AGENTS.md`.
- Claude: `../CLAUDE.md`.
- Gemini: `../GEMINI.md`.

Todos deben reutilizar estos perfiles como canonicos cuando no tengan formato propio.

## Reglas compartidas

- No ejecutar comandos locales si el usuario lo prohibe; usar diagnosticos del editor y dejar comandos pendientes.
- Consultar Graphify para arquitectura si existe `graphify-out/graph.json` y se permite ejecutar.
- No copiar MVC Areas/Servicios del legado como arquitectura objetivo.
- No introducir Bootstrap, jQuery, DataTables ni Select2.
- Mantener Blazor -> API -> Application -> Domain.
- Usar scripts SQL Server manuales versionados cuando haya cambios de base de datos.

