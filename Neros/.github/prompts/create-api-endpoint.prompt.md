---
description: 'Crear endpoint API en Neros con Application, Contracts, seguridad y validacion.'
mode: agent
---

# Crear Endpoint API - Neros

Crear o modificar un endpoint en `Neros.Api` respetando capas.

## Reglas

- Request/response en `Neros.Contracts`.
- Caso de uso en `Neros.Application`.
- Controlador o endpoint delgado.
- Validar entrada en el borde y reglas de negocio en Application/Domain.
- Aplicar autorizacion server-side cuando el endpoint modifique o exponga datos protegidos.
- Sin SQL directo en controlador.
- Sin secretos en logs o respuestas.

## Pasos

1. Identificar contrato requerido.
2. Crear/ajustar caso de uso Application.
3. Agregar endpoint API y mapeo de respuesta/error.
4. Registrar DI si aplica.
5. Documentar validacion.

## Validacion

```powershell
dotnet build .\Neros.slnx -v minimal
```

Si no se permite ejecutar local, dejar el comando como pendiente.