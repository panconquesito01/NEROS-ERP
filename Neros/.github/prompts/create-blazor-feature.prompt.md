---
description: 'Crear una feature Blazor en Neros usando Tailwind, contratos tipados, estados UX y arquitectura por capas.'
mode: agent
---

# Crear Feature Blazor - Neros

Crea una feature en `Neros.Blazor` usando Blazor Web App, Tailwind CSS y contratos de `Neros.Contracts`.

## Reglas

- No Bootstrap, jQuery, DataTables ni Select2.
- No DbContext ni repositorios en Blazor.
- Si necesita datos, usar contrato y servicio cliente/API.
- Mantener componentes enfocados en UI state.
- Incluir loading, empty, error y validation states.
- UI ERP: densa, clara, responsive y accesible.
- Textos funcionales en espanol.

## Pasos

1. Definir ruta/componente y layout.
2. Definir contratos/API necesarios o marcar dependencia pendiente.
3. Implementar UI Tailwind con estados completos.
4. Evitar enlaces muertos y rutas demo.
5. Documentar validacion.

## Validacion

```powershell
npm run css:build
dotnet build .\Neros.slnx -v minimal
```

Si no se permite ejecutar local, usar diagnosticos del editor y dejar comandos pendientes.