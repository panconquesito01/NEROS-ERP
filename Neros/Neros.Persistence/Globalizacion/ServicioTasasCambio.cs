using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Neros.Application.Globalizacion;
using Neros.Contracts.Globalizacion;

namespace Neros.Persistence.Globalizacion;

public sealed class ServicioTasasCambio(
    IConfiguration configuration,
    IEnumerable<IProveedorTasasMercado> proveedores,
    TimeProvider reloj) : IServicioTasasCambio
{
    private static readonly string[] OrigenesPredeterminados = ["USD", "EUR", "MXN", "BRL", "PEN", "CLP"];

    public async Task<PaginaTasasCambio> ListarAsync(string monedaDestino, DateOnly? fecha, CancellationToken cancellationToken)
    {
        var destino = monedaDestino.Trim().ToUpperInvariant();
        var dia = fecha ?? DateOnly.FromDateTime(reloj.GetUtcNow().UtcDateTime);
        await using var sql = await AbrirAsync(cancellationToken);
        await using var comando = sql.CreateCommand();
        comando.CommandText = """
            SELECT [MonedaOrigen], [MonedaDestino], [Fecha], [Valor], [Fuente]
            FROM [globalizacion].[TasaCambio]
            WHERE [MonedaDestino] = @Destino AND [Fecha] = @Fecha
            ORDER BY [MonedaOrigen]
            """;
        comando.Parameters.AddWithValue("@Destino", destino);
        comando.Parameters.AddWithValue("@Fecha", dia.ToDateTime(TimeOnly.MinValue));
        var tasas = new List<TasaCambioDia>();
        await using var reader = await comando.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            tasas.Add(new TasaCambioDia(
                reader.GetString(0).Trim(), reader.GetString(1).Trim(),
                DateOnly.FromDateTime(reader.GetDateTime(2)), reader.GetDecimal(3), reader.GetString(4)));
        }
        return new PaginaTasasCambio(tasas, destino, dia);
    }

    public async Task<TasaCambioDia?> ObtenerAsync(string origen, string destino, DateOnly fecha, CancellationToken cancellationToken)
    {
        var pagina = await ListarAsync(destino, fecha, cancellationToken);
        return pagina.Tasas.FirstOrDefault(t => t.MonedaOrigen.Equals(origen, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<(ResultadoSincronizacionTasas? Resultado, string? Error)> SincronizarMercadoAsync(
        SolicitudSincronizarTasas solicitud, CancellationToken cancellationToken)
    {
        var destino = solicitud.MonedaDestino.Trim().ToUpperInvariant();
        if (destino.Length != 3) return (null, "moneda_invalida");
        var dia = solicitud.Fecha ?? DateOnly.FromDateTime(reloj.GetUtcNow().UtcDateTime);
        var pais = solicitud.Pais?.Trim().ToUpperInvariant();

        var tasas = new Dictionary<string, (decimal Valor, string Fuente)>(StringComparer.Ordinal);
        foreach (var proveedor in proveedores)
        {
            var obtenidas = await proveedor.ObtenerTasasHaciaAsync(destino, dia, OrigenesPredeterminados, pais, cancellationToken);
            foreach (var par in obtenidas)
                tasas[par.Key] = (par.Value, proveedor.NombreFuente);
        }

        if (tasas.Count == 0)
        {
            var frankfurter = proveedores.FirstOrDefault(p => p.NombreFuente.Contains("Frankfurter", StringComparison.Ordinal));
            if (frankfurter is not null)
            {
                foreach (var par in await frankfurter.ObtenerTasasHaciaAsync(destino, dia, OrigenesPredeterminados, pais, cancellationToken))
                    tasas.TryAdd(par.Key, (par.Value, frankfurter.NombreFuente));
            }
        }

        if (tasas.Count == 0) return (null, "proveedor_sin_datos");

        await using var sql = await AbrirAsync(cancellationToken);
        var actualizadas = 0;
        foreach (var (origen, (valor, fuente)) in tasas)
        {
            if (origen.Equals(destino, StringComparison.OrdinalIgnoreCase)) continue;
            await using var comando = sql.CreateCommand();
            comando.CommandText = """
                MERGE [globalizacion].[TasaCambio] AS destino
                USING (SELECT @Fuente AS Fuente, @Fecha AS Fecha, @Origen AS MonedaOrigen, @Destino AS MonedaDestino, @Valor AS Valor) AS origen
                ON destino.[Fuente] = origen.[Fuente] AND destino.[Fecha] = origen.[Fecha]
                   AND destino.[MonedaOrigen] = origen.[MonedaOrigen] AND destino.[MonedaDestino] = origen.[MonedaDestino]
                WHEN MATCHED AND destino.[Valor] <> origen.[Valor] THEN
                    UPDATE SET [Valor] = origen.[Valor]
                WHEN NOT MATCHED THEN
                    INSERT ([Fuente], [Fecha], [MonedaOrigen], [MonedaDestino], [Valor])
                    VALUES (origen.[Fuente], origen.[Fecha], origen.[MonedaOrigen], origen.[MonedaDestino], origen.[Valor]);
                """;
            comando.Parameters.AddWithValue("@Fuente", fuente);
            comando.Parameters.AddWithValue("@Fecha", dia.ToDateTime(TimeOnly.MinValue));
            comando.Parameters.AddWithValue("@Origen", origen);
            comando.Parameters.AddWithValue("@Destino", destino);
            comando.Parameters.AddWithValue("@Valor", valor);
            actualizadas += await comando.ExecuteNonQueryAsync(cancellationToken);
        }

        var fuentePrincipal = tasas.Values.Select(v => v.Fuente).Distinct().Count() == 1
            ? tasas.Values.First().Fuente
            : "Compuesto";
        return (new ResultadoSincronizacionTasas(actualizadas, dia, fuentePrincipal), null);
    }

    private async Task<SqlConnection> AbrirAsync(CancellationToken cancellationToken)
    {
        var conexion = configuration.GetConnectionString("Globalizacion")
            ?? configuration.GetConnectionString("Neros")
            ?? throw new InvalidOperationException("Configura ConnectionStrings:Globalizacion o Neros.");
        var sql = new SqlConnection(conexion);
        await sql.OpenAsync(cancellationToken);
        return sql;
    }
}
