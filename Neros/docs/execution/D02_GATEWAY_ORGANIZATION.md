# D-02: Gateway y host Organization

Fecha: 2026-09-13. Depende de D-01 aprobado. [Estado vigente](NEROS_EXECUTION_STATUS.md).

## Implementacion

- [GatewayHost](../../src/Gateway/Neros.Gateway/GatewayHost.cs): YARP 2.3.0, rutas acotadas /api/acceso, /api/empresas y /api/v1/organization. Destinos desde configuracion validada, no query/header ni tenant hardcoded. Solo biblioteca tecnica ServiceDefaults; no referencia a Api/Domain/Persistence de propietarios.
- [OrganizationHost](../../src/Services/Organization/Neros.Organization.Api/OrganizationHost.cs): host independiente, GET /api/v1/organization/context protegido, sin SQL ni dependencia de capas globales. Devuelve el contexto del token, NO empresas persistidas ni decision fresca de membresia. No usa el manifiesto NEROS-TEST como fallback.
- [BFF](../../Neros.Blazor/Program.cs): Gateway:BaseUrl tiene prioridad sobre Api:BaseUrl. Se mantiene destino anterior si no se configura Gateway, para no romper el arranque existente. No se duplican endpoints/cookies ni se reescribe UI.
- JWT para Organization exige audience neros.organization, scope organization.read y permiso Organization.Context.Read. Cada servicio valida firma/emisor/vigencia. No emitir tokens desde Gateway ni transformar tokens opacos a JWT.
- Rutas legacy son anonimas SOLO en Gateway: el esquema compatibility no autentica ni crea claims, y la API propietaria conserva su validacion de sesiones y recursos. No consultar OIDC por un token opaco. La API no se vuelve anonima por pasar por proxy.
- Allowlist de headers de request; cookies, headers tenant/actor y forwarded no pasan. Correlacion normalizada y propagacion W3C/OTel. Organization rechaza contexto de identidad por headers en acceso directo.
- Sin retries YARP de escrituras; limite de concurrencia 32, cola cero, conexion 3 s y timeout de inactividad 10 s. BFF conserva presupuesto de resiliencia D-01; limites de login actuales BFF 10/min y API 60/min no se sustituyen por quotas SaaS ficticias. El limite de API sigue viendo al BFF/proxy local, como en el flujo anterior.
- HTTPS requerido fuera de Development tanto en destinos como requests entrantes. No se confia en X-Forwarded-Proto del cliente. El despliegue tras terminacion TLS confiable requiere configurar proxies/red en D-04; no desactivar validacion de certificados.

## Ejecucion local

Perfiles http solo de desarrollo: API 5067, Gateway 5069, Organization 5070. No contienen secretos. Identity:Authority apunta a https://localhost:7180/ como direccion prevista de desarrollo, NO como proveedor ya implementado. Sin emisor real Organization deniega; login legacy puede funcionar con Identity/Organization nuevos fuera de servicio. Firma RSA efimera se usa solo en tests, nunca en los hosts productivos.

Ejecutar cada servidor en una terminal separada y detenerlos al terminar:

```powershell
dotnet run --project ./Neros.Api --launch-profile http
dotnet run --project ./src/Services/Organization/Neros.Organization.Api --launch-profile http
dotnet run --project ./src/Gateway/Neros.Gateway --launch-profile http
```

En la terminal del BFF:

```powershell
$env:Gateway__BaseUrl = 'http://localhost:5069/'
dotnet run --project ./Neros.Blazor --launch-profile http
Remove-Item Env:Gateway__BaseUrl
```

Produccion no usa estos perfiles; Services:Compatibility, Services:Organization, Identity:Authority y Gateway:BaseUrl se configuran por entorno. No poner credenciales en URLs o JSON. Sin configuracion valida los hosts nuevos rechazan el arranque. No se ha aprovisionado infraestructura dedicada por declarar el tenant NEROS-TEST.

## Evidencia y limites

Las diez regresiones [AccesoMultiempresaTests](../../tests/Neros.Tests/AccesoMultiempresaTests.cs) pasan a traves de Gateway, con SQL temporal y navegador real (login, rechazo, CSRF, empresas, actividad, cambios, revocacion, logout y limites). El entorno actualiza el destino BFF sin modificar el contrato HTTP. No se tocaron datos/cuentas existentes.

[GatewayOrganizationTests](../../tests/Neros.Tests/GatewayOrganizationTests.cs): host directo 401 sin token y 400 con header de identidad; proxy preserva contexto firmado sin headers falsificados; audience/permiso invalidos denegados; health live 200, readiness 401/403; spans cliente/servidor con mismo TraceId/correlacion. Organization detenido da 502 ProblemDetails sanitizado. Metadata Identity 503 se simula con HttpMessageHandler: cero consultas para legacy, Organization 401; no es ensayo de proveedor OIDC real D-06. Treinta y dos solicitudes retenidas y la adicional 429, sin cola.

Gate D-02 aprobado: suite completa 41/41, build de trece proyectos y auditoria NuGet sin vulnerabilidades conocidas tras incorporar estos hosts. No certifica broker, Outbox/Inbox durable, collector remoto, SQL independiente Organization, identidad productiva ni carga ERP. D-03 debe probar broker y almacenamiento reales antes de migracion D-05. No crear otros servicios de negocio copiando este endpoint tecnico. Servidores de validacion detenidos al terminar.