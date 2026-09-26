# Analitica (warehouse D-17a)

Base dedicada **`NEROSANALYTICS`**: proyecciones de lectura alimentadas por eventos de integracion. No escribe en bases OLTP.

## Objetos

| Tabla | Uso |
|---|---|
| `MarcaAguaIngesta` | Watermark por tenant, empresa y modulo fuente |
| `EventoIngesta` | Linaje append-only (dedupe por `MessageId`) |
| `HechoOperativo` | Hechos materializados para KPI |
| `DefinicionIndicador` / `ValorIndicador` | Indicadores de tablero por periodo de negocio |

## Despliegue

```powershell
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo analitica --servidor localhost --base NEROSANALYTICS --crear-base
```

El consumidor `analitica.ingesta` del worker de integracion requiere `ConnectionStrings:Analitica` apuntando a esta base.
