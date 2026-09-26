using Microsoft.Data.SqlClient;
using Neros.Database.Deploy;
using Xunit;

namespace Neros.Tests;

public sealed class DespliegueBaseDatosTests
{
    [Fact]
    public void EncabezadoCompletoSeLee()
    {
        var errores = new List<string>();
        var encabezado = EncabezadoScript.Leer(ModuloTemporal.Script("V0002__crear_tabla.sql", "SELECT 1;", "V0001__crear_esquema.sql"),
            "V0002__crear_tabla.sql", "prueba", errores);

        Assert.Empty(errores);
        Assert.NotNull(encabezado);
        Assert.False(encabezado.Destructivo);
        Assert.True(encabezado.Transaccional);
        Assert.Equal(["V0001__crear_esquema.sql"], encabezado.Dependencias);
        Assert.Equal("Script de prueba con descripcion en dos lineas.", encabezado.Descripcion);
    }

    [Fact]
    public void EncabezadoRechazaCamposFaltantesYNombreDistinto()
    {
        var sinAutor = ModuloTemporal.Script("V0001__a.sql", "SELECT 1;").Replace("Autor         : Pruebas\n", "");
        var errores = new List<string>();
        Assert.Null(EncabezadoScript.Leer(sinAutor, "V0001__a.sql", "prueba", errores));
        Assert.Contains(errores, e => e.Contains("Autor", StringComparison.Ordinal));

        errores.Clear();
        Assert.Null(EncabezadoScript.Leer(ModuloTemporal.Script("V0001__a.sql", "SELECT 1;"), "V0001__b.sql", "otro", errores));
        Assert.Contains(errores, e => e.Contains("Script", StringComparison.Ordinal));
        Assert.Contains(errores, e => e.Contains("Modulo", StringComparison.Ordinal));

        errores.Clear();
        Assert.Null(EncabezadoScript.Leer("SELECT 1;", "V0001__a.sql", "prueba", errores));
        Assert.Single(errores);
    }

    [Fact]
    public void DestructivoExigeAprobacion()
    {
        var sinAprobacion = ModuloTemporal.Script("V0001__a.sql", "SELECT 1;", destructivo: true).Replace("Aprobacion    : Pruebas\n", "");
        var errores = new List<string>();
        Assert.Null(EncabezadoScript.Leer(sinAprobacion, "V0001__a.sql", "prueba", errores));
        Assert.Contains(errores, e => e.Contains("Aprobacion", StringComparison.Ordinal));
    }

    [Fact]
    public void ChecksumIgnoraFinDeLineaYBom()
    {
        Assert.Equal(ChecksumScript.Calcular("SELECT 1;\nGO\n"), ChecksumScript.Calcular("\uFEFFSELECT 1;\r\nGO\r\n"));
        Assert.NotEqual(ChecksumScript.Calcular("SELECT 1;"), ChecksumScript.Calcular("SELECT 2;"));
    }

    [Fact]
    public void DivisorSeparaLotesSinCortarComentariosNiTextos()
    {
        var lotes = DivisorLotes.Dividir("SELECT 1;\nGO\n/*\nGO\n*/\nSELECT N'a\nGO\nb';\n  go -- fin\n\nGO\nSELECT 3;", "x.sql");

        Assert.Equal(3, lotes.Count);
        Assert.Contains("N'a\nGO\nb'", lotes[1], StringComparison.Ordinal);
        Assert.Throws<InvalidOperationException>(() => DivisorLotes.Dividir("SELECT 1;\nGO 5\n", "x.sql"));
    }

    [Theory]
    [InlineData("CREATE TABLE dbo.T (Importe float NOT NULL);", "float")]
    [InlineData("CREATE TABLE dbo.T (Importe money NOT NULL);", "money")]
    [InlineData("SELECT Id FROM dbo.T WITH (NOLOCK);", "NOLOCK")]
    [InlineData("SELECT * FROM dbo.T;", "SELECT *")]
    [InlineData("SELECT TOP (5) t.* FROM dbo.T AS t;", "SELECT *")]
    [InlineData("BEGIN TRANSACTION; SELECT 1;", "BEGIN TRANSACTION")]
    [InlineData("DROP TABLE dbo.T;", "Destructivo")]
    [InlineData("ALTER TABLE dbo.T DROP COLUMN Nombre;", "Destructivo")]
    [InlineData("TRUNCATE TABLE dbo.T;", "Destructivo")]
    public void AnalizadorRechazaPatronesProhibidos(string sql, string esperado)
    {
        var errores = new List<string>();
        AnalizadorScript.Analizar("V0001__a.sql", sql, Encabezado(ModuloTemporal.Script("V0001__a.sql", sql)), errores);
        Assert.Contains(errores, e => e.Contains(esperado, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AnalizadorIgnoraComentariosTextosEIdentificadores()
    {
        const string sql = """
            -- DROP TABLE dbo.T; SELECT * FROM x WITH (NOLOCK)
            /* float money /* anidado COMMIT */ ROLLBACK */
            SELECT N'DROP TABLE dbo.T; SELECT *', [Real], COUNT(*) FROM dbo.CostoReal;
            """;
        var errores = new List<string>();
        AnalizadorScript.Analizar("V0001__a.sql", sql, Encabezado(ModuloTemporal.Script("V0001__a.sql", sql)), errores);
        Assert.Empty(errores);
    }

    [Fact]
    public void CatalogoDeCompatibilidadEsValido()
    {
        var catalogo = CatalogoScripts.Cargar(BaseDatosPruebas.RaizScripts, "compatibilidad");

        Assert.Empty(catalogo.Errores);
        Assert.Equal("V0001__identity_empresas.sql", catalogo.Versiones[0].Nombre);
        Assert.NotEmpty(catalogo.Validaciones);
    }

    [Fact]
    public void CatalogoRechazaHuecosNombresYDependenciasInexistentes()
    {
        using var modulo = new ModuloTemporal();
        modulo.Version("V0001__uno.sql", "SELECT 1;");
        modulo.Version("V0003__tres.sql", "SELECT 3;", "V0009__nada.sql");
        modulo.Escribir("migrations", "0004_malo.sql", "SELECT 4;");

        var errores = CatalogoScripts.Cargar(modulo.Raiz, ModuloTemporal.Nombre).Errores;

        Assert.Contains(errores, e => e.Contains("V0002", StringComparison.Ordinal));
        Assert.Contains(errores, e => e.Contains("V0009__nada.sql", StringComparison.Ordinal));
        Assert.Contains(errores, e => e.Contains("0004_malo.sql", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ApplyEsIdempotenteYRegistraCadaScript()
    {
        using var modulo = ModuloBasico();
        var conexion = BaseDatosPruebas.NuevaConexion();
        try
        {
            var primera = await modulo.Desplegador(conexion).ApplyAsync();
            Assert.True(primera.Exito, string.Join('\n', primera.Problemas));
            Assert.Equal(["V0001__crear_esquema.sql", "V0002__crear_tabla.sql", "R__vista_nombres.sql", "S0001__datos_base.sql"], primera.Aplicados);

            var segunda = await modulo.Desplegador(conexion).ApplyAsync();
            Assert.True(segunda.Exito);
            Assert.Empty(segunda.Aplicados);
            Assert.True((await modulo.Desplegador(conexion).VerifyAsync()).Exito);
            Assert.True((await modulo.Desplegador(conexion).PlanAsync()).Exito);
            Assert.Equal(1, await EscalarAsync(conexion, "SELECT COUNT_BIG(1) FROM prueba.VistaNombres"));
            Assert.Equal(4, await EscalarAsync(conexion, "SELECT COUNT_BIG(1) FROM dbo.NerosSchemaVersion WHERE Exito = 1"));
        }
        finally { await BaseDatosPruebas.EliminarAsync(conexion); }
    }

    [Fact]
    public async Task ScriptAplicadoModificadoDetieneElDespliegue()
    {
        using var modulo = ModuloBasico();
        var conexion = BaseDatosPruebas.NuevaConexion();
        try
        {
            Assert.True((await modulo.Desplegador(conexion).ApplyAsync()).Exito);
            modulo.Version("V0001__crear_esquema.sql", "CREATE SCHEMA prueba;\nGO\n-- editado");

            var apply = await modulo.Desplegador(conexion).ApplyAsync();
            Assert.False(apply.Exito);
            Assert.Contains(apply.Problemas, p => p.Contains("modificado", StringComparison.Ordinal));
            Assert.False((await modulo.Desplegador(conexion).VerifyAsync()).Exito);
        }
        finally { await BaseDatosPruebas.EliminarAsync(conexion); }
    }

    [Fact]
    public async Task ScriptFallidoSeRevierteCompletoYQuedaRegistrado()
    {
        using var modulo = new ModuloTemporal();
        modulo.Version("V0001__crear_esquema.sql", "CREATE SCHEMA prueba;");
        modulo.Version("V0002__falla.sql", "CREATE TABLE prueba.Parcial (Id int NOT NULL);\nGO\nSELECT CAST(N'x' AS int);", "V0001__crear_esquema.sql");
        var conexion = BaseDatosPruebas.NuevaConexion();
        try
        {
            var resultado = await modulo.Desplegador(conexion).ApplyAsync();

            Assert.False(resultado.Exito);
            Assert.Equal(["V0001__crear_esquema.sql"], resultado.Aplicados);
            Assert.Contains(resultado.Problemas, p => p.Contains("V0002__falla.sql", StringComparison.Ordinal));
            Assert.Equal(0, await EscalarAsync(conexion, "SELECT COUNT_BIG(1) FROM sys.tables WHERE name = N'Parcial'"));
            Assert.Equal(1, await EscalarAsync(conexion, "SELECT COUNT_BIG(1) FROM dbo.NerosSchemaVersion WHERE Script = N'V0002__falla.sql' AND Exito = 0 AND Error IS NOT NULL"));
        }
        finally { await BaseDatosPruebas.EliminarAsync(conexion); }
    }

    [Fact]
    public async Task RepetibleSeReaplicaSoloCuandoCambia()
    {
        using var modulo = ModuloBasico();
        var conexion = BaseDatosPruebas.NuevaConexion();
        try
        {
            Assert.True((await modulo.Desplegador(conexion).ApplyAsync()).Exito);
            modulo.Repetible("R__vista_nombres.sql", "CREATE OR ALTER VIEW prueba.VistaNombres AS SELECT Id, Nombre, 1 AS Version FROM prueba.Cliente;", "V0002__crear_tabla.sql");

            var resultado = await modulo.Desplegador(conexion).ApplyAsync();

            Assert.True(resultado.Exito, string.Join('\n', resultado.Problemas));
            Assert.Equal(["R__vista_nombres.sql"], resultado.Aplicados);
            Assert.Equal(1, await EscalarAsync(conexion, "SELECT COUNT_BIG(1) FROM sys.columns WHERE object_id = OBJECT_ID(N'prueba.VistaNombres') AND name = N'Version'"));
        }
        finally { await BaseDatosPruebas.EliminarAsync(conexion); }
    }

    [Fact]
    public async Task ValidacionConFilasHaceFallarElDespliegue()
    {
        using var modulo = ModuloBasico();
        modulo.Escribir("validation", "002_falla.sql", "SELECT N'problema de prueba' AS Problema;");
        var conexion = BaseDatosPruebas.NuevaConexion();
        try
        {
            var resultado = await modulo.Desplegador(conexion).ApplyAsync();

            Assert.False(resultado.Exito);
            Assert.Contains("002_falla.sql: problema de prueba", resultado.Problemas);
        }
        finally { await BaseDatosPruebas.EliminarAsync(conexion); }
    }

    [Fact]
    public async Task BaselineRegistraSinEjecutarYSoloUnaVez()
    {
        using var modulo = ModuloBasico();
        var conexion = BaseDatosPruebas.NuevaConexion();
        try
        {
            var baseline = await modulo.Desplegador(conexion).BaselineAsync("V0002");
            Assert.True(baseline.Exito, string.Join('\n', baseline.Problemas));
            Assert.Equal(2, baseline.Aplicados.Count);
            Assert.Equal(0, await EscalarAsync(conexion, "SELECT COUNT_BIG(1) FROM sys.schemas WHERE name = N'prueba'"));
            Assert.Equal(2, await EscalarAsync(conexion, "SELECT COUNT_BIG(1) FROM dbo.NerosSchemaVersion WHERE EsBaseline = 1"));

            Assert.False((await modulo.Desplegador(conexion).BaselineAsync("V0002")).Exito);
        }
        finally { await BaseDatosPruebas.EliminarAsync(conexion); }
    }

    [Fact]
    public async Task DestructivoRequiereBanderaExplicita()
    {
        using var modulo = ModuloBasico();
        var conexion = BaseDatosPruebas.NuevaConexion();
        try
        {
            Assert.True((await modulo.Desplegador(conexion).ApplyAsync()).Exito);
            modulo.Version("V0003__retirar_temporal.sql", "CREATE TABLE prueba.Temporal (Id int NOT NULL);\nGO\nDROP TABLE prueba.Temporal;",
                "V0001__crear_esquema.sql", destructivo: true);

            var bloqueado = await modulo.Desplegador(conexion).ApplyAsync();
            Assert.False(bloqueado.Exito);
            Assert.Contains(bloqueado.Problemas, p => p.Contains("--permitir-destructivos", StringComparison.Ordinal));

            var permitido = await modulo.Desplegador(conexion, permitirDestructivos: true).ApplyAsync();
            Assert.True(permitido.Exito, string.Join('\n', permitido.Problemas));
            Assert.Equal(["V0003__retirar_temporal.sql"], permitido.Aplicados);
        }
        finally { await BaseDatosPruebas.EliminarAsync(conexion); }
    }

    private static ModuloTemporal ModuloBasico()
    {
        var modulo = new ModuloTemporal();
        modulo.Version("V0001__crear_esquema.sql", "CREATE SCHEMA prueba;");
        modulo.Version("V0002__crear_tabla.sql", """
            CREATE TABLE prueba.Cliente
            (
                Id int NOT NULL CONSTRAINT PK_Cliente PRIMARY KEY,
                Nombre nvarchar(80) NOT NULL,
                Saldo decimal(19,4) NOT NULL CONSTRAINT DF_Cliente_Saldo DEFAULT 0
            );
            """, "V0001__crear_esquema.sql");
        modulo.Repetible("R__vista_nombres.sql", "CREATE OR ALTER VIEW prueba.VistaNombres AS SELECT Id, Nombre FROM prueba.Cliente;", "V0002__crear_tabla.sql");
        modulo.Escribir("seed", "S0001__datos_base.sql", ModuloTemporal.Script("S0001__datos_base.sql",
            "IF NOT EXISTS (SELECT 1 FROM prueba.Cliente WHERE Id = 1) INSERT INTO prueba.Cliente (Id, Nombre) VALUES (1, N'Base');",
            "V0002__crear_tabla.sql"));
        modulo.Escribir("validation", "001_tablas.sql",
            "SELECT N'Falta prueba.Cliente' AS Problema WHERE OBJECT_ID(N'prueba.Cliente', N'U') IS NULL;");
        return modulo;
    }

    private static EncabezadoScript Encabezado(string contenido) =>
        EncabezadoScript.Leer(contenido, "V0001__a.sql", "prueba", []) ?? throw new InvalidOperationException("encabezado");

    private static async Task<long> EscalarAsync(string conexion, string sql)
    {
        await using var sqlConexion = new SqlConnection(conexion);
        await sqlConexion.OpenAsync();
        await using var comando = new SqlCommand(sql, sqlConexion);
        return Convert.ToInt64(await comando.ExecuteScalarAsync());
    }

    private sealed class ModuloTemporal : IDisposable
    {
        public const string Nombre = "prueba";

        public string Raiz { get; } = Path.Combine(Path.GetTempPath(), $"neros-db-{Guid.NewGuid():N}");

        public ModuloTemporal()
        {
            foreach (var carpeta in new[] { "migrations", "repeatable", "seed", "validation" })
                Directory.CreateDirectory(Path.Combine(Raiz, Nombre, carpeta));
        }

        public void Escribir(string carpeta, string archivo, string contenido) =>
            File.WriteAllText(Path.Combine(Raiz, Nombre, carpeta, archivo), contenido);

        public void Version(string archivo, string cuerpo, string dependencias = "Ninguna", bool destructivo = false) =>
            Escribir("migrations", archivo, Script(archivo, cuerpo, dependencias, destructivo));

        public void Repetible(string archivo, string cuerpo, string dependencias) =>
            Escribir("repeatable", archivo, Script(archivo, cuerpo, dependencias));

        public DesplegadorBaseDatos Desplegador(string conexion, bool permitirDestructivos = false) =>
            new(new OpcionesDespliegue(Raiz, Nombre, conexion) { CrearBase = true, PermitirDestructivos = permitirDestructivos, TimeoutSegundos = 60 },
                TextWriter.Null);

        public static string Script(string archivo, string cuerpo, string dependencias = "Ninguna", bool destructivo = false) =>
            string.Join('\n',
                "/*",
                "===============================================================================",
                "Neros ERP",
                $"Script        : {archivo}",
                $"Modulo        : {Nombre}",
                "Fecha         : 2026-09-25",
                "Autor         : Pruebas",
                "Descripcion   : Script de prueba",
                "                con descripcion en dos lineas.",
                $"Dependencias  : {dependencias}",
                "Objetos       : prueba",
                "Motivo        : Prueba del runner.",
                $"Destructivo   : {(destructivo ? "SI" : "NO")}",
                "Riesgo        : BAJO",
                "Rollback      : No aplica.",
                "Validacion    : validation/001_tablas.sql",
                destructivo ? "Aprobacion    : Pruebas\n*/" : "*/",
                cuerpo,
                "");

        public void Dispose()
        {
            if (Directory.Exists(Raiz)) Directory.Delete(Raiz, recursive: true);
        }
    }
}
