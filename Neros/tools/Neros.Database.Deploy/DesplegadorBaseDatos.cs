using System.Data;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Data.SqlClient;

namespace Neros.Database.Deploy;

public sealed class DesplegadorBaseDatos(OpcionesDespliegue opciones, TextWriter salida)
{
    private sealed record Aplicado(string Script, char Tipo, string Checksum);
    private sealed record Estado(CatalogoScripts Catalogo, Dictionary<string, Aplicado> Aplicados, List<string> Problemas)
    {
        public IEnumerable<ScriptSql> VersionesPendientes =>
            Catalogo.Versiones.Where(s => !Aplicados.ContainsKey(s.Nombre));
        public IEnumerable<ScriptSql> ReaplicablesPendientes =>
            Catalogo.Repetibles.Concat(Catalogo.Semillas)
                .Where(s => !Aplicados.TryGetValue(s.Nombre, out var aplicado) || aplicado.Checksum != s.Checksum);
    }

    public async Task<ResultadoDespliegue> PlanAsync(CancellationToken cancellationToken = default)
    {
        var catalogo = CatalogoScripts.Cargar(opciones.Raiz, opciones.Modulo);
        if (catalogo.Errores.Count > 0) return Informar(ResultadoDespliegue.Fallo(catalogo.Errores));
        if (!await BaseExisteAsync(cancellationToken))
        {
            salida.WriteLine($"La base no existe; apply --crear-base aplicaria {catalogo.Todos.Count()} scripts:");
            foreach (var script in catalogo.Todos) salida.WriteLine($"  {script.Codigo} {script.Nombre}");
            return new(true, [], []);
        }
        await using var conexion = await AbrirAsync(cancellationToken);
        var estado = await LeerEstadoAsync(conexion, catalogo, cancellationToken);
        var pendientes = estado.VersionesPendientes.Concat(estado.ReaplicablesPendientes).ToList();
        salida.WriteLine(pendientes.Count == 0 ? $"Modulo {opciones.Modulo} al dia." : $"Pendientes en {opciones.Modulo}:");
        foreach (var script in pendientes)
            salida.WriteLine($"  {script.Codigo} {script.Nombre}{(script.Encabezado.Destructivo ? "  [DESTRUCTIVO]" : "")}");
        return Informar(new(estado.Problemas.Count == 0, [], estado.Problemas));
    }

    public async Task<ResultadoDespliegue> VerifyAsync(CancellationToken cancellationToken = default)
    {
        var catalogo = CatalogoScripts.Cargar(opciones.Raiz, opciones.Modulo);
        if (catalogo.Errores.Count > 0) return Informar(ResultadoDespliegue.Fallo(catalogo.Errores));
        await using var conexion = await AbrirAsync(cancellationToken);
        var estado = await LeerEstadoAsync(conexion, catalogo, cancellationToken);
        if (estado.Problemas.Count == 0) salida.WriteLine($"Checksums de {opciones.Modulo} verificados.");
        return Informar(new(estado.Problemas.Count == 0, [], estado.Problemas));
    }

    public async Task<ResultadoDespliegue> ApplyAsync(CancellationToken cancellationToken = default)
    {
        var catalogo = CatalogoScripts.Cargar(opciones.Raiz, opciones.Modulo);
        if (catalogo.Errores.Count > 0) return Informar(ResultadoDespliegue.Fallo(catalogo.Errores));
        if (opciones.CrearBase) await CrearBaseAsync(cancellationToken);
        await using var conexion = await AbrirAsync(cancellationToken);
        await PrepararRegistroAsync(conexion, cancellationToken);
        await BloquearAsync(conexion, cancellationToken);
        var estado = await LeerEstadoAsync(conexion, catalogo, cancellationToken);
        if (estado.Problemas.Count > 0) return Informar(ResultadoDespliegue.Fallo(estado.Problemas));

        var pendientes = estado.VersionesPendientes.Concat(estado.ReaplicablesPendientes).ToList();
        var bloqueados = pendientes.Where(s => s.Encabezado.Destructivo && !opciones.PermitirDestructivos)
            .Select(s => $"{s.Nombre}: es destructivo; requiere --permitir-destructivos y backup previo.").ToList();
        if (bloqueados.Count > 0) return Informar(ResultadoDespliegue.Fallo(bloqueados));

        var aplicados = new List<string>();
        foreach (var script in pendientes)
        {
            var faltantes = script.Encabezado.Dependencias.Where(d => !estado.Aplicados.ContainsKey(d)).ToList();
            if (faltantes.Count > 0)
                return Informar(new(false, aplicados, [$"{script.Nombre}: dependencias sin aplicar: {string.Join(", ", faltantes)}."]));
            try
            {
                await EjecutarAsync(conexion, script, cancellationToken);
            }
            catch (DespliegueException error)
            {
                return Informar(new(false, aplicados, [error.Message]));
            }
            estado.Aplicados[script.Nombre] = new(script.Nombre, script.Codigo, script.Checksum);
            aplicados.Add(script.Nombre);
        }
        salida.WriteLine(aplicados.Count == 0 ? $"Modulo {opciones.Modulo} sin cambios." : $"Aplicados {aplicados.Count} scripts en {opciones.Modulo}.");

        var validacion = await ValidarAsync(conexion, catalogo, cancellationToken);
        return Informar(new(validacion.Count == 0, aplicados, validacion));
    }

    public async Task<ResultadoDespliegue> ValidateAsync(CancellationToken cancellationToken = default)
    {
        var catalogo = CatalogoScripts.Cargar(opciones.Raiz, opciones.Modulo);
        if (catalogo.Errores.Count > 0) return Informar(ResultadoDespliegue.Fallo(catalogo.Errores));
        await using var conexion = await AbrirAsync(cancellationToken);
        var problemas = await ValidarAsync(conexion, catalogo, cancellationToken);
        return Informar(new(problemas.Count == 0, [], problemas));
    }

    public async Task<ResultadoDespliegue> BaselineAsync(string hasta, CancellationToken cancellationToken = default)
    {
        var catalogo = CatalogoScripts.Cargar(opciones.Raiz, opciones.Modulo);
        if (catalogo.Errores.Count > 0) return Informar(ResultadoDespliegue.Fallo(catalogo.Errores));
        var limite = catalogo.Versiones.FirstOrDefault(s => s.Nombre.StartsWith(hasta + "__", StringComparison.Ordinal));
        if (limite is null) return Informar(ResultadoDespliegue.Fallo([$"No existe la version {hasta} en {opciones.Modulo}."]));
        if (opciones.CrearBase) await CrearBaseAsync(cancellationToken);
        await using var conexion = await AbrirAsync(cancellationToken);
        await PrepararRegistroAsync(conexion, cancellationToken);
        await BloquearAsync(conexion, cancellationToken);

        await using var transaccion = (SqlTransaction)await conexion.BeginTransactionAsync(cancellationToken);
        await using (var existentes = Comando(conexion, "SELECT COUNT_BIG(1) FROM dbo.NerosSchemaVersion WHERE Modulo = @Modulo", transaccion))
        {
            existentes.Parameters.Add("@Modulo", SqlDbType.NVarChar, 64).Value = opciones.Modulo;
            if ((long)(await existentes.ExecuteScalarAsync(cancellationToken))! > 0)
                return Informar(ResultadoDespliegue.Fallo([$"El modulo {opciones.Modulo} ya tiene registros; baseline solo aplica a bases sin historial."]));
        }
        var registrados = catalogo.Versiones.Where(s => s.Numero <= limite.Numero).ToList();
        foreach (var script in registrados)
            await RegistrarAsync(conexion, transaccion, script, 0, true, null, true, cancellationToken);
        await transaccion.CommitAsync(cancellationToken);
        salida.WriteLine($"Baseline de {opciones.Modulo} hasta {limite.Nombre}: {registrados.Count} versiones registradas sin ejecutarse. Ejecutar validate para confirmar el esquema.");
        return new(true, [.. registrados.Select(s => s.Nombre)], []);
    }

    private async Task EjecutarAsync(SqlConnection conexion, ScriptSql script, CancellationToken cancellationToken)
    {
        var lotes = DivisorLotes.Dividir(script.Contenido, script.Nombre);
        var cronometro = Stopwatch.StartNew();
        salida.WriteLine($"  {script.Codigo} {script.Nombre} ...");
        SqlTransaction? transaccion = null;
        try
        {
            if (script.Encabezado.Transaccional)
            {
                await using (var abortar = Comando(conexion, "SET XACT_ABORT ON;", null)) await abortar.ExecuteNonQueryAsync(cancellationToken);
                transaccion = (SqlTransaction)await conexion.BeginTransactionAsync(cancellationToken);
            }
            foreach (var lote in lotes)
            {
                await using var comando = Comando(conexion, lote, transaccion);
                await comando.ExecuteNonQueryAsync(cancellationToken);
            }
            await RegistrarAsync(conexion, transaccion, script, cronometro.ElapsedMilliseconds, true, null, false, cancellationToken);
            if (transaccion is not null) await transaccion.CommitAsync(cancellationToken);
        }
        catch (SqlException error)
        {
            if (transaccion is not null)
            {
                try { await transaccion.RollbackAsync(CancellationToken.None); }
                catch (InvalidOperationException) { }
            }
            await RegistrarAsync(conexion, null, script, cronometro.ElapsedMilliseconds, false, error.Message, false, CancellationToken.None);
            var detalle = script.Encabezado.Transaccional ? "se revirtio completo" : "no es transaccional; revisar su estado antes de reintentar";
            throw new DespliegueException($"{script.Nombre} fallo y {detalle}: {error.Message}", error);
        }
        finally
        {
            if (transaccion is not null) await transaccion.DisposeAsync();
        }
    }

    private async Task<List<string>> ValidarAsync(SqlConnection conexion, CatalogoScripts catalogo, CancellationToken cancellationToken)
    {
        var problemas = new List<string>();
        foreach (var validacion in catalogo.Validaciones)
        {
            await using var transaccion = (SqlTransaction)await conexion.BeginTransactionAsync(cancellationToken);
            foreach (var lote in DivisorLotes.Dividir(validacion.Contenido, validacion.Nombre))
            {
                await using var comando = Comando(conexion, lote, transaccion);
                await using var lector = await comando.ExecuteReaderAsync(cancellationToken);
                do
                {
                    while (await lector.ReadAsync(cancellationToken))
                        problemas.Add($"{validacion.Nombre}: {(lector.FieldCount > 0 && !lector.IsDBNull(0) ? Convert.ToString(lector.GetValue(0)) : "fila inesperada")}");
                }
                while (await lector.NextResultAsync(cancellationToken));
            }
            await transaccion.RollbackAsync(cancellationToken);
        }
        salida.WriteLine(problemas.Count == 0
            ? $"Validacion de {catalogo.Modulo}: {catalogo.Validaciones.Count} scripts sin problemas."
            : $"Validacion de {catalogo.Modulo}: {problemas.Count} problemas.");
        return problemas;
    }

    private async Task<Estado> LeerEstadoAsync(SqlConnection conexion, CatalogoScripts catalogo, CancellationToken cancellationToken)
    {
        var aplicados = new Dictionary<string, Aplicado>(StringComparer.Ordinal);
        await using (var comando = Comando(conexion, """
            IF OBJECT_ID(N'dbo.NerosSchemaVersion', N'U') IS NOT NULL
                SELECT v.Script, v.Tipo, v.Checksum
                FROM dbo.NerosSchemaVersion AS v
                WHERE v.Modulo = @Modulo AND v.Exito = 1
                  AND v.Id = (SELECT MAX(u.Id) FROM dbo.NerosSchemaVersion AS u
                              WHERE u.Modulo = v.Modulo AND u.Script = v.Script AND u.Exito = 1);
            """, null))
        {
            comando.Parameters.Add("@Modulo", SqlDbType.NVarChar, 64).Value = catalogo.Modulo;
            await using var lector = await comando.ExecuteReaderAsync(cancellationToken);
            while (await lector.ReadAsync(cancellationToken))
                aplicados[lector.GetString(0)] = new(lector.GetString(0), lector.GetString(1)[0], lector.GetString(2));
        }

        var problemas = new List<string>();
        var versiones = catalogo.Versiones.ToDictionary(s => s.Nombre, StringComparer.Ordinal);
        foreach (var aplicado in aplicados.Values.Where(a => a.Tipo == 'V'))
        {
            if (!versiones.TryGetValue(aplicado.Script, out var script))
                problemas.Add($"{aplicado.Script}: esta aplicado pero ya no existe en disco.");
            else if (script.Checksum != aplicado.Checksum)
                problemas.Add($"{aplicado.Script}: fue modificado despues de aplicarse; crear un script nuevo en lugar de editarlo.");
        }
        var ultima = catalogo.Versiones.Where(s => aplicados.ContainsKey(s.Nombre)).Select(s => s.Numero).DefaultIfEmpty(0).Max();
        foreach (var pendiente in catalogo.Versiones.Where(s => !aplicados.ContainsKey(s.Nombre) && s.Numero < ultima))
            problemas.Add($"{pendiente.Nombre}: esta pendiente pero es anterior a la ultima version aplicada.");
        return new(catalogo, aplicados, problemas);
    }

    private async Task RegistrarAsync(SqlConnection conexion, SqlTransaction? transaccion, ScriptSql script, long duracionMs,
        bool exito, string? error, bool esBaseline, CancellationToken cancellationToken)
    {
        await using var comando = Comando(conexion, """
            INSERT INTO dbo.NerosSchemaVersion (Modulo, Script, Tipo, Checksum, Descripcion, DuracionMs, Exito, EsBaseline, Error)
            VALUES (@Modulo, @Script, @Tipo, @Checksum, @Descripcion, @DuracionMs, @Exito, @EsBaseline, @Error);
            """, transaccion);
        comando.Parameters.Add("@Modulo", SqlDbType.NVarChar, 64).Value = opciones.Modulo;
        comando.Parameters.Add("@Script", SqlDbType.NVarChar, 200).Value = script.Nombre;
        comando.Parameters.Add("@Tipo", SqlDbType.Char, 1).Value = script.Codigo.ToString();
        comando.Parameters.Add("@Checksum", SqlDbType.Char, 64).Value = script.Checksum;
        comando.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 400).Value = Recortar(script.Encabezado.Descripcion, 400);
        comando.Parameters.Add("@DuracionMs", SqlDbType.Int).Value = (int)Math.Min(duracionMs, int.MaxValue);
        comando.Parameters.Add("@Exito", SqlDbType.Bit).Value = exito;
        comando.Parameters.Add("@EsBaseline", SqlDbType.Bit).Value = esBaseline;
        comando.Parameters.Add("@Error", SqlDbType.NVarChar, 2000).Value = error is null ? DBNull.Value : Recortar(error, 2000);
        await comando.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task PrepararRegistroAsync(SqlConnection conexion, CancellationToken cancellationToken)
    {
        await using var recurso = Assembly.GetExecutingAssembly().GetManifestResourceStream("Neros.Database.Deploy.NerosSchemaVersion.sql")
            ?? throw new InvalidOperationException("Recurso NerosSchemaVersion.sql ausente.");
        using var lector = new StreamReader(recurso);
        await using var comando = Comando(conexion, await lector.ReadToEndAsync(cancellationToken), null);
        await comando.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task BloquearAsync(SqlConnection conexion, CancellationToken cancellationToken)
    {
        await using var comando = Comando(conexion, """
            DECLARE @Resultado int;
            EXEC @Resultado = sys.sp_getapplock @Resource = @Recurso, @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = 60000;
            SELECT @Resultado;
            """, null);
        comando.Parameters.Add("@Recurso", SqlDbType.NVarChar, 255).Value = $"NerosSchemaVersion:{opciones.Modulo}";
        if ((int)(await comando.ExecuteScalarAsync(cancellationToken))! < 0)
            throw new DespliegueException($"Otro despliegue de {opciones.Modulo} esta en curso.");
    }

    private async Task<bool> BaseExisteAsync(CancellationToken cancellationToken)
    {
        var (maestra, nombre) = Maestra();
        await using var conexion = new SqlConnection(maestra);
        await conexion.OpenAsync(cancellationToken);
        await using var comando = Comando(conexion, "SELECT CASE WHEN DB_ID(@Base) IS NULL THEN 0 ELSE 1 END", null);
        comando.Parameters.Add("@Base", SqlDbType.NVarChar, 128).Value = nombre;
        return (int)(await comando.ExecuteScalarAsync(cancellationToken))! == 1;
    }

    private async Task CrearBaseAsync(CancellationToken cancellationToken)
    {
        var (maestra, nombre) = Maestra();
        await using var conexion = new SqlConnection(maestra);
        await conexion.OpenAsync(cancellationToken);
        await using var comando = Comando(conexion, """
            IF DB_ID(@Base) IS NULL
            BEGIN
                DECLARE @Sql nvarchar(400) = N'CREATE DATABASE ' + QUOTENAME(@Base);
                EXEC sys.sp_executesql @Sql;
                SELECT 1;
            END
            ELSE SELECT 0;
            """, null);
        comando.Parameters.Add("@Base", SqlDbType.NVarChar, 128).Value = nombre;
        if ((int)(await comando.ExecuteScalarAsync(cancellationToken))! == 1) salida.WriteLine($"Base {nombre} creada.");
    }

    private (string Cadena, string Base) Maestra()
    {
        var constructor = new SqlConnectionStringBuilder(opciones.CadenaConexion);
        var nombre = constructor.InitialCatalog;
        if (string.IsNullOrWhiteSpace(nombre)) throw new DespliegueException("La cadena de conexion no indica la base de datos.");
        constructor.InitialCatalog = "master";
        return (constructor.ConnectionString, nombre);
    }

    private async Task<SqlConnection> AbrirAsync(CancellationToken cancellationToken)
    {
        var conexion = new SqlConnection(opciones.CadenaConexion);
        await conexion.OpenAsync(cancellationToken);
        return conexion;
    }

    private SqlCommand Comando(SqlConnection conexion, string sql, SqlTransaction? transaccion) =>
        new(sql, conexion, transaccion) { CommandTimeout = opciones.TimeoutSegundos };

    private ResultadoDespliegue Informar(ResultadoDespliegue resultado)
    {
        foreach (var problema in resultado.Problemas) salida.WriteLine($"ERROR {problema}");
        return resultado;
    }

    private static string Recortar(string texto, int maximo) => texto.Length <= maximo ? texto : texto[..maximo];
}
