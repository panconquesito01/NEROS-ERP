using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Neros.Application.Operacion;
using Neros.Contracts.Operacion;

namespace Neros.Persistence.Operacion;

public sealed partial class ServicioProgramasOperativos(IConfiguration configuration) : IServicioProgramasOperativos
{
    private static readonly Guid TerceroPlaceholder = Guid.Parse("00000000-0000-4000-8000-000000000001");

    public async Task<PaginaProgramaOperativo> ListarAsync(Guid empresaId, string programa, int pagina, int tamano, CancellationToken cancellationToken)
    {
        if (RegistroProgramasOperativosSql.EsInforme(programa))
            return await ListarInformesAsync(empresaId, programa, cancellationToken);

        if (!RegistroProgramasOperativosSql.TryGet(programa, out var definicion))
            return Vacio(true, "Programa no reconocido.");

        var conexion = configuration.GetConnectionString(definicion.Conexion);
        if (string.IsNullOrWhiteSpace(conexion))
            return Vacio(false, $"Configure ConnectionStrings:{definicion.Conexion} y despliegue el módulo SQL.");

        try
        {
            await using var sql = new SqlConnection(conexion);
            await sql.OpenAsync(cancellationToken);
            var offset = Math.Max(0, pagina - 1) * tamano;
            await using var cmd = sql.CreateCommand();
            cmd.CommandText = definicion.SqlListado;
            cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@Tamano", tamano);
            if (programa.Equals("inventarios/conteos", StringComparison.OrdinalIgnoreCase))
                cmd.Parameters.AddWithValue("@EstadoFiltro", "Abierto");

            var filas = await LeerFilasAsync(cmd, cancellationToken);
            return new PaginaProgramaOperativo(filas.Filas, filas.Total, true, null);
        }
        catch (SqlException)
        {
            return Vacio(false, "El módulo no está desplegado o el esquema no coincide. Ejecute Database.Deploy para la base correspondiente.");
        }
    }

    public async Task<(BorradorProgramaCreado? Creado, string? Error)> CrearBorradorAsync(Guid empresaId, string programa, CancellationToken cancellationToken)
    {
        if (programa.Equals("inventarios/movimientos", StringComparison.OrdinalIgnoreCase))
            return await CrearBorradorMovimientoInventarioAsync(empresaId, cancellationToken);
        if (programa.Equals("compras/recepciones", StringComparison.OrdinalIgnoreCase))
            return await CrearBorradorRecepcionAsync(empresaId, cancellationToken);

        if (!RegistroProgramasOperativosSql.TryGet(programa, out var definicion) || definicion.SqlBorrador is null)
            return (null, CodigosProgramaOperativo.ProgramaDesconocido);

        var conexion = configuration.GetConnectionString(definicion.Conexion);
        if (string.IsNullOrWhiteSpace(conexion))
            return (null, CodigosProgramaOperativo.ModuloNoConfigurado);

        var id = Guid.NewGuid();
        try
        {
            await using var sql = new SqlConnection(conexion);
            await sql.OpenAsync(cancellationToken);
            await using var cmd = sql.CreateCommand();
            cmd.CommandText = definicion.SqlBorrador;
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
            cmd.Parameters.AddWithValue("@TenantId", empresaId);
            cmd.Parameters.AddWithValue("@TerceroId", TerceroPlaceholder);
            var filas = await cmd.ExecuteNonQueryAsync(cancellationToken);
            if (filas == 0 && programa.Equals("finanzas/comprobantes", StringComparison.OrdinalIgnoreCase))
                return (null, CodigosProgramaOperativo.ModuloNoConfigurado);
            return (new BorradorProgramaCreado(id.ToString("D"), null), null);
        }
        catch (SqlException)
        {
            return (null, CodigosProgramaOperativo.ModuloNoConfigurado);
        }
    }

    private async Task<PaginaProgramaOperativo> ListarInformesAsync(Guid empresaId, string programa, CancellationToken cancellationToken)
    {
        var filas = new List<FilaProgramaOperativo>();
        await AgregarConteoAsync(filas, "Ventas", "Cotizaciones", "Ventas", """
            SELECT COUNT(1) FROM [ventas].[Cotizacion] WHERE [EmpresaId]=@EmpresaId
            """, empresaId, cancellationToken);
        await AgregarConteoAsync(filas, "Ventas", "Pedidos", "Ventas", """
            SELECT COUNT(1) FROM [ventas].[Pedido] WHERE [EmpresaId]=@EmpresaId
            """, empresaId, cancellationToken);
        await AgregarConteoAsync(filas, "Inventario", "Productos", "Inventario", """
            SELECT COUNT(1) FROM [inventario].[Producto] WHERE [EmpresaId]=@EmpresaId
            """, empresaId, cancellationToken);
        await AgregarConteoAsync(filas, "Compras", "Órdenes", "Compras", """
            SELECT COUNT(1) FROM [compras].[OrdenCompra] WHERE [EmpresaId]=@EmpresaId
            """, empresaId, cancellationToken);
        await AgregarConteoAsync(filas, "Cartera", "Pagos", "Cartera", """
            SELECT COUNT(1) FROM [cartera].[Pago] WHERE [EmpresaId]=@EmpresaId
            """, empresaId, cancellationToken);

        if (filas.Count == 0)
            filas.Add(new FilaProgramaOperativo(Guid.NewGuid(), "—", "Sin datos", DateOnly.FromDateTime(DateTime.UtcNow),
                "Despliegue las bases operativas para ver indicadores.", 0));

        var titulo = programa switch
        {
            "informes/gerenciales" => "Gerencial",
            "informes/presupuesto" => "Presupuesto",
            "informes/busqueda" => "Búsqueda",
            "informes/indicadores" => "Indicadores",
            "informes/exportaciones" => "Exportación",
            _ => "Operativo"
        };
        filas.Insert(0, new FilaProgramaOperativo(Guid.NewGuid(), titulo, "Resumen", DateOnly.FromDateTime(DateTime.UtcNow),
            $"Indicadores {titulo.ToLowerInvariant()} por empresa", filas.Sum(f => f.Total ?? 0)));

        return new PaginaProgramaOperativo(filas, filas.Count, true, null);
    }

    private async Task AgregarConteoAsync(
        List<FilaProgramaOperativo> filas, string modulo, string concepto, string conexionNombre, string sql, Guid empresaId, CancellationToken cancellationToken)
    {
        var conexion = configuration.GetConnectionString(conexionNombre);
        if (string.IsNullOrWhiteSpace(conexion)) return;
        try
        {
            await using var cn = new SqlConnection(conexion);
            await cn.OpenAsync(cancellationToken);
            await using var cmd = cn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
            var count = (int)(await cmd.ExecuteScalarAsync(cancellationToken) ?? 0);
            filas.Add(new FilaProgramaOperativo(Guid.NewGuid(), modulo, concepto, DateOnly.FromDateTime(DateTime.UtcNow), concepto, count));
        }
        catch (SqlException)
        {
            // módulo no desplegado
        }
    }

    private async Task<(BorradorProgramaCreado? Creado, string? Error)> CrearBorradorRecepcionAsync(Guid empresaId, CancellationToken cancellationToken)
    {
        var conexion = configuration.GetConnectionString("Compras");
        if (string.IsNullOrWhiteSpace(conexion))
            return (null, CodigosProgramaOperativo.ModuloNoConfigurado);

        var id = Guid.NewGuid();
        const string sql = """
            DECLARE @OrdenId uniqueidentifier = (SELECT TOP (1) [Id] FROM [compras].[OrdenCompra] WHERE [EmpresaId]=@EmpresaId ORDER BY [FechaDocumento] DESC);
            IF @OrdenId IS NULL RETURN;
            INSERT INTO [compras].[RecepcionCompra]
                ([Id],[TenantId],[EmpresaId],[OrdenCompraId],[Numero],[FechaRecepcion],[RegistradoPorUsuarioId])
            VALUES (@Id,@TenantId,@EmpresaId,@OrdenId,CONCAT('REC-',FORMAT(@Id,'N')[1,8]),CAST(SYSUTCDATETIME() AS date),@UsuarioId);
            """;
        try
        {
            await using var cn = new SqlConnection(conexion);
            await cn.OpenAsync(cancellationToken);
            await using var cmd = cn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
            cmd.Parameters.AddWithValue("@TenantId", empresaId);
            cmd.Parameters.AddWithValue("@UsuarioId", TerceroPlaceholder);
            if (await cmd.ExecuteNonQueryAsync(cancellationToken) == 0)
                return (null, CodigosProgramaOperativo.ModuloNoConfigurado);
            return (new BorradorProgramaCreado(id.ToString("D"), null), null);
        }
        catch (SqlException)
        {
            return (null, CodigosProgramaOperativo.ModuloNoConfigurado);
        }
    }

    private async Task<(BorradorProgramaCreado? Creado, string? Error)> CrearBorradorMovimientoInventarioAsync(Guid empresaId, CancellationToken cancellationToken)
    {
        var conexion = configuration.GetConnectionString("Inventario");
        if (string.IsNullOrWhiteSpace(conexion))
            return (null, CodigosProgramaOperativo.ModuloNoConfigurado);

        var id = Guid.NewGuid();
        const string sql = """
            DECLARE @ProductoId uniqueidentifier = (SELECT TOP (1) [Id] FROM [inventario].[Producto] WHERE [EmpresaId]=@EmpresaId ORDER BY [Codigo]);
            DECLARE @BodegaId uniqueidentifier = (SELECT TOP (1) [Id] FROM [inventario].[Bodega] WHERE [EmpresaId]=@EmpresaId ORDER BY [Codigo]);
            IF @ProductoId IS NULL OR @BodegaId IS NULL RETURN;
            DECLARE @Secuencia int = (SELECT ISNULL(MAX([Secuencia]), 0) + 1 FROM [inventario].[MovimientoInventario] WHERE [EmpresaId]=@EmpresaId);
            INSERT INTO [inventario].[MovimientoInventario]
                ([Id],[TenantId],[EmpresaId],[ProductoId],[BodegaId],[Fecha],[Secuencia],[Tipo],[EsEntrada],[Cantidad],[CostoUnitarioAplicado],[ValorTotal],[ModuloOrigen])
            VALUES (@Id,@TenantId,@EmpresaId,@ProductoId,@BodegaId,CAST(SYSUTCDATETIME() AS date),@Secuencia,'Entrada',1,1,0,0,'Manual');
            """;
        try
        {
            await using var cn = new SqlConnection(conexion);
            await cn.OpenAsync(cancellationToken);
            await using var cmd = cn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
            cmd.Parameters.AddWithValue("@TenantId", empresaId);
            if (await cmd.ExecuteNonQueryAsync(cancellationToken) == 0)
                return (null, CodigosProgramaOperativo.ModuloNoConfigurado);
            return (new BorradorProgramaCreado(id.ToString("D"), null), null);
        }
        catch (SqlException)
        {
            return (null, CodigosProgramaOperativo.ModuloNoConfigurado);
        }
    }

    private static async Task<(List<FilaProgramaOperativo> Filas, int Total)> LeerFilasAsync(SqlCommand cmd, CancellationToken cancellationToken)
    {
        var filas = new List<FilaProgramaOperativo>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            filas.Add(new FilaProgramaOperativo(
                reader.GetGuid(0),
                reader.IsDBNull(1) ? null : reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : DateOnly.FromDateTime(reader.GetDateTime(3)),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetDecimal(5)));
        }
        var total = filas.Count;
        if (await reader.NextResultAsync(cancellationToken) && await reader.ReadAsync(cancellationToken))
            total = reader.GetInt32(0);
        return (filas, total);
    }

    private static PaginaProgramaOperativo Vacio(bool moduloDisponible, string aviso) =>
        new([], 0, moduloDisponible, aviso);
}
