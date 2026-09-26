IF OBJECT_ID(N'dbo.NerosSchemaVersion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NerosSchemaVersion
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_NerosSchemaVersion PRIMARY KEY,
        Modulo nvarchar(64) NOT NULL,
        Script nvarchar(200) NOT NULL,
        Tipo char(1) NOT NULL CONSTRAINT CK_NerosSchemaVersion_Tipo CHECK (Tipo IN ('V', 'R', 'S')),
        Checksum char(64) NOT NULL,
        Descripcion nvarchar(400) NOT NULL,
        AplicadoEnUtc datetime2(3) NOT NULL CONSTRAINT DF_NerosSchemaVersion_AplicadoEnUtc DEFAULT SYSUTCDATETIME(),
        AplicadoPor nvarchar(128) NOT NULL CONSTRAINT DF_NerosSchemaVersion_AplicadoPor DEFAULT SUSER_SNAME(),
        DuracionMs int NOT NULL,
        Exito bit NOT NULL,
        EsBaseline bit NOT NULL CONSTRAINT DF_NerosSchemaVersion_EsBaseline DEFAULT 0,
        Error nvarchar(2000) NULL
    );
    CREATE INDEX IX_NerosSchemaVersion_Modulo_Script
        ON dbo.NerosSchemaVersion (Modulo, Script) INCLUDE (Exito, Checksum);
END;
