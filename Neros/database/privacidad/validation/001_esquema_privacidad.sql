SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRAN;

SELECT Problema FROM (
    SELECT N'Falta esquema privacidad' AS Problema WHERE SCHEMA_ID(N'privacidad') IS NULL
    UNION ALL SELECT N'Falta privacidad.CatalogoDatoPersonal' WHERE OBJECT_ID(N'privacidad.CatalogoDatoPersonal', N'U') IS NULL
    UNION ALL SELECT N'Falta privacidad.DocumentoLegal' WHERE OBJECT_ID(N'privacidad.DocumentoLegal', N'U') IS NULL
    UNION ALL SELECT N'Falta privacidad.DocumentoLegalVersion' WHERE OBJECT_ID(N'privacidad.DocumentoLegalVersion', N'U') IS NULL
    UNION ALL SELECT N'Falta privacidad.AceptacionLegal' WHERE OBJECT_ID(N'privacidad.AceptacionLegal', N'U') IS NULL
    UNION ALL SELECT N'Falta privacidad.DefinicionCookie' WHERE OBJECT_ID(N'privacidad.DefinicionCookie', N'U') IS NULL
    UNION ALL SELECT N'Falta privacidad.RetencionPolicy' WHERE OBJECT_ID(N'privacidad.RetencionPolicy', N'U') IS NULL
    UNION ALL SELECT N'Falta privacidad.LegalHold' WHERE OBJECT_ID(N'privacidad.LegalHold', N'U') IS NULL
    UNION ALL SELECT N'Sin politicas de retencion activas' WHERE NOT EXISTS (SELECT 1 FROM privacidad.RetencionPolicy WHERE Activo = 1)
    UNION ALL SELECT N'Sin definiciones de cookies activas' WHERE NOT EXISTS (SELECT 1 FROM privacidad.DefinicionCookie WHERE Activo = 1)
    UNION ALL SELECT N'Sin documentos legales vigentes' WHERE NOT EXISTS (
        SELECT 1 FROM privacidad.DocumentoLegalVersion AS v
        INNER JOIN privacidad.DocumentoLegal AS d ON d.Id = v.DocumentoId
        WHERE d.Activo = 1 AND d.RequiereAceptacion = 1
          AND v.VigenteDesde <= SYSUTCDATETIME()
          AND (v.VigenteHasta IS NULL OR v.VigenteHasta > SYSUTCDATETIME()))
) AS problemas;

ROLLBACK TRAN;
