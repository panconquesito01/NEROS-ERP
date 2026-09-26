---
name: Neros ERP
description: Sistema visual implementado para acceso y trabajo multiempresa.
colors:
  claro-canvas: "#f5f7f6"
  claro-surface: "#ffffff"
  claro-soft: "#edf1ee"
  claro-ink: "#202d27"
  claro-muted: "#5f7066"
  claro-line: "#dbe3de"
  claro-accent: "#176748"
  claro-accent-hover: "#105338"
  claro-on-accent: "#ffffff"
  claro-tint: "#e5f2e9"
  claro-coral: "#ab4e3d"
  claro-coral-tint: "#fbede8"
  claro-error: "#a9323f"
  claro-error-tint: "#fff0f1"
  oscuro-canvas: "#151a18"
  oscuro-surface: "#1b221e"
  oscuro-soft: "#252e28"
  oscuro-ink: "#eef3ef"
  oscuro-muted: "#a5b6aa"
  oscuro-line: "#344239"
  oscuro-accent: "#86d8a7"
  oscuro-accent-hover: "#a0e6bb"
  oscuro-on-accent: "#14291d"
  oscuro-tint: "#233e2d"
  oscuro-coral: "#e7a08d"
  oscuro-coral-tint: "#3b2a25"
  oscuro-error: "#ffb1b9"
  oscuro-error-tint: "#3d2529"
typography:
  body:
    fontFamily: "Manrope, Aptos, Segoe UI, sans-serif"
    fontSize: "14px"
    lineHeight: 1.6
    letterSpacing: "0"
  title:
    fontFamily: "Manrope, Aptos, Segoe UI, sans-serif"
    fontSize: "26px"
    fontWeight: 750
    lineHeight: 1.35
  title-mobile:
    fontFamily: "Manrope, Aptos, Segoe UI, sans-serif"
    fontSize: "23px"
    fontWeight: 750
    lineHeight: 1.35
  section:
    fontFamily: "Manrope, Aptos, Segoe UI, sans-serif"
    fontSize: "15px"
    fontWeight: 750
  label:
    fontFamily: "Manrope, Aptos, Segoe UI, sans-serif"
    fontSize: "13px"
    fontWeight: 700
  supporting:
    fontFamily: "Manrope, Aptos, Segoe UI, sans-serif"
    fontSize: "12px"
rounded:
  sm: "5px"
  control: "6px"
  item: "8px"
spacing:
  1: "4px"
  2: "8px"
  3: "12px"
  4: "16px"
  5: "20px"
  6: "24px"
  8: "32px"
  10: "40px"
components:
  button-primary-claro:
    backgroundColor: "{colors.claro-accent}"
    textColor: "{colors.claro-on-accent}"
    rounded: "{rounded.control}"
  button-primary-claro-hover:
    backgroundColor: "{colors.claro-accent-hover}"
    textColor: "{colors.claro-on-accent}"
  button-primary-oscuro:
    backgroundColor: "{colors.oscuro-accent}"
    textColor: "{colors.oscuro-on-accent}"
    rounded: "{rounded.control}"
  button-primary-oscuro-hover:
    backgroundColor: "{colors.oscuro-accent-hover}"
    textColor: "{colors.oscuro-on-accent}"
  button-secondary-claro:
    backgroundColor: "{colors.claro-surface}"
    textColor: "{colors.claro-ink}"
    rounded: "{rounded.control}"
  button-secondary-oscuro:
    backgroundColor: "{colors.oscuro-surface}"
    textColor: "{colors.oscuro-ink}"
    rounded: "{rounded.control}"
  input-claro:
    backgroundColor: "{colors.claro-surface}"
    textColor: "{colors.claro-ink}"
    rounded: "{rounded.control}"
  input-oscuro:
    backgroundColor: "{colors.oscuro-surface}"
    textColor: "{colors.oscuro-ink}"
    rounded: "{rounded.control}"
  navigation-active-claro:
    backgroundColor: "{colors.claro-tint}"
    textColor: "{colors.claro-accent}"
    rounded: "{rounded.control}"
  navigation-active-oscuro:
    backgroundColor: "{colors.oscuro-tint}"
    textColor: "{colors.oscuro-accent}"
    rounded: "{rounded.control}"
---

# Sistema de diseno: Neros ERP

## Overview

**Creative North Star: "Registro operativo simplificado"**

Neros presenta un entorno de trabajo multiempresa sobrio, de densidad moderada y lectura directa. Verde, coral y neutros claros o carbon distinguen acciones, contexto y estados sin convertir el inicio en una portada comercial. Manrope, divisores discretos y controles de geometria estable sostienen la identidad.

Este documento registra la implementacion de acceso, inicio y empresas, con refinamiento del login al 2026-09-13. No define modulos futuros ni una composicion obligatoria para todas las pantallas. La evidencia visual y funcional prevalece sobre los valores residuales de configuracion.

**Key Characteristics:**

- Contexto empresarial y acciones reconocibles, con secciones abiertas y listas escaneables.
- Dos paletas efectivas y tres preferencias de apariencia: sistema, claro y oscuro.
- Texto, iconos y estados semanticos combinados; movimiento breve y condicionado.
- Errores y vacios explicitos, sin indicadores ni capacidades de negocio ficticias.

Fuentes: [PRODUCT.md](PRODUCT.md), [contrato de superficie](.impeccable/surfaces/acceso-inicio.md), [estilos](Neros.Blazor/Styles/tailwind.css), [configuracion Tailwind](tailwind.config.js), [documento HTML](Neros.Blazor/Components/App.razor), componentes y scripts enlazados abajo. El frontmatter extrae valores reales; los prefijos `claro-` y `oscuro-` distinguen las dos asignaciones de cada variable CSS, no crean nuevas variables de la aplicacion.

Existe el registro auxiliar [.impeccable/design.json](.impeccable/design.json), generado para el sistema inicial. Este documento y el codigo describen el refinamiento posterior del acceso; el registro auxiliar no se ha reextraido y sus ejemplos no representan todos los ajustes particulares del login.

## Colors

### Primary

El verde Neros es `accent`: accion principal, enlace, seleccion, foco, navegacion activa y confirmacion. `accent-hover` modifica el fondo de la accion principal; `on-accent` mantiene el texto adecuado al tema. `tint` acompana iconos, monogramas y estados activos sin llenar grandes superficies de color.

### Secondary

El coral distingue el acceso a actividad mediante `coral` y `coral-tint`. Es un acento secundario observado, no un estado de error ni otra accion principal.

### Neutral

`canvas` es el fondo general; `surface`, el de barra lateral, controles y accesos; `soft`, el de cabeceras de tabla, avisos neutros y algunos estados hover. `ink` sostiene texto principal, `muted` texto auxiliar y `line` bordes y divisores. El oscuro cambia todas estas asignaciones; no es una inversion automatica de colores.

El acceso comparte los neutros del entorno operativo. Recupera la cabecera tonal verde y el fondo de marca anterior, con coral discreto como contrapunto. A peticion del usuario, los refinamientos deben conservar esta identidad: no reemplazarla por el tratamiento gris y centrado descartado.

### Estados y temas

`error` y `error-tint` distinguen fallos; las confirmaciones reutilizan verde y tinte. Los enlaces dentro de avisos heredan el color del aviso, con subrayado y peso fuerte, en vez de conservar el verde global sobre cualquier fondo.

[theme-init.js](Neros.Blazor/wwwroot/theme-init.js) se carga antes de las hojas de estilo. Lee `neros.theme` de `localStorage`, acepta `system`, `light` o `dark` y resuelve `data-theme` a claro u oscuro. Sistema sigue `prefers-color-scheme`; los botones reflejan la preferencia, no solo el tema resuelto. Los cambios del sistema y de almacenamiento entre pestanas vuelven a aplicar el tema. Los errores de almacenamiento se toleran, aunque impiden garantizar persistencia.

**The Contraste Por Contexto Rule.** Texto, fondo y estado usan el mismo conjunto semantico del tema; los avisos conservan enlaces legibles y distinguibles sin depender solo del color.

## Typography

Una sola familia aplicada: Manrope, con las alternativas del token `body`. [App.razor](Neros.Blazor/Components/App.razor) solicita Google Fonts externamente, con `display=swap` y pesos 400, 500, 600, 700 y 800. No hay fuente Manrope autoalojada en esta carga. Sin acceso al proveedor se usan las alternativas instaladas y pueden cambiar las metricas.

La jerarquia reutilizable esta en el frontmatter. El cuerpo operativo combina 13 px en campos, acciones y filas con el cuerpo base; textos auxiliares usan el rol `supporting`. Las cabeceras de pagina se reducen en movil. Los titulos usan ajuste equilibrado de lineas; los nombres largos de empresa y usuario permiten corte de palabra.

El acceso tiene un titulo alineado a la izquierda de 30 px, 25 px hasta 540 px, peso 800 e interlineado 1.3; Neros ocupa la segunda linea en color de acento. Campos y envio usan 16 px; etiquetas, opciones y ayuda, 14 px; introduccion, 15 px; cierre del formulario, 13 px; pie de pagina, 12 px. La marca general usa 27 px y peso 800; en la cabecera del login usa 30 px y 25 px hasta 540 px. No es una escala hero para otras pantallas.

La fuente se solicita como eje variable `wght@400..800`, por lo que los pesos intermedios 650 y 750 usados en la hoja se representan con fidelidad cuando el proveedor esta disponible. `Space Grotesk` y la paleta residual `neros` se retiraron de la configuracion de Tailwind.

**The Escala Operativa Rule.** No extrapolar marca ni titulo de acceso a paneles compactos; mantener jerarquia por funcion y espaciado entre letras cero, sin escalar la fuente con el ancho del viewport.

## Layout

- Acceso: pagina de altura minima `100svh`, cabecera con marca y selector de tema, herramienta centrada de ancho maximo 480 px, radio 8 px y relleno 32/36/26 px. Cabecera tonal con titulo a la izquierda e icono a la derecha, separada por un divisor; formulario alineado a la izquierda. Fondo con transiciones verde y coral y red animada de nodos, sin el monograma decorativo anterior. En movil, relleno 28/22/24 px. Bajo el formulario aparecen el enlace Explorar modulos y el control de pausa, seguidos de una franja comercial abierta. No hay metricas ficticias.
- Entorno autenticado: desde 768 px, rejilla de barra lateral de 236 px y contenido `minmax(0, 1fr)`. La barra lateral es fija (`sticky`, altura de viewport) para mantener usuario y empresa visibles al desplazar. El contenido tiene ancho maximo de 1300 px y margen horizontal automatico.
- Cabecera: pegajosa, altura minima de 68 px, fondo `canvas` translucido con desenfoque de 12 px. Contiene una ruta de ubicacion (empresa activa, enlazada a Mis empresas, y seccion actual con `aria-current="page"`), selector de tema y boton Salir con icono y texto.
- Hasta 767 px: la navegacion pasa a una barra inferior fija con icono y etiqueta (52 px, respeta `safe-area-inset-bottom`); se ocultan marca, usuario y selector de empresa de la barra lateral. La cabecera muestra el monograma de marca, la empresa activa como enlace y Salir solo con icono. Contenido con relleno de 24 px vertical y 16 px horizontal.
- Accesos: una columna inicialmente y dos desde 640 px. El contexto empresarial admite envoltura; su accion ocupa todo el ancho en movil. Las filas de empresas desplazan el rol a una linea inferior y permiten nombres largos.
- Actividad: tabla con desplazamiento horizontal dentro de su contenedor. No se transforma en tarjetas ni oculta columnas en movil.
- Ritmo: utilidades Tailwind sobre pasos de 4 px, con ajustes locales observados. No existe una escala propia de espaciado adicional a la configuracion y CSS.

Los cortes de 640, 768 y 1024 px proceden de Tailwind 3.4; los ajustes particulares de 540 y 767 px estan en la hoja fuente. No se documentan otros cortes sin uso en las superficies revisadas.

## Elevation & Depth

La profundidad operativa proviene de tonos, bordes de 1 px y divisores. El login es una herramienta delimitada con una sombra local de 24 px de desplazamiento y 64 px de desenfoque, mas una sombra menor de contacto. No constituye una escala general de elevacion. Contexto empresarial, actividad y listado de empresas siguen siendo secciones abiertas; los accesos repetidos si tienen contenedor delimitado.

El foco general dibuja un contorno de 2 px en verde con separacion de 4 px para enlaces, botones, entradas y `summary`. El contenedor de entrada usa borde verde y contorno de 2 px en tinte con separacion de 1 px al recibir foco interior; el input interior elimina su contorno propio. El salto al contenido y el aviso global de error son capas fijas, no superficies con elevacion ornamental.

Blazor conserva `FocusOnNavigate` sobre el h1 para anunciar cambios de pagina. Solo `h1[tabindex="-1"]:focus` oculta su contorno automatico; no eliminar globalmente los indicadores de foco de controles interactivos.

**The Separacion Operativa Rule.** Preservar tonos y divisores en las secciones de trabajo; la profundidad particular del login no justifica convertir cada seccion en una tarjeta flotante.

## Shapes

Las esquinas reutilizables estan en `rounded`: pequenas para segmentos de tema e insignias, de control para botones, campos, avisos y navegacion, y de item para accesos, monogramas y marca. Los bordes son finos, sin contornos ornamentales.

El avatar es circular. El simbolo de acceso es log-in de Lucide de 32 px, en un espacio de 40 por 48 px sin caja decorativa; el monograma N permanece en la marca de cabecera y navegacion. Los iconos tienen caja estable de 20 px por defecto, con ajustes entre 15 y 32 px segun componente. Los contenedores de marca, monograma y acceso reservan dimensiones fijas para evitar desplazamientos.

## Components

### Botones y controles de icono

Las acciones combinan icono y texto cuando necesitan una etiqueta explicita. El boton comun tiene altura minima de 44 px, relleno horizontal de 16 px, separacion de 8 px y texto de 13 px seminegrita. El primario usa verde; el secundario, superficie y borde neutro. El envio de acceso ocupa el ancho, tiene altura minima de 54 px y texto de 16 px. Mientras envia, sustituye la flecha por progreso y un texto de estado; reduced-motion detiene su giro.

Los botones de herramienta tienen caja de 40 por 40 px. Tema usa 34 por 34 px; mostrar contrasena, 40 por 44 px en el acceso. Se conservan estos tamanos como evidencia, sin afirmar un objetivo tactil uniforme de 44 px. Las acciones solo con icono incluyen nombre accesible y `title`; no existe un tooltip personalizado.

Hover se aplica solo con `hover: hover` y `pointer: fine`: cambia fondos o bordes segun componente. Los botones deshabilitados reducen opacidad a .65 y muestran cursor de espera. En el login, `:user-invalid` distingue el borde del campo invalido sin marcarlo antes de la interaccion.

### Campos y acceso

[PaginaAcceso.razor](Neros.Blazor/Components/Features/Acceso/PaginaAcceso.razor) usa formulario HTML, etiquetas asociadas, tipos email y password, `required`, limites de longitud y autocompletado. En el acceso el marco tiene altura minima de 54 px, relleno horizontal de 12 px y separacion de 12 px; el input interior alcanza 52 px y texto de 16 px en escritorio y movil. El placeholder usa `muted` con opacidad 1. El foco del marco usa contorno accent de 2 px separado 2 px. No abre automaticamente el teclado movil al cargar.

Mostrar contrasena cambia tipo, icono eye/eye-off, `aria-pressed`, nombre accesible y `title`. Un aviso indica bloqueo de mayusculas. Recordar correo y mantener sesion son casillas nativas independientes: el correo solo se guarda tras consentimiento y envio, y se elimina al desmarcar. Es un dato personal, no una credencial; la contrasena nunca se almacena. La ayuda usa `details` y `summary`, no un flujo automatico de recuperacion. Errores usan `role="alert"`; confirmacion de salida usa `role="status"`.

### Navegacion y tema

[MainLayout.razor](Neros.Blazor/Components/Layout/MainLayout.razor) ofrece salto al contenido principal, ruta de ubicacion en cabecera y salida mediante formulario. [NavMenu.razor](Neros.Blazor/Components/Layout/NavMenu.razor) presenta Inicio y Mis empresas, estado activo en verde sobre tinte y navegacion con nombre accesible. El selector de empresa y el avatar usan iniciales ([Presentacion.cs](Neros.Blazor/Components/Shared/Presentacion.cs)). Los enlaces tienen 44 px minimos en escritorio y 52 px en la barra inferior movil.

[SelectorTema.razor](Neros.Blazor/Components/Shared/SelectorTema.razor) agrupa tres botones: sistema, claro y oscuro. Cada uno tiene icono, `aria-label`, `title` y `aria-pressed`. Son botones recorridos con Tab, no un patron de pestanas con flechas. La inicializacion JavaScript sincroniza la seleccion, incluida la vuelta mediante historial.

[SelectorIdioma.razor](Neros.Blazor/Components/Shared/SelectorIdioma.razor) va siempre junto al tema dentro de [Preferencias.razor](Neros.Blazor/Components/Shared/Preferencias.razor) (cabecera autenticada, acceso y paginas de estado). Es un `details` con icono `languages` y el codigo actual (`ES`, `EN`, `PT`; en la cabecera movil solo el icono), misma altura y borde que el grupo de tema. Abre un menu de 176 px anclado a la derecha, con entrada de 160 ms `--ease-out` desde `scale(.97)` y origen arriba a la derecha; con movimiento reducido solo cambia opacidad. Cada idioma se muestra en su propio nombre nativo con `lang`, el actual con `aria-current`, tinte verde y check. Es un formulario POST nativo a `/idioma` que conserva la ruta actual, asi que funciona sin JavaScript; el script solo cierra el menu con clic exterior o Escape y devuelve el foco al disparador.

### Empresas, accesos y estados

[PaginaEmpresas.razor](Neros.Blazor/Components/Features/Empresas/PaginaEmpresas.razor) con [FilaEmpresa.razor](Neros.Blazor/Components/Features/Empresas/FilaEmpresa.razor) presenta cada empresa como boton de formulario de ancho completo (nombre accesible "Entrar a {empresa}"), con monograma, nombre, identificacion, rol y accion Entrar. La empresa activa es un enlace directo a Inicio ("Activa · Continuar"), con tinte y borde izquierdo verde, sin reenviar la seleccion. Al elegir una empresa, su flecha se sustituye por progreso y el resto de filas se atenuan y deshabilitan para evitar selecciones dobles.

El buscador aparece desde 6 empresas; con menos no aporta. Filtra localmente nombre, codigo e identificacion, ignorando mayusculas y diacriticos; `/` lo enfoca y `Escape` lo limpia. Un texto con `role="status"` anuncia "X de N empresas" mientras hay consulta. La insignia de cabecera refleja empresas disponibles. La ausencia de asignaciones ofrece una explicacion y una accion para volver a consultar.

Tras un acceso correcto, si la cuenta tiene exactamente una empresa autorizada, el servidor la selecciona y navega a Inicio (o al destino `ReturnUrl` local validado); con varias, a Mis empresas. Si la preseleccion falla, se degrada a Mis empresas sin bloquear el acceso.

Los accesos del inicio usan borde, superficie y relleno de 20 px, con icono de caja fija y flecha. Las insignias comunican disponibilidad o actividad mediante texto; no son filtros. Los avisos de error autenticados usan `role="alert"` y enlaces para reintentar donde existen.

### Actividad y refresco

[PaginaInicio.razor](Neros.Blazor/Components/Features/Inicio/PaginaInicio.razor) saluda por nombre de pila y muestra contexto empresarial, rol y actividad recibida del servicio, con vacio explicito. La tabla tiene `caption` visualmente oculto, encabezados con `scope="col"` y fechas en elementos `time`. El script muestra tiempo relativo en espanol ("Hace 5 minutos", "Ayer, 14:30") y debajo la fecha absoluta en zona horaria del navegador, omitida si coincide; sin el script permanece la fecha generada en servidor. Bajo la tabla, una nota indica a quien avisar si no se reconoce una entrada.

Actualizar actividad es un boton con icono y texto (solo icono en movil) dentro de un formulario GET a `/home#actividad`, con parametro `actualizar` generado mediante GUID y navegacion mejorada desactivada. Solicita un documento nuevo y conserva el destino de actividad. Mientras carga, el icono gira; con movimiento reducido permanece quieto.

Ventas, inventario, finanzas y consolidacion se muestran dentro de un `details` como En preparacion, sin acciones que simulen modulos funcionales.

### Movimiento y teclado

La entrada usa opacidad y desplazamiento vertical: 220 ms con `cubic-bezier(0.23, 1, 0.32, 1)`, desde opacidad .3 y 6 px mediante `@starting-style`. El estado base es completamente visible, incluso si el navegador no admite esa regla. No hay secuencia escalonada.

[login-network.js](Neros.Blazor/wwwroot/login-network.js) define `neros-red`: canvas decorativo a toda anchura, con 60 nodos, conexiones y pulsos de avance continuo. Usa los colores de cada tema, limite de 30 dibujos por segundo y escala de pixeles de hasta 2. El formulario permanece opaco y quieto. No representa IA implementada, datos reales ni actividad del sistema.

Un boton con iconos locales de pausa y reproduccion detiene o reanuda el fondo. La preferencia de movimiento reducido muestra una red estatica y deshabilita la reproduccion. La animacion se suspende cuando la pestana esta oculta o el fondo sale de la vista; al desconectar el elemento se liberan el frame, los observadores y los eventos. El formulario y el catalogo funcionan sin este script.

### Catalogo comercial del acceso

La seccion Tu negocio, conectado presenta seis areas en filas desplegables nativas, sin carrusel automatico ni tarjetas anidadas. Usa tres columnas en escritorio, dos hasta 767 px y una hasta 540 px. Acceso multiempresa se marca Disponible; Ventas y facturacion, Inventarios, Compras y proveedores, Finanzas y contabilidad y Gestion e informes se marcan En preparacion. Cada area combina un beneficio comercial con un detalle de alcance; las capacidades futuras no se presentan como operativas ni tienen fecha de lanzamiento confirmada.

### Interacciones y accesibilidad

Los botones transicionan transformacion durante 160 ms con la misma curva y fondo durante 160 ms con `ease`. La pulsacion reduce escala a .98 solo con puntero fino, hover disponible y modalidad distinta de teclado. La flecha de modulos rota al abrir `details`, sin transicion propia.

Con `prefers-reduced-motion: reduce`, entradas y botones eliminan transformacion y conservan un desvanecimiento de opacidad de 150 ms; la entrada comienza en .8. No se debe describir este modo como ausencia absoluta de animacion. Con `data-input="keyboard"`, ambos eliminan transiciones y transformaciones. [experience.js](Neros.Blazor/wwwroot/experience.js) detecta `keydown` y `pointerdown`, persiste modalidad en `sessionStorage` y la inicializacion de tema la restaura antes del contenido.

**The Movimiento Condicionado Rule.** Conservar los estados finales visibles y respetar las ramas de teclado y movimiento reducido; no agregar movimiento imprescindible para comprender una accion.

### Envio, iconos y dependencias

Los formularios marcados como ocupados usan `aria-busy`, deshabilitan envio y bloquean duplicados; el login sustituye la etiqueta por Verificando acceso. `pageshow` restablece botones y etiquetas para la vuelta desde historial. El login envia FormData con antiforgery mediante fetch al servidor Blazor: un rechazo actualiza el aviso con role alert sin navegar ni perder los campos. El envio se rehabilita tras errores de credenciales, red o servicio. Solo el exito (204 con cookie) navega a empresas. Sin JavaScript conserva el POST HTML tradicional; seleccion de empresa y salida siguen usando navegacion completa.

[Icono.razor](Neros.Blazor/Components/Shared/Icono.razor) representa SVG locales de `/icons/{Nombre}.svg` como mascara CSS y hereda `currentColor`. Los iconos son decorativos (`aria-hidden="true"`); el nombre accesible corresponde al control. La dependencia de origen es `lucide-static`, declarada en [package.json](package.json); los iconos de esta UI no se solicitan a un CDN. Su renderizado requiere que el archivo local y la mascara CSS esten disponibles. No es una biblioteca cliente de JavaScript ni una fuente de iconos.

Manrope si depende de Google Fonts. No confundir disponibilidad local de iconos con autosuficiencia total de recursos. El favicon es un recurso separado de esta iconografia.

### Cierre ship y limites de verificacion

Resultado de cierre recibido: **ship**, con **9/9 pruebas previas aprobadas**. Esta pasada documental no vuelve a ejecutar esas pruebas ni constituye una auditoria WCAG completa. Se comprueba en fuente la presencia de los tres ajustes de cierre:

1. Legibilidad de enlaces dentro de avisos: color heredado del estado, subrayado y peso fuerte.
2. Legibilidad y foco de campos moviles: texto de entrada a 16 px hasta 767 px; el placeholder conserva color auxiliar y opacidad completa.
3. Refresco real de actividad: formulario GET con parametro unico y recarga completa hasta el ancla de actividad.

Hay doce capturas locales en [.impeccable/review/](.impeccable/review/): acceso, inicio y empresas, cada una a 1440 y 390 px, en claro y oscuro. Por ejemplo: [inicio escritorio claro](.impeccable/review/inicio-1440-claro.png), [inicio movil oscuro](.impeccable/review/inicio-390-oscuro.png), [acceso movil claro](.impeccable/review/login-390-claro.png) y [empresas escritorio oscuro](.impeccable/review/empresas-1440-oscuro.png). Son evidencia local previa, no assets de la UI; no se publican ni se regeneran aqui. Su existencia no sustituye mediciones de contraste ni pruebas de lector de pantalla.

Persisten limites observados: tipografia externa y pesos intermedios no solicitados, texto secundario pequeno, controles de icono menores que 44 px y dependencia de JavaScript para tema, filtro y mejoras de envio. No se infiere conformidad global de accesibilidad, compatibilidad exhaustiva de navegador ni funcionalidad futura a partir del cierre acotado.

## Do's and Don'ts

### Do:

- **Do** reutilizar variables semanticas por tema, con verde principal y coral secundario.
- **Do** mantener nombre y rol empresarial legibles, con envoltura y acciones claras en movil.
- **Do** combinar nombres accesibles, texto de estado, foco visible e iconos locales decorativos.
- **Do** conservar tablas y listas operativas, vacios honestos y navegacion HTML cuando asi esta implementada.
- **Do** mantener las condiciones de teclado, hover y movimiento reducido al reutilizar transiciones.

### Don't:

- **Don't** convertir el inicio operativo en una portada de marketing ni inventar indicadores o modulos activos.
- **Don't** promover los colores residuales `neros` ni `fontFamily.display` de Tailwind a identidad vigente sin evidencia de uso.
- **Don't** usar el coral como error ni mezclar texto y fondo de temas distintos.
- **Don't** presentar rotulos pequenos, pesos tipograficos no descargados o excepciones de radio como reglas universales.
- **Don't** introducir Bootstrap, jQuery, DataTables ni Select2 en la UI nueva.
- **Don't** tratar 9/9 pruebas de cierre como certificacion integral de accesibilidad o como pruebas ejecutadas por esta pasada documental.