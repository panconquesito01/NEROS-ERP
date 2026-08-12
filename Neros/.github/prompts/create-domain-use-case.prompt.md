---
description: 'Crear caso de uso de Domain/Application en Neros con contratos, reglas, seguridad y validacion.'
mode: agent
---

# Crear Caso De Uso - Neros

Disenar e implementar una regla/caso de uso respetando Clean Architecture.

## Reglas

- Domain contiene invariantes, entidades, value objects y reglas puras.
- Application orquesta el caso de uso, validaciones, puertos y transaccion.
- Contracts define request/response si cruza API/Blazor.
- API solo adapta HTTP y autorizacion.
- No infraestructura concreta en Application o Domain.
- Nombres de negocio en espanol.

## Pasos

1. Identificar regla de negocio e invariantes.
2. Definir contrato si cruza capa.
3. Implementar Domain si hay regla pura.
4. Implementar Application use case y puertos necesarios.
5. Integrar API/UI solo si el alcance lo pide.
6. Documentar riesgos de datos, seguridad y concurrencia.

## Validacion

```powershell
dotnet build .\Neros.slnx -v minimal
```

Si no se permite ejecutar local, usar diagnosticos del editor y dejar comandos pendientes.