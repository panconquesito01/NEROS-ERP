using Microsoft.Data.SqlClient;
using Neros.Contracts.Busqueda;
using Neros.Domain.Busqueda;

namespace Neros.Search.Api;

public sealed class ServicioBusqueda(string cadenaConexion)
{
    public async Task<PaginaBusquedaDto> BuscarAsync(
        Guid tenantId,
        Guid empresaId,
        IReadOnlySet<string> permisosCodigoEmpresa,
        string? termino,
        int maxResultados,
        string? cursorTitulo,
        Guid? cursorId,
        CancellationToken cancellationToken)
    {
        var indice = await CargarIndiceAsync(tenantId, empresaId, cancellationToken);
        var checkpoints = await CargarCheckpointsAsync(tenantId, empresaId, cancellationToken);
        var criterio = new CriterioBusqueda(tenantId, empresaId, termino ?? "", maxResultados, cursorTitulo, cursorId);
        var resultado = MotorConsultaBusqueda.Buscar(indice, permisosCodigoEmpresa, criterio, checkpoints);
        return new PaginaBusquedaDto(
            resultado.Documentos.Select(d => new DocumentoBusquedaDto(
                d.Id, d.TipoEntidad, d.EntidadId, d.Titulo, d.Resumen, d.IndexadoEnUtc)).ToList(),
            resultado.SiguienteCursorTitulo,
            resultado.SiguienteCursorId,
            resultado.Frescura.UltimoIndexadoUtc,
            resultado.Frescura.ModuloMasReciente);
    }

    private async Task<List<DocumentoIndice>> CargarIndiceAsync(Guid tenantId, Guid empresaId, CancellationToken cancellationToken)
    {
        var lista = new List<DocumentoIndice>();
        await using var conexion = new SqlConnection(cadenaConexion);
        await conexion.OpenAsync(cancellationToken);
        await using var cmd = conexion.CreateCommand();
        cmd.CommandText = """
            SELECT [Id],[TipoEntidad],[EntidadId],[VersionIndice],[Titulo],[Resumen],[TextoBusqueda],[PermisoRequerido],
                   [OrigenModulo],[CorrelationId],[Activo],[TombstoneEnUtc],[IndexadoEnUtc]
            FROM [busqueda].[DocumentoIndice]
            WHERE [TenantId]=@TenantId AND [EmpresaId]=@EmpresaId;
            """;
        cmd.Parameters.AddWithValue("@TenantId", tenantId);
        cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(new DocumentoIndice(
                reader.GetGuid(0),
                tenantId,
                empresaId,
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt64(3),
                reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5),
                reader.GetString(6),
                reader.GetString(7),
                reader.GetString(8),
                reader.GetGuid(9),
                reader.GetBoolean(10),
                reader.IsDBNull(11) ? null : reader.GetDateTime(11),
                reader.GetDateTime(12)));
        }
        return lista;
    }

    private async Task<List<CheckpointIngesta>> CargarCheckpointsAsync(
        Guid tenantId, Guid empresaId, CancellationToken cancellationToken)
    {
        var lista = new List<CheckpointIngesta>();
        await using var conexion = new SqlConnection(cadenaConexion);
        await conexion.OpenAsync(cancellationToken);
        await using var cmd = conexion.CreateCommand();
        cmd.CommandText = """
            SELECT [FuenteModulo],[UltimoInstanteUtc],[UltimoEventoId]
            FROM [busqueda].[CheckpointIngesta]
            WHERE [TenantId]=@TenantId AND [EmpresaId]=@EmpresaId;
            """;
        cmd.Parameters.AddWithValue("@TenantId", tenantId);
        cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            lista.Add(new CheckpointIngesta(tenantId, empresaId, reader.GetString(0), reader.GetDateTime(1),
                reader.IsDBNull(2) ? null : reader.GetGuid(2)));
        return lista;
    }
}
