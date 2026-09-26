# Busqueda (Search D-17b)

Base dedicada **`NEROSSEARCH`**: indice desacoplado alimentado por eventos indexables. Sin joins al OLTP en tiempo de consulta.

## Objetos

| Tabla | Uso |
|---|---|
| `DocumentoIndice` | Titulo, texto, ACL (`PermisoRequerido`), version y tombstone |
| `EventoIndexacion` | Linaje y dedupe por `MessageId` |
| `CheckpointIngesta` | Frescura por modulo fuente |
| `EnlaceVista360` | Relaciones entre documentos ya indexados |

## Despliegue

```powershell
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo busqueda --servidor localhost --base NEROSSEARCH --crear-base
```

La consulta en aplicacion debe **filtrar por ACL y tombstone antes de paginar** (`MotorConsultaBusqueda`); permiso `BUSQUEDA.INDICE.CONSULTAR`.
