# Base multiempresa de Neros ERP

## Resumen ejecutivo

Esta entrega transforma el acceso visual de Neros en una base funcional: usuarios autenticados, empresas asignadas, cambio de contexto y trazabilidad de acceso. Tambien reemplaza los indicadores de demostracion por un inicio que muestra informacion real de la empresa y del usuario.

## Valor entregado

| Capacidad | Beneficio |
| --- | --- |
| Acceso con Identity | El inicio requiere credenciales verificadas; ya no es una navegacion libre |
| Empresas autorizadas | Cada persona ve solo las empresas que tiene asignadas |
| Contexto visible | Reduce la posibilidad de trabajar creyendo estar en otra empresa |
| Roles por empresa | Una misma persona puede tener responsabilidades distintas entre empresas |
| Cierre revocable y bloqueo | Mejora el control ante sesiones abandonadas e intentos de acceso |
| Actividad personal | Permite revisar los accesos propios a la empresa seleccionada |
| Tema automatico y manual | Adapta el espacio a preferencias claras u oscuras |
| Inicio simplificado | Evita saturacion y elimina cifras comerciales ficticias |

## Lo que no incluye

Ventas, inventario, finanzas y consolidacion gerencial se muestran como pendientes. No existen aun documentos comerciales, saldos contables, indicadores consolidados ni permisos por cada operacion de negocio. Los roles de esta base permiten acceso y consulta de actividad propia; no deben interpretarse como una matriz completa de autorizacion del ERP.

## Puesta en marcha

1. El responsable tecnico instala el esquema SQL y configura la conexion fuera del repositorio.
2. Un responsable autorizado crea localmente el primer administrador y la primera empresa, sin claves predeterminadas.
3. Se asignan las empresas adicionales con el rol correspondiente.
4. Se verifica el acceso, cambio de empresa, tema visual y cierre de sesion.
5. Antes de exponer el sistema fuera del equipo local, se completa la lista de seguridad y operacion de produccion.

## Decisiones gerenciales necesarias

| Decision | Responsable sugerido |
| --- | --- |
| Empresas y responsables iniciales | Direccion administrativa |
| Matriz de permisos por operacion | Dueños de proceso y seguridad |
| Primer modulo transaccional a migrar | Gerencia y lider funcional |
| Alcance de consolidacion entre empresas | Finanzas y gerencia |
| Retencion de auditoria y politica de acceso | Seguridad y cumplimiento |
| Infraestructura, respaldos y recuperacion | Tecnologia |

## Siguiente etapa

Priorizar un flujo operativo completo, por ejemplo ventas o inventario, con reglas de negocio, permisos por empresa y pruebas de aislamiento. La consolidacion gerencial requiere definir moneda, periodos, eliminaciones intercompañia y permisos de lectura cruzada; no consiste en sumar todos los registros disponibles.

## Criterio de aceptacion

Una cuenta valida puede entrar, elegir una empresa autorizada, identificar su rol, revisar su actividad y cerrar la sesion. Una cuenta no puede abrir una empresa ajena mediante una URL manipulada. La interfaz debe conservar legibilidad y controles utilizables en escritorio y movil, en ambos temas.

Esta base no equivale a una certificacion de seguridad ni a autorizacion para produccion. Los resultados concretos de las verificaciones se registran en [validacion tecnica](../tecnica/validacion.md).