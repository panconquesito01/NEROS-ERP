# Configuracion por entorno y secretos de Neros

Revision: 2026-09-13. Alcance: nueva solucion Neros, no el sistema anterior ni el historial completo del repositorio.

## Resultado de la revision

| Elemento | Estado comprobado | Decision |
| --- | --- | --- |
| API: appsettings base | Logging y AllowedHosts; sin ConnectionStrings | Mantener valores comunes no secretos |
| Blazor: appsettings base | Logging, AllowedHosts y Api:BaseUrl local; sin acceso SQL | Mantener para compatibilidad local; sobrescribir URL y hosts al desplegar |
| Development de ambos proyectos | Existian, con Logging, y estaban versionados | Conservar en disco y retirar del indice Git |
| Conexion Neros | Clave presente en User Secrets de API, ID neros-api-desarrollo | Mantener fuera del arbol del proyecto |
| Variable ConnectionStrings__Neros del proceso de revision | Ausente | No sustituye el secreto local en esta sesion |
| Proveedor externo de secretos | No registrado | No agregar infraestructura sin destino de despliegue definido |

No se imprimieron valores de secretos. Los cuatro appsettings revisados no contienen cadenas de conexion. Una busqueda acotada de indicadores de claves privadas, contrasenas SQL, AccountKey, SAS y Server en codigo/configuracion versionados de Neros no produjo coincidencias. Esto no certifica la ausencia de secretos: no cubre historial, binarios, recursos externos, formatos arbitrarios ni todo el monolito del repositorio padre. Activar deteccion de secretos en la plataforma Git antes de publicar.

## Como obtiene la conexion

[Neros.Api/Program.cs](../../Neros.Api/Program.cs) llama a `WebApplication.CreateBuilder(args)` y luego a `GetConnectionString("Neros")`, equivalente a leer `ConnectionStrings:Neros`. El valor se entrega a EF Core mediante `UseSqlServer`. No hay una conexion fija ni se consulta el archivo de User Secrets directamente desde el codigo. Si la clave falta, la API rechaza el arranque; una clave vacia o mal formada tampoco constituye configuracion valida.

[Neros.Blazor/Program.cs](../../Neros.Blazor/Program.cs) solo obtiene `Api:BaseUrl` para su cliente HTTP. Nunca necesita la cadena SQL. Fuera de Development exige HTTPS al construir ese cliente; esa comprobacion no reemplaza una revision del despliegue.

Precedencia predeterminada, de mayor a menor, por cada clave:

1. Argumentos de linea de comandos.
2. Variables de entorno de aplicacion, por ejemplo `ConnectionStrings__Neros`.
3. User Secrets, solo en Development y para el UserSecretsId del proyecto ejecutado.
4. `appsettings.{Environment}.json`.
5. `appsettings.json`.
6. Configuracion de host como respaldo.

El ultimo proveedor que contiene una clave prevalece; se combinan claves, no se reemplaza necesariamente todo el objeto JSON. Un proveedor agregado posteriormente cambia esa precedencia. Neros no agrega un proveedor personalizado. `launchSettings.json` configura la ejecucion local, no la produccion; sus perfiles http activan Development. Mantener coherentes `DOTNET_ENVIRONMENT` y `ASPNETCORE_ENVIRONMENT`; no usar valores contradictorios.

Aunque los proveedores JSON admiten recarga, la conexion se captura al registrar el contexto. Reiniciar API tras cambiarla. Las variables de entorno nuevas requieren un proceso nuevo. No usar argumentos de comando para secretos: pueden quedar en historial o lista de procesos. No imprimir `AsEnumerable()`, `GetDebugView()` ni `dotnet user-secrets list` en logs o chats.

## Efecto de Gitignore

[.gitignore](../../.gitignore) excluye `**/appsettings.Development.json` dentro de Neros. La raiz Git esta un nivel por encima, pero este archivo aplica correctamente al subarbol. La regla no excluye los archivos `.example`.

Se ejecuto `git rm --cached` solo para ambos Development: su eliminacion queda preparada en el indice, sus copias locales permanecen y no se hizo commit. Agregar un ignore por si solo no habria detenido el seguimiento. No usar `git add -f` para estos archivos.

Al integrar esta eliminacion, otras copias de trabajo pueden perder su archivo anteriormente versionado. Cada desarrollador debe respaldar sus ajustes privados fuera del repositorio antes de actualizar y recrear solo el archivo que falte. Los ejemplos no se cargan automaticamente; el runtime sigue leyendo los nombres originales. La compilacion no depende de estos archivos opcionales. El arranque local si depende de contar con la conexion y permisos SQL correctos.

Un ignore tampoco excluye archivos del paquete de publicacion ni borra el historial. Publicar desde una copia limpia y revisar el contenido del artefacto: no distribuir Development, ejemplos, secretos ni configuraciones locales. No se modifico la politica de publicacion en esta entrega.

## Incorporar otro desarrollador

Desde la carpeta Neros, crear solo lo que falte, sin sobrescribir ajustes locales:

```powershell
foreach ($proyecto in 'Neros.Api', 'Neros.Blazor') {
    $destino = Join-Path $proyecto 'appsettings.Development.json'
    if (-not (Test-Path $destino)) {
        Copy-Item "$destino.example" $destino
    }
}
git check-ignore Neros.Api/appsettings.Development.json Neros.Blazor/appsettings.Development.json
```

Configurar la clave `ConnectionStrings:Neros` en User Secrets del proyecto API con el procedimiento de entrada oculta de [acceso multiempresa](acceso-multiempresa.md#configuracion-local). El identificador UserSecretsId no es una credencial. Cada cuenta Windows tiene su almacen propio; clonarlo no clona secretos.

Para esta instalacion: servidor localhost, base NEROSERP y autenticacion integrada de Windows. La identidad del proceso necesita permisos sobre la base. El cifrado local admite el certificado de desarrollo; no replicar esa excepcion en produccion. Instalar los scripts manuales una sola vez sobre la base vacia y ejecutar el alta interactiva documentada. No inventar una clave inicial.

Arrancar los perfiles http documentados. API en 5067 y Blazor en 5268. Ante conflicto, elegir puertos libres y ajustar `Api:BaseUrl` en Development de Blazor. Esa URL no es un secreto. La API puede escuchar sin que SQL este disponible: comprobar una operacion de acceso controlada, no solo que exista el puerto.

## Desarrollo y produccion

| Opcion | Uso recomendado | Limite |
| --- | --- | --- |
| User Secrets | Equipo de desarrollo individual | No esta cifrado; proteger cuenta/disco, no usar secretos productivos |
| Variables de entorno | CI y despliegue, suministradas por el almacen de secretos de la plataforma | Pueden leerse desde el proceso o diagnosticos; evitar volcados y valores en YAML |
| Azure Key Vault | Destino Azure con identidad administrada, permisos minimos, auditoria y rotacion | Requiere configurar acceso y proveedor o referencia de plataforma; hoy no implementado |
| Otro gestor administrado | AWS Secrets Manager, Vault u opcion propia del hosting cuando corresponda | Integrar autenticacion del servicio y definir precedencia; no instalar todos |

Recomendacion actual: mantener User Secrets en desarrollo; decidir gestor con el destino productivo. Puede suministrarse `ConnectionStrings__Neros` a la API mediante la plataforma sin cambiar Application o Domain. `Api__BaseUrl` pertenece al servidor Blazor y debe ser una URL HTTPS accesible desde el hosting, no localhost por inercia. Usar identidad de servicio o administrada compatible con SQL, permisos limitados, certificado confiable y credenciales distintas por entorno. No otorgar DDL a la cuenta de ejecucion; los scripts los aplica una identidad de despliegue controlada.

Tambien configurar hosts admitidos, HTTPS, proxy confiable, claves Data Protection persistidas/protegidas, copias y restauracion verificadas, monitorizacion y politica de rotacion. No trasladar los comandos de alta Development a endpoints publicos. La iniciativa I-01 del [roadmap](../NEROS_ERP_IMPLEMENTATION_ROADMAP.md) retiro la dependencia OpenAPI no utilizada el 2026-09-13: el paquete vulnerable ya no aparece en el arbol restaurado ni en la auditoria NuGet posterior. Esto no completa las restantes condiciones de produccion ni certifica ausencia de vulnerabilidades.

## Si aparece un secreto en Git

1. Restringir acceso y revocar o rotar la credencial en su emisor; moverla sin rotarla no revierte la exposicion.
2. Registrar solo tipo, ruta y responsables del incidente, nunca el valor. Evaluar accesos y alcance.
3. Configurar la nueva credencial en el gestor del entorno y comprobar conectividad con privilegios minimos.
4. Sustituir configuracion versionada por claves documentadas y ejemplos inocuos; revisar logs, artefactos y copias.
5. Coordinar la limpieza del historial y forks con responsables y aprobarla antes de reescribir Git. No se reescribio historial en esta revision.

## Fuentes

- [Microsoft Learn: configuracion en ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0).
- [Microsoft Learn: secretos durante desarrollo](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0).
- [Microsoft Learn: proveedor Azure Key Vault](https://learn.microsoft.com/en-us/aspnet/core/security/key-vault-configuration?view=aspnetcore-10.0).

Consulta: 2026-09-13. Las dos primeras fuentes contrastan el comportamiento observado; Key Vault es una alternativa de despliegue, no una capacidad instalada.