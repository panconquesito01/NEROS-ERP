using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Neros.Application.Globalizacion;
using Neros.Contracts.Globalizacion;

namespace Neros.Persistence.Globalizacion;

public sealed class ServicioCatalogosGlobalizacion(IConfiguration configuration) : IServicioCatalogosGlobalizacion
{
    public async Task<CatalogosTransversales> ObtenerAsync(string? paisTiposIdentificacion, CancellationToken cancellationToken)
    {
        var conexion = configuration.GetConnectionString("Globalizacion")
            ?? configuration.GetConnectionString("Neros")
            ?? throw new InvalidOperationException("Configura ConnectionStrings:Globalizacion o Neros.");
        await using var sql = new SqlConnection(conexion);
        await sql.OpenAsync(cancellationToken);

        var paises = await LeerAsync(sql,
            "SELECT [Codigo], [Nombre] FROM [globalizacion].[Pais] WHERE [Activo] = 1 ORDER BY [Nombre]",
            reader => new CatalogoPais(reader.GetString(0).Trim(), reader.GetString(1)), cancellationToken);

        var monedas = await LeerAsync(sql,
            "SELECT [Codigo], [Nombre], [DecimalesIso4217] FROM [globalizacion].[Moneda] WHERE [Activo] = 1 ORDER BY [Nombre]",
            reader => new CatalogoMoneda(reader.GetString(0).Trim(), reader.GetString(1), reader.GetByte(2)), cancellationToken);

        var zonas = await LeerAsync(sql,
            "SELECT [Id], [Nombre] FROM [globalizacion].[ZonaHoraria] WHERE [Activo] = 1 ORDER BY [Nombre]",
            reader => new CatalogoZonaHoraria(reader.GetString(0), reader.GetString(1)), cancellationToken);

        var unidades = await LeerAsync(sql,
            "SELECT [Codigo], [Nombre], [Simbolo] FROM [globalizacion].[UnidadMedida] WHERE [Activo] = 1 ORDER BY [Nombre]",
            reader => new CatalogoUnidadMedida(reader.GetString(0).Trim(), reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2)), cancellationToken);

        var filtroPais = string.IsNullOrWhiteSpace(paisTiposIdentificacion) ? null : paisTiposIdentificacion.Trim()[..2].ToUpperInvariant();
        await using var cmdTipos = sql.CreateCommand();
        cmdTipos.CommandText = filtroPais is null
            ? "SELECT [Pais], [Codigo], [Nombre] FROM [globalizacion].[TipoIdentificacion] WHERE [Activo] = 1 ORDER BY [Pais], [Nombre]"
            : "SELECT [Pais], [Codigo], [Nombre] FROM [globalizacion].[TipoIdentificacion] WHERE [Activo] = 1 AND [Pais] = @Pais ORDER BY [Nombre]";
        if (filtroPais is not null) cmdTipos.Parameters.AddWithValue("@Pais", filtroPais);
        var tipos = await LeerAsync(cmdTipos,
            reader => new CatalogoTipoIdentificacion(reader.GetString(0).Trim(), reader.GetString(1).Trim(), reader.GetString(2)),
            cancellationToken);

        return new CatalogosTransversales(paises, monedas, zonas, unidades, tipos);
    }

    private static async Task<List<T>> LeerAsync<T>(SqlConnection sql, string sqlText, Func<SqlDataReader, T> map,
        CancellationToken cancellationToken)
    {
        await using var comando = sql.CreateCommand();
        comando.CommandText = sqlText;
        return await LeerAsync(comando, map, cancellationToken);
    }

    private static async Task<List<T>> LeerAsync<T>(SqlCommand comando, Func<SqlDataReader, T> map, CancellationToken cancellationToken)
    {
        var lista = new List<T>();
        await using var reader = await comando.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            lista.Add(map(reader));
        return lista;
    }
}
