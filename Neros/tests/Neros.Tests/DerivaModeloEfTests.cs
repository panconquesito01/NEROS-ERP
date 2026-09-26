using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Neros.Persistence;
using Xunit;

namespace Neros.Tests;

public sealed class DerivaModeloEfTests
{
    private sealed record ColumnaReal(string Tipo, bool Nula);

    [Fact]
    public async Task ModeloEfCoincideConEsquemaDesplegadoPorScripts()
    {
        var conexion = BaseDatosPruebas.NuevaConexion();
        try
        {
            await BaseDatosPruebas.DesplegarCompatibilidadYPrivacidadAsync(conexion);
            await using var contexto = new NerosDbContext(new DbContextOptionsBuilder<NerosDbContext>().UseSqlServer(conexion).Options);
            var modelo = contexto.GetService<IDesignTimeModel>().Model;
            var (columnas, indices) = await LeerEsquemaAsync(conexion);
            var diferencias = new List<string>();
            var tablasMapeadas = new HashSet<(string, string)>();
            var columnasMapeadas = new HashSet<(string, string, string)>();

            foreach (var entidad in modelo.GetEntityTypes())
            {
                if (entidad.GetTableName() is not { } tabla) continue;
                var esquema = entidad.GetSchema() ?? "dbo";
                var objeto = StoreObjectIdentifier.Table(tabla, entidad.GetSchema());
                tablasMapeadas.Add((esquema, tabla));
                foreach (var propiedad in entidad.GetProperties())
                {
                    if (propiedad.GetColumnName(objeto) is not { } nombre) continue;
                    columnasMapeadas.Add((esquema, tabla, nombre));
                    var tipo = Normalizar(propiedad.GetColumnType(objeto));
                    if (!columnas.TryGetValue((esquema, tabla, nombre), out var real))
                        diferencias.Add($"Falta en SQL: {esquema}.{tabla}.{nombre} ({tipo})");
                    else
                    {
                        if (real.Tipo != tipo) diferencias.Add($"Tipo distinto en {esquema}.{tabla}.{nombre}: EF {tipo}, SQL {real.Tipo}");
                        if (real.Nula != propiedad.IsColumnNullable(objeto)) diferencias.Add($"Nulabilidad distinta en {esquema}.{tabla}.{nombre}: SQL {(real.Nula ? "NULL" : "NOT NULL")}");
                    }
                }
                foreach (var indice in entidad.GetIndexes())
                    if (indice.GetDatabaseName(objeto) is { } nombreIndice && !indices.Contains((esquema, tabla, nombreIndice)))
                        diferencias.Add($"Falta indice en SQL: {esquema}.{tabla}.{nombreIndice}");
            }

            foreach (var (esquema, tabla, nombre) in columnas.Keys.Where(c => tablasMapeadas.Contains((c.Esquema, c.Tabla)) && !columnasMapeadas.Contains(c)))
                diferencias.Add($"Columna SQL sin mapeo EF: {esquema}.{tabla}.{nombre}");
            foreach (var (esquema, tabla) in columnas.Keys.Select(c => (c.Esquema, c.Tabla)).Distinct()
                .Where(t => !tablasMapeadas.Contains(t) && t != ("dbo", "NerosSchemaVersion")))
                diferencias.Add($"Tabla SQL sin mapeo EF: {esquema}.{tabla}");

            Assert.True(diferencias.Count == 0, string.Join(Environment.NewLine, diferencias));
        }
        finally { await BaseDatosPruebas.EliminarAsync(conexion); }
    }

    private static async Task<(Dictionary<(string Esquema, string Tabla, string Columna), ColumnaReal>, HashSet<(string, string, string)>)> LeerEsquemaAsync(string conexion)
    {
        await using var sqlConexion = new SqlConnection(conexion);
        await sqlConexion.OpenAsync();
        await using var comando = new SqlCommand("""
            SELECT c.TABLE_SCHEMA, c.TABLE_NAME, c.COLUMN_NAME, c.DATA_TYPE, c.CHARACTER_MAXIMUM_LENGTH,
                   c.NUMERIC_PRECISION, c.NUMERIC_SCALE, c.DATETIME_PRECISION, c.IS_NULLABLE
            FROM INFORMATION_SCHEMA.COLUMNS AS c
            JOIN INFORMATION_SCHEMA.TABLES AS t ON t.TABLE_SCHEMA = c.TABLE_SCHEMA AND t.TABLE_NAME = c.TABLE_NAME
            WHERE t.TABLE_TYPE = 'BASE TABLE'
              AND COLUMNPROPERTY(OBJECT_ID(QUOTENAME(c.TABLE_SCHEMA) + '.' + QUOTENAME(c.TABLE_NAME)), c.COLUMN_NAME, 'GeneratedAlwaysType') = 0;
            SELECT s.name, t.name, i.name
            FROM sys.indexes AS i
            JOIN sys.tables AS t ON t.object_id = i.object_id
            JOIN sys.schemas AS s ON s.schema_id = t.schema_id
            WHERE i.name IS NOT NULL AND i.is_primary_key = 0;
            """, sqlConexion);
        await using var lector = await comando.ExecuteReaderAsync();
        var columnas = new Dictionary<(string, string, string), ColumnaReal>();
        while (await lector.ReadAsync())
        {
            var tipo = lector.GetString(3).ToLowerInvariant();
            int? Entero(int indice) => lector.IsDBNull(indice) ? null : Convert.ToInt32(lector.GetValue(indice));
            tipo = tipo switch
            {
                "char" or "varchar" or "nchar" or "nvarchar" or "binary" or "varbinary" =>
                    $"{tipo}({(Entero(4) == -1 ? "max" : Entero(4)!.Value.ToString())})",
                "decimal" or "numeric" => $"{tipo}({Entero(5)},{Entero(6)})",
                "datetime2" or "datetimeoffset" or "time" when Entero(7) is { } precision && precision != 7 => $"{tipo}({precision})",
                _ => tipo
            };
            columnas[(lector.GetString(0), lector.GetString(1), lector.GetString(2))] = new(tipo, lector.GetString(8) == "YES");
        }
        await lector.NextResultAsync();
        var indices = new HashSet<(string, string, string)>();
        while (await lector.ReadAsync()) indices.Add((lector.GetString(0), lector.GetString(1), lector.GetString(2)));
        return (columnas, indices);
    }

    private static string Normalizar(string? tipo) => (tipo ?? "").Replace(" ", "", StringComparison.Ordinal).ToLowerInvariant();
}
