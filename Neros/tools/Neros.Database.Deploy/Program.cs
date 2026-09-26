using Microsoft.Data.SqlClient;

namespace Neros.Database.Deploy;

public static class Program
{
    private const string Uso = """
        Uso: plan|apply|validate|verify|baseline --modulo <modulo> (--servidor <servidor> --base <base> | --conexion-variable <VARIABLE>)
             [--raiz <carpeta database>] [--crear-base] [--permitir-destructivos] [--hasta V0001] [--timeout <segundos>]
        """;
    private static readonly HashSet<string> Banderas = ["--crear-base", "--permitir-destructivos"];
    private static readonly HashSet<string> Valores = ["--modulo", "--servidor", "--base", "--conexion-variable", "--raiz", "--hasta", "--timeout"];

    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || args[0] is not ("plan" or "apply" or "validate" or "verify" or "baseline")
            || !TryLeer(args[1..], out var argumentos) || !TryOpciones(argumentos, out var opciones, out var problema))
        {
            Console.Error.WriteLine(Uso);
            return 2;
        }
        if (problema is not null)
        {
            Console.Error.WriteLine(problema);
            return 2;
        }
        if (args[0] == "baseline" && !argumentos.ContainsKey("--hasta"))
        {
            Console.Error.WriteLine("baseline exige --hasta V<NNNN>.");
            return 2;
        }

        using var cancelacion = new CancellationTokenSource();
        Console.CancelKeyPress += (_, evento) => { evento.Cancel = true; cancelacion.Cancel(); };
        var desplegador = new DesplegadorBaseDatos(opciones!, Console.Out);
        try
        {
            var resultado = args[0] switch
            {
                "plan" => await desplegador.PlanAsync(cancelacion.Token),
                "apply" => await desplegador.ApplyAsync(cancelacion.Token),
                "validate" => await desplegador.ValidateAsync(cancelacion.Token),
                "verify" => await desplegador.VerifyAsync(cancelacion.Token),
                _ => await desplegador.BaselineAsync(argumentos["--hasta"]!, cancelacion.Token)
            };
            return resultado.Exito ? 0 : 1;
        }
        catch (Exception error) when (error is SqlException or DespliegueException or InvalidOperationException or IOException or OperationCanceledException)
        {
            Console.Error.WriteLine($"ERROR {error.Message}");
            return 1;
        }
    }

    private static bool TryLeer(string[] args, out Dictionary<string, string?> argumentos)
    {
        argumentos = new(StringComparer.Ordinal);
        for (var i = 0; i < args.Length; i++)
        {
            if (Banderas.Contains(args[i])) argumentos[args[i]] = null;
            else if (Valores.Contains(args[i]) && i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal)) argumentos[args[i]] = args[++i];
            else return false;
        }
        return true;
    }

    private static bool TryOpciones(Dictionary<string, string?> argumentos, out OpcionesDespliegue? opciones, out string? problema)
    {
        opciones = null;
        problema = null;
        if (argumentos.GetValueOrDefault("--modulo") is not { } modulo) return false;

        string cadena;
        if (argumentos.GetValueOrDefault("--conexion-variable") is { } variable)
        {
            if (Environment.GetEnvironmentVariable(variable) is not { Length: > 0 } valor)
            {
                problema = $"La variable de entorno {variable} no existe o esta vacia.";
                return true;
            }
            cadena = valor;
        }
        else if (argumentos.GetValueOrDefault("--servidor") is { } servidor && argumentos.GetValueOrDefault("--base") is { } baseDatos)
        {
            cadena = new SqlConnectionStringBuilder
            {
                DataSource = servidor, InitialCatalog = baseDatos, IntegratedSecurity = true,
                Encrypt = true, TrustServerCertificate = true, ApplicationName = "Neros.Database.Deploy"
            }.ConnectionString;
        }
        else return false;

        var raiz = argumentos.GetValueOrDefault("--raiz") ?? BuscarRaiz(modulo);
        if (raiz is null)
        {
            problema = $"No se encontro la carpeta database/{modulo}; indicar --raiz.";
            return true;
        }
        var timeout = 300;
        if (argumentos.GetValueOrDefault("--timeout") is { } texto && (!int.TryParse(texto, out timeout) || timeout <= 0))
        {
            problema = "--timeout debe ser un entero positivo.";
            return true;
        }
        opciones = new(Path.GetFullPath(raiz), modulo, cadena)
        {
            CrearBase = argumentos.ContainsKey("--crear-base"),
            PermitirDestructivos = argumentos.ContainsKey("--permitir-destructivos"),
            TimeoutSegundos = timeout
        };
        return true;
    }

    private static string? BuscarRaiz(string modulo)
    {
        foreach (var inicio in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
            for (var carpeta = new DirectoryInfo(inicio); carpeta is not null; carpeta = carpeta.Parent)
            {
                var candidata = Path.Combine(carpeta.FullName, "database");
                if (Directory.Exists(Path.Combine(candidata, modulo))) return candidata;
            }
        return null;
    }
}
