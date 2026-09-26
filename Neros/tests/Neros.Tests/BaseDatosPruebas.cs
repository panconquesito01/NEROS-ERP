using Microsoft.Data.SqlClient;
using Neros.Database.Deploy;
using Xunit;

namespace Neros.Tests;

public static class BaseDatosPruebas
{
    public const string Prefijo = "NerosTests_";

    public static string RaizScripts { get; } = BuscarRaiz();

    public static string NuevaConexion() => new SqlConnectionStringBuilder
    {
        DataSource = Environment.GetEnvironmentVariable("NEROS_TEST_SERVER") ?? "localhost",
        InitialCatalog = $"{Prefijo}{Guid.NewGuid():N}",
        IntegratedSecurity = true, Encrypt = true, TrustServerCertificate = true
    }.ConnectionString;

    public static async Task DesplegarAsync(string conexion, string modulo = "compatibilidad", string? raiz = null, bool crearBase = true)
    {
        var salida = new StringWriter();
        var resultado = await new DesplegadorBaseDatos(
            new OpcionesDespliegue(raiz ?? RaizScripts, modulo, conexion) { CrearBase = crearBase }, salida).ApplyAsync();
        Assert.True(resultado.Exito, salida.ToString());
    }

    public static async Task DesplegarCompatibilidadYPrivacidadAsync(string conexion, string? raiz = null)
    {
        await DesplegarAsync(conexion, "compatibilidad", raiz);
        await DesplegarAsync(conexion, "privacidad", raiz, crearBase: false);
    }

    public static async Task DesplegarCompatibilidadPrivacidadYGlobalizacionAsync(string conexion, string? raiz = null)
    {
        await DesplegarCompatibilidadYPrivacidadAsync(conexion, raiz);
        await DesplegarAsync(conexion, "globalizacion", raiz, crearBase: false);
    }

    public static async Task EliminarAsync(string conexion)
    {
        var constructor = new SqlConnectionStringBuilder(conexion);
        var nombre = constructor.InitialCatalog;
        Assert.StartsWith(Prefijo, nombre);
        SqlConnection.ClearAllPools();
        constructor.InitialCatalog = "master";
        await using var maestra = new SqlConnection(constructor.ConnectionString);
        await maestra.OpenAsync();
        await using var comando = maestra.CreateCommand();
        comando.CommandText = """
            IF DB_ID(@Base) IS NOT NULL
            BEGIN
                DECLARE @Sql nvarchar(600) = N'ALTER DATABASE ' + QUOTENAME(@Base) + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE ' + QUOTENAME(@Base) + N';';
                EXEC sys.sp_executesql @Sql;
            END
            """;
        comando.Parameters.AddWithValue("@Base", nombre);
        await comando.ExecuteNonQueryAsync();
    }

    private static string BuscarRaiz()
    {
        for (var carpeta = new DirectoryInfo(AppContext.BaseDirectory); carpeta is not null; carpeta = carpeta.Parent)
        {
            var candidata = Path.Combine(carpeta.FullName, "database");
            if (Directory.Exists(Path.Combine(candidata, "conventions"))) return candidata;
        }
        throw new DirectoryNotFoundException("No se encontro la carpeta database del repositorio.");
    }
}
