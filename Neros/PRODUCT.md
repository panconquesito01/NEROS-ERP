# Neros ERP

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

Personal operativo y gerencia que trabajan con varias empresas. El inicio prioriza elegir el contexto empresarial y acceder a las funciones disponibles.

## Product Purpose

ERP multiempresa en espanol. Esta entrega establece acceso autenticado, empresas autorizadas e inicio con datos reales. La consolidacion gerencial y los modulos transaccionales quedan para entregas posteriores.

## Operating Context

Blazor Web App y API ASP.NET Core sobre .NET 10, con capas separadas. SQL Server local, base NEROSERP y autenticacion integrada de Windows para desarrollo. El administrador inicial se crea mediante un comando local sin contrasenas predeterminadas.

## Capabilities and Constraints

- Identity como base de autenticacion y autorizacion de empresa en servidor.
- Blazor consume la API; no accede a persistencia.
- Tailwind CSS; sin Bootstrap, jQuery, DataTables ni Select2.
- Scripts SQL Server manuales en carpeta independiente.
- Documentacion tecnica y gerencial separadas.
- Ventas, inventario y finanzas no se presentan como modulos implementados.
- Consolidacion entre empresas: pendiente, no debe sumarse informacion sin permisos explicitos.

## Brand Commitments

Nombre Neros ERP e interfaz en espanol.
El usuario requiere una experiencia atractiva, sencilla y no abrumadora, con animaciones cuidadas y colores propios. El tema sigue al navegador/sistema y permite elegir claro u oscuro manualmente.

## Evidence on Hand

El login previo era una navegacion sin validacion. Los indicadores del inicio eran estaticos; no constituyen datos de negocio.

## Product Principles

- Contexto de empresa siempre visible.
- Permisos comprobados en servidor en cada acceso.
- Estados vacios y errores honestos, sin indicadores ficticios.
- Trabajo repetitivo rapido y accesible.