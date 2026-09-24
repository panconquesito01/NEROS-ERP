# Validacion de acceso multiempresa

## Errores de acceso sin recarga

Revision del 13 de septiembre de 2026: ampliada la prueba existente NavegadorLoginEmpresasTemaYSalidaAsync y aprobada (1/1). Correo inexistente y clave incorrecta devuelven 401 con aviso generico, mantienen URL, marcador del documento y correo escrito, y rehabilitan el envio. Se verificaron recuperacion tras fallo de red simulado, antiforgery con 400 JSON, exito posterior con cookie y flujo empresas/inicio/salida. Tambien se comprobo rechazo tradicional sin JavaScript y limite real con 429 JSON sin Location. Todo usa cuentas y base SQL temporales, no credenciales del usuario.

Capturas login-error-{1440,390}-{claro,oscuro}.png en .impeccable/review; inspeccion visual movil clara realizada, sin overflow. Los rechazos ya no reinician el fondo animado porque no se reemplaza el documento. Las comprobaciones de redireccion descritas en revisiones anteriores corresponden al envio tradicional.

## Red animada y catalogo comercial

Revision del 13 de septiembre de 2026: ocho combinaciones de 320, 390, 768 y 1440 px, en claro y oscuro, comprobadas con Edge y Playwright. El canvas tiene pixeles dibujados y cambia entre frames; pausa y reanudacion funcionan, y movimiento reducido mantiene una imagen estatica. Se comprobaron foco del titulo sin recuadro, Tab hacia correo, apertura de detalles, estados comerciales, carga de iconos locales y ausencia de overflow horizontal y errores JavaScript.

Capturas locales login-red-{ancho}-{tema}.png en .impeccable/review; inspeccion visual de escritorio oscuro y movil claro. La prueba existente NavegadorLoginEmpresasTemaYSalidaAsync paso (1/1), sin cambiar cuentas del usuario. CSS y solucion compilados; persiste NU1903 por Microsoft.OpenApi 2.0.0. No es una auditoria integral de accesibilidad o rendimiento. El catalogo no implementa los modulos anunciados como En preparacion.

## Confirmacion de autenticacion y arranque

Revision adicional del 13 de septiembre de 2026: cuenta administrativa y permiso global confirmados directamente en NEROSERP, sin consultar contrasena ni hash. La autenticacion usa Identity y SQL Server, independientemente del IDE.

La prueba existente NavegadorLoginEmpresasTemaYSalidaAsync paso tras actualizar su selector de Mantener sesion: cubre login valido con una cuenta en SQL temporal, empresas, temas y cierre. No se utilizo la contrasena del usuario ni se crearon proyectos de pruebas nuevos. La primera ejecucion fallo por buscar la etiqueta antigua; no fue un fallo de autenticacion.

Con Blazor activo y API detenida se reprodujo el error de red mediante el formulario con datos ficticios: redireccion a estado=conexion y aviso de que la contrasena no se ha comprobado. El registro contiene ConnectionError, sin datos de credenciales. Se agrego Neros.slnLaunch para Visual Studio, con ambos proyectos en Start; JSON y rutas validados. La seleccion del perfil en Visual Studio no se automatizo.

Diseno neutro actualizado: ocho capturas login-neutral-{320,390,768,1440}-{light,dark}.png en .impeccable/review, sin overflow horizontal ni errores JavaScript. Revision visual de escritorio oscuro y movil claro realizada. No equivale a certificacion de accesibilidad. La compilacion mantiene la advertencia previa NU1903; los apartados siguientes describen revisiones anteriores.

## Revision del login y estrategia comercial

Confirmacion local del 13 de septiembre de 2026, posterior al refinamiento visual. No se crearon proyectos de pruebas ni se modificaron los existentes. Los nueve casos de la seccion historica siguiente no se presentan como una nueva ejecucion de esta revision.

| Comprobacion | Resultado y alcance |
| --- | --- |
| Tailwind y compilacion .NET | Correctos durante la implementacion; permanece NU1903 de Microsoft.OpenApi 2.0.0 |
| Diagnosticos del editor | Sin errores en Login.razor, tailwind.css y experience.js |
| Navegador aislado | Edge con Playwright ya instalado; sin errores JavaScript |
| Adaptacion | 320, 390, 768 y 1440 px, claro y oscuro; ocho capturas actuales sin desbordamiento horizontal ni colision de marca y selector |
| Apariencia | Seleccion manual de tema y seguimiento del tema de sistema comprobados |
| Clave | Mostrar/ocultar comprobado; icono eye-off servido correctamente |
| Correo recordado | Optativo, persiste al recargar y se elimina al desmarcar; no activa mantener sesion |
| Envio | Estado ocupado y boton deshabilitado; restauracion tras recarga comprobada con evento cancelado, sin enviar credenciales |
| Estados y teclado | Aviso de credenciales y ayuda desplegable presentes; modalidad de teclado detectada |
| Configuracion local | Ambos Development conservados en disco y retirados del indice; ejemplos JSON y regla de exclusion comprobados |

Las capturas `login-nuevo-{compacto,movil,tablet,escritorio}-{light,dark}.png` estan en `.impeccable/review/`, fuera de versionado. No contienen informacion de clientes. La comprobacion aislada del correo utilizo una direccion ficticia en un contexto de navegador descartado al finalizar.

No se repitio el recorrido autenticado contra SQL ni se creo un administrador. Tampoco se certifican accesibilidad, contraste de todos los estados, dispositivos fisicos, aviso de Caps Lock o funcionamiento con almacenamiento bloqueado. El modo de movimiento reducido se activo durante la comprobacion, pero no se midieron todas las animaciones. La recuperacion automatica de contrasena sigue pendiente; el login ofrece ayuda, no un servicio de restablecimiento.

La [estrategia comercial](../gerencial/estrategia-comercial-producto.md) distingue producto disponible y roadmap, y marca sus precios como simulaciones internas. No se construyo la web comercial ni se habilitaron planes de venta.

## Resultado

Validacion local realizada el 12 de septiembre de 2026.

| Comprobacion | Resultado |
| --- | --- |
| Tailwind CSS | Generacion correcta |
| Compilacion .NET | Correcta; advertencia preexistente NU1903 en Microsoft.OpenApi 2.0.0 |
| Pruebas automatizadas | 9 correctas, 0 fallidas, 0 omitidas |
| SQL Server localhost | Conexion integrada comprobada; esquema instalado en NEROSERP |
| Datos de desarrollo | 8 tablas; 0 usuarios, 0 empresas; sin datos de prueba persistentes |
| Limpieza de pruebas | 0 bases NerosTests_ restantes |
| Detector Impeccable | Sin hallazgos en la pasada mecanica |
| Revision visual independiente | Tres ajustes solicitados y resueltos; veredicto final ship sobre esos ajustes |
| Recursos locales | Iconos incluidos en el manifiesto de compilacion |
| Graphify | Grafo de codigo actualizado: 966 nodos y 1119 relaciones; SQL y extraccion semantica documental pendientes |

## Casos cubiertos

1. Login real y listado limitado a empresas autorizadas; consulta y seleccion de empresa ajena rechazadas.
2. API sin sesion devuelve 401; datos invalidos, 400; inicio web anonimo redirige al login.
3. Revocacion de membresia efectiva en la siguiente consulta.
4. Empresa inactiva inaccesible.
5. Logout invalida el token en servidor.
6. Cambio de sello y vencimiento invalidan sesiones; mantener sesion extiende el vencimiento a siete dias.
7. Bloqueo tras cinco intentos fallidos.
8. Actividad limitada al usuario y a la empresa activa.
9. Navegador: login, mostrar/ocultar clave, recordar sesion, busqueda y estado vacio, CSRF rechazado, cambio de empresa, temas sistema/claro/oscuro, logout y proteccion posterior del inicio.

La prueba de navegador genero 12 capturas: login, empresas e inicio a 1440 y 390 px, en claro y oscuro. Estan en `.impeccable/review/`, excluidas de versionado. Contienen unicamente datos de prueba identificados como tales. Se comprobo ausencia de desbordamiento horizontal en esos tamanos.

## Revision de acabado

| Antes | Despues | Motivo |
| --- | --- | --- |
| Datos operativos de 10-11 px | Datos de 13-14 px; encabezados de 12 px | Lectura mas comoda |
| Campos de 13 px en movil | Campos de 16 px | Mejor lectura y prevencion del zoom automatico de iOS |
| Actualizar actividad mediante fragmento | Formulario GET con parametro unico y recarga real | Garantizar nueva consulta |

No se verifico un iPhone fisico, un lector de pantalla ni un despliegue productivo. Las capturas y comprobaciones automatizadas no equivalen a una certificacion WCAG o de seguridad.

## Observaciones

- La fuente Telerik configurada globalmente en el equipo devolvio 401. Se uso NuGet publico para restaurar las dependencias de prueba, sin modificar esa configuracion.
- Microsoft.OpenApi 2.0.0 tiene la advertencia preexistente GHSA-v5pm-xwqc-g5wc. Debe actualizarse antes de produccion; no se cambio una dependencia ajena a esta entrega.
- Browserslist informa que su catalogo caniuse-lite esta desactualizado; no impidio generar CSS.
- SQL Server advierte sobre la longitud teorica de claves compuestas. Los usuarios actuales se identifican con GUID de texto generado por Identity; login externo no esta implementado.
- No se creo el primer administrador: su clave debe introducirla el responsable directamente en la terminal local.
- Graphify no indexo los scripts SQL porque falta `tree_sitter_sql`. La actualizacion ejecutada cubre codigo; los documentos nuevos requieren una extraccion semantica posterior.

Repetir las pruebas:

```powershell
dotnet restore .\tests\Neros.Tests\Neros.Tests.csproj --source https://api.nuget.org/v3/index.json
dotnet test .\tests\Neros.Tests\Neros.Tests.csproj --no-restore -v minimal
```