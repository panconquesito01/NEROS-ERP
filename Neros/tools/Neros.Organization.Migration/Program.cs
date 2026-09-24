using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Neros.Organization.Migration;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length != 3 || args[0] is not ("snapshot" or "validate" or "inspect"))
        {
            Console.Error.WriteLine("Uso: snapshot|validate|inspect <user-secrets-id> <archivo-local.json>. Solo lectura SQL.");
            return 2;
        }
        using var limite = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try
        {
            var configuration = new ConfigurationBuilder().AddUserSecrets(args[1]).AddEnvironmentVariables().Build();
            var connectionString = configuration.GetConnectionString("Neros")
                ?? throw new InvalidOperationException("configuracion_ausente");
            if (args[0] == "inspect")
            {
                await InspeccionarAsync(connectionString, args[2], limite.Token);
                return 0;
            }
            var empresas = await LeerEmpresasAsync(connectionString, limite.Token);
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                WriteIndented = true,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
                AllowDuplicateProperties = false
            };
            if (args[0] == "snapshot")
            {
                var path = Path.GetFullPath(args[2]);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                await JsonSerializer.SerializeAsync(output, ValidadorMigracion.Preparar(empresas), options, limite.Token);
                Console.WriteLine($"Snapshot creado: {empresas.Count} empresas pendientes; sin asignacion automatica ni cambios SQL.");
                return 0;
            }

            await using var input = File.OpenRead(args[2]);
            if (input.Length > 16 * 1024 * 1024) throw new InvalidOperationException("plan_demasiado_grande");
            var plan = await JsonSerializer.DeserializeAsync<PlanMigracion>(input, options, limite.Token)
                ?? throw new InvalidOperationException("plan_ausente");
            var errores = ValidadorMigracion.Validar(plan, empresas);
            foreach (var error in errores) Console.Error.WriteLine(error);
            Console.WriteLine(errores.Count == 0 ? "Correspondencias validas; no se ha migrado ningun dato." : "Correspondencias rechazadas; no se ha migrado ningun dato.");
            return errores.Count == 0 ? 0 : 1;
        }
        catch (Exception exception) when (exception is SqlException or IOException or UnauthorizedAccessException
            or JsonException or InvalidOperationException or ArgumentException or OperationCanceledException)
        {
            Console.Error.WriteLine("No se pudo procesar el plan. Revisar configuracion, acceso, archivo y formato. Sin cambios SQL.");
            return 2;
        }
    }

    private sealed record ColumnaOrigen(string Schema, string Table, string Column);
    private sealed record EmpresaOrigen(Guid CompanyId, string Code, string Name);
    private sealed record InformeOrigen(List<ColumnaOrigen> Columns, List<EmpresaOrigen> Companies);

    private static async Task InspeccionarAsync(string connectionString, string outputPath, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'Empresas' OR TABLE_NAME LIKE '%Tenant%' OR TABLE_NAME LIKE '%Grupo%'
               OR TABLE_NAME LIKE '%Group%' OR COLUMN_NAME LIKE '%Tenant%' OR COLUMN_NAME LIKE '%Grupo%'
               OR COLUMN_NAME LIKE '%Group%'
            ORDER BY TABLE_SCHEMA, TABLE_NAME, ORDINAL_POSITION;
            SELECT Id, Codigo, Nombre FROM dbo.Empresas ORDER BY Id;
            """;
        command.CommandTimeout = 15;
        var columns = new List<ColumnaOrigen>();
        var companies = new List<EmpresaOrigen>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            columns.Add(new(reader.GetString(0), reader.GetString(1), reader.GetString(2)));
        await reader.NextResultAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            companies.Add(new(reader.GetGuid(0), reader.GetString(1), reader.GetString(2)));
        var path = Path.GetFullPath(outputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(output, new InformeOrigen(columns, companies),
            new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true }, cancellationToken);
        Console.WriteLine($"Inspeccion local generada: {companies.Count} empresas y {columns.Count} columnas candidatas. Sin credenciales ni datos personales de usuarios.");
    }

    private static async Task<List<Guid>> LeerEmpresasAsync(string connectionString, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id FROM dbo.Empresas ORDER BY Id";
        command.CommandTimeout = 15;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var empresas = new List<Guid>();
        while (await reader.ReadAsync(cancellationToken)) empresas.Add(reader.GetGuid(0));
        return empresas;
    }
}