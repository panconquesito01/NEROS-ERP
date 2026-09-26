# Modulo `globalizacion`

Catalogos ISO, politicas de redondeo, tasas de cambio, paquete de localizacion generico y registro normativo (`cumplimiento.ReglaNormativa`).

## Despliegue local

```powershell
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo globalizacion --servidor localhost --base NEROSGLOBAL --crear-base
dotnet run --project .\tools\Neros.Database.Deploy -- validate --modulo globalizacion --servidor localhost --base NEROSGLOBAL
```

Puede coexistir en la misma instancia que `NEROSERP` (modulo independiente) o en base dedicada `NEROSGLOBAL`.
