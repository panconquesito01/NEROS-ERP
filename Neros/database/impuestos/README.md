# Modulo `impuestos`

Configuracion tributaria (impuesto, tarifa con vigencia, conceptos) y **versiones publicadas inmutables** (`VersionPublicada` + `VersionRegla`). El calculo en runtime usa el motor puro en `Neros.Domain/Impuestos` (sin E/S).

## Despliegue

```powershell
dotnet run --project ../../tools/Neros.Database.Deploy -- apply --modulo impuestos --servidor localhost --base NEROSIMPUESTOS --crear-base
```
