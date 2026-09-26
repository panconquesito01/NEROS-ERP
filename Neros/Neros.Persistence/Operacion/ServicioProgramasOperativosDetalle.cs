using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Neros.Contracts.Operacion;

namespace Neros.Persistence.Operacion;

public sealed partial class ServicioProgramasOperativos
{
    public async Task<(DetalleProgramaOperativo? Detalle, string? Error)> ObtenerDetalleAsync(
        Guid empresaId, string programa, Guid id, CancellationToken cancellationToken)
    {
        if (!RegistroProgramasOperativosSql.TryGet(programa, out var definicion))
            return (null, CodigosProgramaOperativo.ProgramaDesconocido);

        var conexion = configuration.GetConnectionString(definicion.Conexion);
        if (string.IsNullOrWhiteSpace(conexion))
            return (null, CodigosProgramaOperativo.ModuloNoConfigurado);

        try
        {
            return programa.ToLowerInvariant() switch
            {
                "ventas/cotizaciones" => await DetalleDocumentoVentasAsync(conexion, empresaId, id, programa, "Cotizacion", "CotizacionLinea", "CotizacionId", definicion, cancellationToken),
                "ventas/pedidos" => await DetalleDocumentoVentasAsync(conexion, empresaId, id, programa, "Pedido", "PedidoLinea", "PedidoId", definicion, cancellationToken),
                "compras/ordenes" => await DetalleOrdenCompraAsync(conexion, empresaId, id, definicion, cancellationToken),
                "ventas/facturacion" => await DetalleFacturacionAsync(conexion, empresaId, id, definicion, cancellationToken),
                "inventarios/productos" => await DetalleMaestroAsync(conexion, empresaId, id, "inventario.Producto", "Codigo", "Nombre", "Activo", cancellationToken),
                "inventarios/bodegas" => await DetalleMaestroAsync(conexion, empresaId, id, "inventario.Bodega", "Codigo", "Nombre", "Activa", cancellationToken),
                "ventas/clientes" or "compras/proveedores" => await DetalleTerceroAsync(conexion, empresaId, id, cancellationToken),
                "finanzas/cuentas-cobrar" or "finanzas/cuentas-pagar" => await DetalleCarteraAsync(conexion, empresaId, id, definicion, cancellationToken),
                "finanzas/pagos" => await DetallePagoAsync(conexion, empresaId, id, cancellationToken),
                "finanzas/tesoreria-cuentas" => await DetalleTesoreriaCuentaAsync(conexion, empresaId, id, cancellationToken),
                "compras/recepciones" => await DetalleRecepcionAsync(conexion, empresaId, id, cancellationToken),
                "inventarios/movimientos" => await DetalleMovimientoInventarioAsync(conexion, empresaId, id, cancellationToken),
                _ => await DetalleGenericoAsync(conexion, empresaId, id, programa, definicion, cancellationToken)
            };
        }
        catch (SqlException)
        {
            return (null, CodigosProgramaOperativo.NoEncontrado);
        }
    }

    public async Task<string?> GuardarAsync(
        Guid empresaId, string programa, Guid id, SolicitudGuardarPrograma solicitud, CancellationToken cancellationToken)
    {
        if (!RegistroProgramasOperativosSql.TryGet(programa, out var definicion))
            return CodigosProgramaOperativo.ProgramaDesconocido;

        var conexion = configuration.GetConnectionString(definicion.Conexion);
        if (string.IsNullOrWhiteSpace(conexion))
            return CodigosProgramaOperativo.ModuloNoConfigurado;

        var campos = solicitud.Campos;
        try
        {
            await using var sql = new SqlConnection(conexion);
            await sql.OpenAsync(cancellationToken);
            var cmdText = programa.ToLowerInvariant() switch
            {
                "ventas/cotizaciones" => """
                    UPDATE [ventas].[Cotizacion] SET [ClienteRazonSocial]=@Ref, [FechaDocumento]=@Fecha
                    WHERE [Id]=@Id AND [EmpresaId]=@EmpresaId AND [Estado]='Borrador'
                    """,
                "ventas/pedidos" => """
                    UPDATE [ventas].[Pedido] SET [ClienteRazonSocial]=@Ref, [FechaDocumento]=@Fecha
                    WHERE [Id]=@Id AND [EmpresaId]=@EmpresaId AND [Estado]='Borrador'
                    """,
                "compras/ordenes" => """
                    UPDATE [compras].[OrdenCompra] SET [ProveedorRazonSocial]=@Ref, [FechaDocumento]=@Fecha
                    WHERE [Id]=@Id AND [EmpresaId]=@EmpresaId AND [Estado]='Borrador'
                    """,
                "inventarios/productos" => """
                    UPDATE [inventario].[Producto] SET [Codigo]=@Codigo, [Nombre]=@Nombre
                    WHERE [Id]=@Id AND [EmpresaId]=@EmpresaId
                    """,
                "inventarios/bodegas" => """
                    UPDATE [inventario].[Bodega] SET [Codigo]=@Codigo, [Nombre]=@Nombre
                    WHERE [Id]=@Id AND [EmpresaId]=@EmpresaId
                    """,
                "ventas/clientes" or "compras/proveedores" => """
                    UPDATE [terceros].[Tercero] SET [RazonSocial]=@Ref WHERE [Id]=@Id AND [TenantId]=@EmpresaId
                    """,
                "ventas/facturacion" => """
                    UPDATE [facturacion].[DocumentoComercial] SET [ClienteRazonSocial]=@Ref, [FechaDocumento]=@Fecha
                    WHERE [Id]=@Id AND [EmpresaId]=@EmpresaId AND [Estado]='Borrador'
                    """,
                _ => null
            };
            if (cmdText is null) return CodigosProgramaOperativo.NoEditable;

            await using var cmd = sql.CreateCommand();
            cmd.CommandText = cmdText;
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
            cmd.Parameters.AddWithValue("@Ref", (object?)Valor(campos, "referencia", "cliente", "proveedor", "razonsocial") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Codigo", (object?)Valor(campos, "codigo", "numero") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Nombre", (object?)Valor(campos, "nombre", "descripcion") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Fecha", ParseFecha(campos) ?? DateOnly.FromDateTime(DateTime.UtcNow));
            if (await cmd.ExecuteNonQueryAsync(cancellationToken) == 0)
                return CodigosProgramaOperativo.NoEncontrado;
            return null;
        }
        catch (SqlException)
        {
            return CodigosProgramaOperativo.NoEditable;
        }
    }

    public async Task<(LineaProgramaOperativo? Linea, string? Error)> AgregarLineaAsync(
        Guid empresaId, string programa, Guid id, SolicitudLineaPrograma solicitud, CancellationToken cancellationToken)
    {
        if (!RegistroProgramasOperativosSql.TryGet(programa, out var definicion) || !definicion.SoportaLineas)
            return (null, CodigosProgramaOperativo.ProgramaDesconocido);

        var conexion = configuration.GetConnectionString(definicion.Conexion);
        if (string.IsNullOrWhiteSpace(conexion))
            return (null, CodigosProgramaOperativo.ModuloNoConfigurado);

        var lineaId = Guid.NewGuid();
        var bruto = solicitud.Cantidad * solicitud.PrecioUnitario;
        try
        {
            await using var sql = new SqlConnection(conexion);
            await sql.OpenAsync(cancellationToken);
            string cmdText;
            switch (programa.ToLowerInvariant())
            {
                case "ventas/cotizaciones":
                    cmdText = ScriptLineaVentas("Cotizacion", "CotizacionLinea", "CotizacionId");
                    break;
                case "ventas/pedidos":
                    cmdText = ScriptLineaVentas("Pedido", "PedidoLinea", "PedidoId");
                    break;
                case "compras/ordenes":
                    cmdText = """
                        DECLARE @Num int = (SELECT ISNULL(MAX([LineaNumero]),0)+1 FROM [compras].[OrdenCompraLinea] WHERE [OrdenCompraId]=@DocId);
                        INSERT INTO [compras].[OrdenCompraLinea]
                            ([Id],[OrdenCompraId],[LineaNumero],[Descripcion],[CantidadPedida],[PrecioUnitario],[Bruto],[BaseNeta])
                        VALUES (@LineaId,@DocId,@Num,@Desc,@Cant,@Precio,@Bruto,@Bruto);
                        UPDATE [compras].[OrdenCompra] SET [Subtotal]=(SELECT SUM([BaseNeta]) FROM [compras].[OrdenCompraLinea] WHERE [OrdenCompraId]=@DocId),
                            [Total]=(SELECT SUM([BaseNeta]) FROM [compras].[OrdenCompraLinea] WHERE [OrdenCompraId]=@DocId)
                        WHERE [Id]=@DocId AND [EmpresaId]=@EmpresaId;
                        """;
                    break;
                case "ventas/facturacion":
                    cmdText = """
                        DECLARE @Num int = (SELECT ISNULL(MAX([LineaNumero]),0)+1 FROM [facturacion].[DocumentoComercialLinea] WHERE [DocumentoComercialId]=@DocId);
                        INSERT INTO [facturacion].[DocumentoComercialLinea]
                            ([Id],[DocumentoComercialId],[LineaNumero],[Descripcion],[Cantidad],[PrecioUnitario],[BaseNeta])
                        VALUES (@LineaId,@DocId,@Num,@Desc,@Cant,@Precio,@Bruto);
                        UPDATE [facturacion].[DocumentoComercial] SET [Subtotal]=(SELECT SUM([BaseNeta]) FROM [facturacion].[DocumentoComercialLinea] WHERE [DocumentoComercialId]=@DocId),
                            [Total]=(SELECT SUM([BaseNeta]) FROM [facturacion].[DocumentoComercialLinea] WHERE [DocumentoComercialId]=@DocId)
                        WHERE [Id]=@DocId AND [EmpresaId]=@EmpresaId;
                        """;
                    break;
                default:
                    return (null, CodigosProgramaOperativo.ProgramaDesconocido);
            }

            await using var cmd = sql.CreateCommand();
            cmd.CommandText = cmdText;
            cmd.Parameters.AddWithValue("@LineaId", lineaId);
            cmd.Parameters.AddWithValue("@DocId", id);
            cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
            cmd.Parameters.AddWithValue("@Desc", solicitud.Descripcion);
            cmd.Parameters.AddWithValue("@Cant", solicitud.Cantidad);
            cmd.Parameters.AddWithValue("@Precio", solicitud.PrecioUnitario);
            cmd.Parameters.AddWithValue("@Bruto", bruto);
            await cmd.ExecuteNonQueryAsync(cancellationToken);

            return (new LineaProgramaOperativo(lineaId, 0, solicitud.Descripcion, solicitud.Cantidad, solicitud.PrecioUnitario, bruto), null);
        }
        catch (SqlException)
        {
            return (null, CodigosProgramaOperativo.NoEditable);
        }
    }

    public async Task<string?> EjecutarAccionAsync(
        Guid empresaId, string programa, Guid id, string accion, CancellationToken cancellationToken)
    {
        if (!RegistroProgramasOperativosSql.TryGet(programa, out var definicion))
            return CodigosProgramaOperativo.ProgramaDesconocido;

        var conexion = configuration.GetConnectionString(definicion.Conexion);
        if (string.IsNullOrWhiteSpace(conexion))
            return CodigosProgramaOperativo.ModuloNoConfigurado;

        var accionNorm = accion.Trim().ToLowerInvariant();
        if (accionNorm is not (AccionesPrograma.Confirmar or AccionesPrograma.Anular))
            return CodigosProgramaOperativo.ProgramaDesconocido;

        var nuevoEstado = accionNorm == AccionesPrograma.Confirmar ? "Confirmado" : "Anulado";
        if (programa.Equals("ventas/facturacion", StringComparison.OrdinalIgnoreCase))
            nuevoEstado = accionNorm == AccionesPrograma.Confirmar ? "Emitido" : "Anulado";

        string? sqlUpdate = programa.ToLowerInvariant() switch
        {
            "ventas/cotizaciones" => UpdateEstado("ventas", "Cotizacion", nuevoEstado),
            "ventas/pedidos" => UpdateEstado("ventas", "Pedido", nuevoEstado),
            "compras/ordenes" => UpdateEstado("compras", "OrdenCompra", nuevoEstado),
            "ventas/facturacion" => UpdateEstado("facturacion", "DocumentoComercial", nuevoEstado),
            "finanzas/cuentas-cobrar" or "finanzas/cuentas-pagar" => null,
            "finanzas/comprobantes" => accionNorm == AccionesPrograma.Confirmar
                ? "UPDATE [contabilidad].[Comprobante] SET [Estado]='Contabilizado' WHERE [Id]=@Id AND [EmpresaId]=@EmpresaId AND [Estado]='Borrador'"
                : "UPDATE [contabilidad].[Comprobante] SET [Estado]='Reversado' WHERE [Id]=@Id AND [EmpresaId]=@EmpresaId AND [Estado]='Borrador'",
            _ when definicion.SoportaConfirmar => null,
            _ => null
        };

        if (sqlUpdate is null && !definicion.SoportaConfirmar)
            return CodigosProgramaOperativo.NoEditable;

        sqlUpdate ??= UpdateEstadoPorPrograma(programa, nuevoEstado);
        if (sqlUpdate is null) return CodigosProgramaOperativo.NoEditable;

        try
        {
            await using var cn = new SqlConnection(conexion);
            await cn.OpenAsync(cancellationToken);
            await using var cmd = cn.CreateCommand();
            cmd.CommandText = sqlUpdate;
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
            if (await cmd.ExecuteNonQueryAsync(cancellationToken) == 0)
                return CodigosProgramaOperativo.NoEncontrado;
            return null;
        }
        catch (SqlException)
        {
            return CodigosProgramaOperativo.NoEditable;
        }
    }

    private static string ScriptLineaVentas(string doc, string linea, string fk) => $"""
        DECLARE @Num int = (SELECT ISNULL(MAX([LineaNumero]),0)+1 FROM [ventas].[{linea}] WHERE [{fk}]=@DocId);
        INSERT INTO [ventas].[{linea}]
            ([Id],[{fk}],[LineaNumero],[Descripcion],[Cantidad],[PrecioUnitario],[Bruto],[BaseNeta])
        VALUES (@LineaId,@DocId,@Num,@Desc,@Cant,@Precio,@Bruto,@Bruto);
        UPDATE [ventas].[{doc}] SET [Subtotal]=(SELECT SUM([BaseNeta]) FROM [ventas].[{linea}] WHERE [{fk}]=@DocId),
            [Total]=(SELECT SUM([BaseNeta]) FROM [ventas].[{linea}] WHERE [{fk}]=@DocId)
        WHERE [Id]=@DocId AND [EmpresaId]=@EmpresaId;
        """;

    private static string UpdateEstado(string schema, string tabla, string estado) =>
        $"UPDATE [{schema}].[{tabla}] SET [Estado]='{estado}' WHERE [Id]=@Id AND [EmpresaId]=@EmpresaId AND [Estado]='Borrador'";

    private static string? UpdateEstadoPorPrograma(string programa, string estado) =>
        programa.ToLowerInvariant() switch
        {
            "finanzas/cuentas-cobrar" or "finanzas/cuentas-pagar" => null,
            _ => null
        };

    private async Task<(DetalleProgramaOperativo? Detalle, string? Error)> DetalleDocumentoVentasAsync(
        string conexion, Guid empresaId, Guid id, string programa, string doc, string linea, string fk, RegistroProgramasOperativosSql.Definicion definicion, CancellationToken cancellationToken)
    {
        await using var sql = new SqlConnection(conexion);
        await sql.OpenAsync(cancellationToken);
        await using var cmd = sql.CreateCommand();
        cmd.CommandText = $"""
            SELECT [Numero],[Estado],[FechaDocumento],[ClienteRazonSocial],[Total],[MonedaCodigo]
            FROM [ventas].[{doc}] WHERE [Id]=@Id AND [EmpresaId]=@EmpresaId;
            SELECT [Id],[LineaNumero],[Descripcion],[Cantidad],[PrecioUnitario],[BaseNeta]
            FROM [ventas].[{linea}] WHERE [{fk}]=@Id ORDER BY [LineaNumero];
            """;
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return (null, CodigosProgramaOperativo.NoEncontrado);

        var estado = reader.GetString(1);
        var campos = Campos(
            ("numero", reader.IsDBNull(0) ? null : reader.GetString(0)),
            ("estado", estado),
            ("fecha", reader.IsDBNull(2) ? null : DateOnly.FromDateTime(reader.GetDateTime(2)).ToString("yyyy-MM-dd")),
            ("cliente", reader.IsDBNull(3) ? null : reader.GetString(3)),
            ("total", reader.IsDBNull(4) ? null : reader.GetDecimal(4).ToString("G")),
            ("moneda", reader.IsDBNull(5) ? null : reader.GetString(5)));

        var lineas = new List<LineaProgramaOperativo>();
        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                lineas.Add(new LineaProgramaOperativo(
                    reader.GetGuid(0), reader.GetInt32(1), reader.GetString(2),
                    reader.GetDecimal(3), reader.GetDecimal(4), reader.GetDecimal(5)));
            }
        }

        var editable = estado == "Borrador";
        var acciones = editable && definicion.SoportaConfirmar
            ? new[] { AccionesPrograma.Confirmar, AccionesPrograma.Anular }
            : Array.Empty<string>();
        return (new DetalleProgramaOperativo(programa, id, campos, lineas, editable, acciones), null);
    }

    private async Task<(DetalleProgramaOperativo? Detalle, string? Error)> DetalleOrdenCompraAsync(
        string conexion, Guid empresaId, Guid id, RegistroProgramasOperativosSql.Definicion definicion, CancellationToken cancellationToken)
    {
        await using var sql = new SqlConnection(conexion);
        await sql.OpenAsync(cancellationToken);
        await using var cmd = sql.CreateCommand();
        cmd.CommandText = """
            SELECT [Numero],[Estado],[FechaDocumento],[ProveedorRazonSocial],[Total],[MonedaCodigo]
            FROM [compras].[OrdenCompra] WHERE [Id]=@Id AND [EmpresaId]=@EmpresaId;
            SELECT [Id],[LineaNumero],[Descripcion],[CantidadPedida],[PrecioUnitario],[BaseNeta]
            FROM [compras].[OrdenCompraLinea] WHERE [OrdenCompraId]=@Id ORDER BY [LineaNumero];
            """;
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return (null, CodigosProgramaOperativo.NoEncontrado);
        var estado = reader.GetString(1);
        var campos = Campos(
            ("numero", reader.IsDBNull(0) ? null : reader.GetString(0)),
            ("estado", estado),
            ("fecha", reader.IsDBNull(2) ? null : DateOnly.FromDateTime(reader.GetDateTime(2)).ToString("yyyy-MM-dd")),
            ("proveedor", reader.IsDBNull(3) ? null : reader.GetString(3)),
            ("total", reader.IsDBNull(4) ? null : reader.GetDecimal(4).ToString("G")),
            ("moneda", reader.IsDBNull(5) ? null : reader.GetString(5)));
        var lineas = new List<LineaProgramaOperativo>();
        if (await reader.NextResultAsync(cancellationToken))
            while (await reader.ReadAsync(cancellationToken))
                lineas.Add(new LineaProgramaOperativo(reader.GetGuid(0), reader.GetInt32(1), reader.GetString(2),
                    reader.GetDecimal(3), reader.GetDecimal(4), reader.GetDecimal(5)));
        var editable = estado == "Borrador";
        var acciones = editable && definicion.SoportaConfirmar ? new[] { AccionesPrograma.Confirmar, AccionesPrograma.Anular } : [];
        return (new DetalleProgramaOperativo("compras/ordenes", id, campos, lineas, editable, acciones), null);
    }

    private async Task<(DetalleProgramaOperativo? Detalle, string? Error)> DetalleFacturacionAsync(
        string conexion, Guid empresaId, Guid id, RegistroProgramasOperativosSql.Definicion definicion, CancellationToken cancellationToken)
    {
        await using var sql = new SqlConnection(conexion);
        await sql.OpenAsync(cancellationToken);
        await using var cmd = sql.CreateCommand();
        cmd.CommandText = """
            SELECT [TipoDocumento],[Estado],[FechaDocumento],[ClienteRazonSocial],[Total],[MonedaCodigo]
            FROM [facturacion].[DocumentoComercial] WHERE [Id]=@Id AND [EmpresaId]=@EmpresaId;
            SELECT [Id],[LineaNumero],[Descripcion],[Cantidad],[PrecioUnitario],[BaseNeta]
            FROM [facturacion].[DocumentoComercialLinea] WHERE [DocumentoComercialId]=@Id ORDER BY [LineaNumero];
            """;
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return (null, CodigosProgramaOperativo.NoEncontrado);
        var estado = reader.GetString(1);
        var campos = Campos(
            ("tipo", reader.GetString(0)),
            ("estado", estado),
            ("fecha", DateOnly.FromDateTime(reader.GetDateTime(2)).ToString("yyyy-MM-dd")),
            ("cliente", reader.IsDBNull(3) ? null : reader.GetString(3)),
            ("total", reader.GetDecimal(4).ToString("G")),
            ("moneda", reader.GetString(5)));
        var lineas = new List<LineaProgramaOperativo>();
        if (await reader.NextResultAsync(cancellationToken))
            while (await reader.ReadAsync(cancellationToken))
                lineas.Add(new LineaProgramaOperativo(reader.GetGuid(0), reader.GetInt32(1), reader.GetString(2),
                    reader.GetDecimal(3), reader.GetDecimal(4), reader.GetDecimal(5)));
        var editable = estado == "Borrador";
        var acciones = editable ? new[] { AccionesPrograma.Confirmar, AccionesPrograma.Anular } : [];
        return (new DetalleProgramaOperativo("ventas/facturacion", id, campos, lineas, editable, acciones), null);
    }

    private static async Task<(DetalleProgramaOperativo? Detalle, string? Error)> DetalleMaestroAsync(
        string conexion, Guid empresaId, Guid id, string tabla, string codigo, string nombre, string activoCol, CancellationToken cancellationToken)
    {
        var schema = tabla.Split('.')[0];
        var table = tabla.Split('.')[1];
        await using var sql = new SqlConnection(conexion);
        await sql.OpenAsync(cancellationToken);
        await using var cmd = sql.CreateCommand();
        cmd.CommandText = $"SELECT [{codigo}],[{nombre}],[{activoCol}] FROM [{schema}].[{table}] WHERE [Id]=@Id AND [EmpresaId]=@EmpresaId";
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return (null, CodigosProgramaOperativo.NoEncontrado);
        var campos = Campos(
            ("codigo", reader.GetString(0)),
            ("nombre", reader.GetString(1)),
            ("activo", reader.GetBoolean(2) ? "1" : "0"));
        return (new DetalleProgramaOperativo($"inventarios/{table.ToLowerInvariant()}", id, campos, [], true, []), null);
    }

    private static async Task<(DetalleProgramaOperativo? Detalle, string? Error)> DetalleTerceroAsync(
        string conexion, Guid empresaId, Guid id, CancellationToken cancellationToken)
    {
        await using var sql = new SqlConnection(conexion);
        await sql.OpenAsync(cancellationToken);
        await using var cmd = sql.CreateCommand();
        cmd.CommandText = """
            SELECT t.[RazonSocial], t.[Activo], i.[Numero]
            FROM [terceros].[Tercero] t
            LEFT JOIN [terceros].[Identificacion] i ON i.[TerceroId]=t.[Id] AND i.[EsPrincipal]=1
            WHERE t.[Id]=@Id AND t.[TenantId]=@EmpresaId
            """;
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return (null, CodigosProgramaOperativo.NoEncontrado);
        var campos = Campos(
            ("razonsocial", reader.GetString(0)),
            ("activo", reader.GetBoolean(1) ? "1" : "0"),
            ("identificacion", reader.IsDBNull(2) ? null : reader.GetString(2)));
        return (new DetalleProgramaOperativo("terceros", id, campos, [], true, []), null);
    }

    private static async Task<(DetalleProgramaOperativo? Detalle, string? Error)> DetalleCarteraAsync(
        string conexion, Guid empresaId, Guid id, RegistroProgramasOperativosSql.Definicion definicion, CancellationToken cancellationToken)
    {
        await using var sql = new SqlConnection(conexion);
        await sql.OpenAsync(cancellationToken);
        await using var cmd = sql.CreateCommand();
        cmd.CommandText = """
            SELECT [Numero],[TipoCartera],[FechaDocumento],[TotalDocumento],[MonedaCodigo]
            FROM [cartera].[Documento] WHERE [Id]=@Id AND [EmpresaId]=@EmpresaId
            """;
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return (null, CodigosProgramaOperativo.NoEncontrado);
        var campos = Campos(
            ("numero", reader.GetString(0)),
            ("tipo", reader.GetString(1)),
            ("fecha", DateOnly.FromDateTime(reader.GetDateTime(2)).ToString("yyyy-MM-dd")),
            ("total", reader.GetDecimal(3).ToString("G")),
            ("moneda", reader.GetString(4)));
        return (new DetalleProgramaOperativo("cartera/documento", id, campos, [], true, definicion.SoportaConfirmar ? [AccionesPrograma.Confirmar] : []), null);
    }

    private static async Task<(DetalleProgramaOperativo? Detalle, string? Error)> DetallePagoAsync(
        string conexion, Guid empresaId, Guid id, CancellationToken cancellationToken)
    {
        await using var sql = new SqlConnection(conexion);
        await sql.OpenAsync(cancellationToken);
        await using var cmd = sql.CreateCommand();
        cmd.CommandText = """
            SELECT [Numero],[TipoCartera],[FechaPago],[ImportePago],[MonedaCodigo]
            FROM [cartera].[Pago] WHERE [Id]=@Id AND [EmpresaId]=@EmpresaId
            """;
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return (null, CodigosProgramaOperativo.NoEncontrado);
        var campos = Campos(
            ("numero", reader.GetString(0)),
            ("tipo", reader.GetString(1)),
            ("fecha", DateOnly.FromDateTime(reader.GetDateTime(2)).ToString("yyyy-MM-dd")),
            ("importe", reader.GetDecimal(3).ToString("G")),
            ("moneda", reader.GetString(4)));
        return (new DetalleProgramaOperativo("finanzas/pagos", id, campos, [], false, []), null);
    }

    private static async Task<(DetalleProgramaOperativo? Detalle, string? Error)> DetalleTesoreriaCuentaAsync(
        string conexion, Guid empresaId, Guid id, CancellationToken cancellationToken)
    {
        await using var sql = new SqlConnection(conexion);
        await sql.OpenAsync(cancellationToken);
        await using var cmd = sql.CreateCommand();
        cmd.CommandText = """
            SELECT [Codigo],[Nombre],[Tipo],[MonedaCodigo],[SaldoInicial],[Activa]
            FROM [tesoreria].[Cuenta] WHERE [Id]=@Id AND [EmpresaId]=@EmpresaId
            """;
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return (null, CodigosProgramaOperativo.NoEncontrado);
        var campos = Campos(
            ("codigo", reader.GetString(0)),
            ("nombre", reader.GetString(1)),
            ("tipo", reader.GetString(2)),
            ("moneda", reader.GetString(3)),
            ("saldo", reader.GetDecimal(4).ToString("G")),
            ("activa", reader.GetBoolean(5) ? "1" : "0"));
        return (new DetalleProgramaOperativo("finanzas/tesoreria-cuentas", id, campos, [], true, []), null);
    }

    private static async Task<(DetalleProgramaOperativo? Detalle, string? Error)> DetalleRecepcionAsync(
        string conexion, Guid empresaId, Guid id, CancellationToken cancellationToken)
    {
        await using var sql = new SqlConnection(conexion);
        await sql.OpenAsync(cancellationToken);
        await using var cmd = sql.CreateCommand();
        cmd.CommandText = """
            SELECT r.[Numero], r.[FechaRecepcion], CAST(r.[OrdenCompraId] AS varchar(36))
            FROM [compras].[RecepcionCompra] r WHERE r.[Id]=@Id AND r.[EmpresaId]=@EmpresaId
            """;
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return (null, CodigosProgramaOperativo.NoEncontrado);
        var campos = Campos(
            ("numero", reader.IsDBNull(0) ? null : reader.GetString(0)),
            ("fecha", DateOnly.FromDateTime(reader.GetDateTime(1)).ToString("yyyy-MM-dd")),
            ("orden", reader.GetString(2)));
        return (new DetalleProgramaOperativo("compras/recepciones", id, campos, [], false, []), null);
    }

    private static async Task<(DetalleProgramaOperativo? Detalle, string? Error)> DetalleMovimientoInventarioAsync(
        string conexion, Guid empresaId, Guid id, CancellationToken cancellationToken)
    {
        await using var sql = new SqlConnection(conexion);
        await sql.OpenAsync(cancellationToken);
        await using var cmd = sql.CreateCommand();
        cmd.CommandText = """
            SELECT m.[Secuencia], m.[Tipo], m.[Fecha], m.[Cantidad], m.[ValorTotal], p.[Nombre], b.[Nombre]
            FROM [inventario].[MovimientoInventario] m
            INNER JOIN [inventario].[Producto] p ON p.[Id]=m.[ProductoId]
            INNER JOIN [inventario].[Bodega] b ON b.[Id]=m.[BodegaId]
            WHERE m.[Id]=@Id AND m.[EmpresaId]=@EmpresaId
            """;
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return (null, CodigosProgramaOperativo.NoEncontrado);
        var campos = Campos(
            ("numero", reader.GetInt32(0).ToString()),
            ("tipo", reader.GetString(1)),
            ("fecha", DateOnly.FromDateTime(reader.GetDateTime(2)).ToString("yyyy-MM-dd")),
            ("cantidad", reader.GetDecimal(3).ToString("G")),
            ("total", reader.GetDecimal(4).ToString("G")),
            ("producto", reader.GetString(5)),
            ("bodega", reader.GetString(6)));
        return (new DetalleProgramaOperativo("inventarios/movimientos", id, campos, [], false, []), null);
    }

    private static async Task<(DetalleProgramaOperativo? Detalle, string? Error)> DetalleGenericoAsync(
        string conexion, Guid empresaId, Guid id, string programa, RegistroProgramasOperativosSql.Definicion definicion, CancellationToken cancellationToken)
    {
        var campos = Campos(("id", id.ToString()), ("programa", programa));
        return (new DetalleProgramaOperativo(programa, id, campos, [], false, []), null);
    }

    private static Dictionary<string, string?> Campos(params (string clave, string? valor)[] pares) =>
        pares.ToDictionary(p => p.clave, p => p.valor, StringComparer.OrdinalIgnoreCase);

    private static string? Valor(Dictionary<string, string?> campos, params string[] claves)
    {
        foreach (var clave in claves)
            if (campos.TryGetValue(clave, out var v) && !string.IsNullOrWhiteSpace(v))
                return v;
        return null;
    }

    private static DateOnly? ParseFecha(Dictionary<string, string?> campos) =>
        Valor(campos, "fecha") is { } t && DateOnly.TryParse(t, out var d) ? d : null;
}
