namespace Neros.Persistence.Operacion;

internal static class RegistroProgramasOperativosSql
{
    internal sealed record Definicion(
        string Conexion,
        string SqlListado,
        string? SqlBorrador,
        bool SoportaLineas,
        bool SoportaConfirmar);

    internal static bool TryGet(string programa, out Definicion definicion) =>
        Definiciones.TryGetValue(programa, out definicion!);

    internal static IEnumerable<string> Programas => Definiciones.Keys;

    private static readonly Dictionary<string, Definicion> Definiciones = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ventas/cotizaciones"] = Doc("Ventas", "ventas.Cotizacion", "ClienteRazonSocial", true),
        ["ventas/pedidos"] = Doc("Ventas", "ventas.Pedido", "ClienteRazonSocial", true),
        ["ventas/entregas"] = Filtro("Ventas", "ventas.Pedido", "ClienteRazonSocial", "Estado IN ('Confirmado','Borrador')"),
        ["ventas/devoluciones"] = Filtro("Ventas", "ventas.Pedido", "ClienteRazonSocial", "Estado = 'Anulado'"),
        ["ventas/facturacion"] = DocCustom("Facturacion", """
            SELECT d.[Id], d.[TipoDocumento], d.[Estado], d.[FechaDocumento], d.[ClienteRazonSocial], d.[Total]
            FROM [facturacion].[DocumentoComercial] d WHERE d.[EmpresaId]=@EmpresaId
            ORDER BY d.[FechaDocumento] DESC OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY;
            SELECT COUNT(1) FROM [facturacion].[DocumentoComercial] WHERE [EmpresaId]=@EmpresaId;
            """, """
            INSERT INTO [facturacion].[DocumentoComercial]
            ([Id],[TenantId],[EmpresaId],[TipoDocumento],[Estado],[FechaDocumento],[MonedaCodigo],[ClienteTerceroId],[ClienteRazonSocial])
            VALUES (@Id,@TenantId,@EmpresaId,'FacturaVenta','Borrador',CAST(SYSUTCDATETIME() AS date),'COP',@TerceroId,N'Cliente borrador');
            """, true, true),
        ["ventas/clientes"] = Terceros("Cliente"),
        ["ventas/listas-precio"] = Maestro("Inventario", "inventario.Producto", "Codigo", "Nombre", true),

        ["inventarios/productos"] = Maestro("Inventario", "inventario.Producto", "Codigo", "Nombre", true),
        ["inventarios/bodegas"] = MaestroBodega(),
        ["inventarios/movimientos"] = Movimientos(),
        ["inventarios/existencias"] = Existencias(),
        ["inventarios/conteos"] = Periodos("Abierto"),
        ["inventarios/periodos"] = Periodos(null),

        ["compras/solicitudes"] = Filtro("Compras", "compras.OrdenCompra", "ProveedorRazonSocial", "Estado = 'Borrador'"),
        ["compras/ordenes"] = Doc("Compras", "compras.OrdenCompra", "ProveedorRazonSocial", true),
        ["compras/recepciones"] = Recepciones(),
        ["compras/devoluciones"] = Filtro("Compras", "compras.OrdenCompra", "ProveedorRazonSocial", "Estado = 'Anulado'"),
        ["compras/proveedores"] = Terceros("Proveedor"),
        ["compras/aprobaciones"] = Filtro("Compras", "compras.OrdenCompra", "ProveedorRazonSocial", "Estado = 'Borrador'"),

        ["finanzas/cuentas-cobrar"] = Cartera("CxC", "Cartera"),
        ["finanzas/cuentas-pagar"] = Cartera("CxP", "Obligación"),
        ["finanzas/pagos"] = Pagos(),
        ["finanzas/tesoreria-cuentas"] = TesoreriaCuentas(),
        ["finanzas/tesoreria-movimientos"] = TesoreriaMovimientos(),
        ["finanzas/conciliacion"] = Conciliacion(),
        ["finanzas/comprobantes"] = Comprobantes(),
        ["finanzas/reportes-contables"] = Reportes()
    };

    internal static bool EsInforme(string programa) =>
        programa.StartsWith("informes/", StringComparison.OrdinalIgnoreCase);

    private static Definicion Doc(string conexion, string tabla, string refCol, bool lineas) => new(conexion, $"""
        SELECT t.[Id], t.[Numero], t.[Estado], t.[FechaDocumento], t.[{refCol}], t.[Total]
        FROM [{Schema(tabla)}].[{Table(tabla)}] t WHERE t.[EmpresaId]=@EmpresaId
        ORDER BY t.[FechaDocumento] DESC OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY;
        SELECT COUNT(1) FROM [{Schema(tabla)}].[{Table(tabla)}] WHERE [EmpresaId]=@EmpresaId;
        """, BorradorDoc(tabla), lineas, true);

    private static Definicion Filtro(string conexion, string tabla, string refCol, string filtro) => new(conexion, $"""
        SELECT t.[Id], t.[Numero], t.[Estado], t.[FechaDocumento], t.[{refCol}], t.[Total]
        FROM [{Schema(tabla)}].[{Table(tabla)}] t WHERE t.[EmpresaId]=@EmpresaId AND {filtro}
        ORDER BY t.[FechaDocumento] DESC OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY;
        SELECT COUNT(1) FROM [{Schema(tabla)}].[{Table(tabla)}] WHERE [EmpresaId]=@EmpresaId AND {filtro};
        """, null, false, false);

    private static Definicion DocCustom(string conexion, string listado, string borrador, bool lineas, bool confirmar) =>
        new(conexion, listado, borrador, lineas, confirmar);

    private static Definicion Maestro(string conexion, string tabla, string codigo, string nombre, bool borrador) => new(conexion, $"""
        SELECT t.[Id], t.[{codigo}], CASE WHEN t.[Activo]=1 THEN 'Activo' ELSE 'Inactivo' END, NULL, t.[{nombre}], NULL
        FROM [{Schema(tabla)}].[{Table(tabla)}] t WHERE t.[EmpresaId]=@EmpresaId
        ORDER BY t.[{codigo}] OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY;
        SELECT COUNT(1) FROM [{Schema(tabla)}].[{Table(tabla)}] WHERE [EmpresaId]=@EmpresaId;
        """, borrador ? BorradorMaestro(tabla, codigo, nombre) : null, false, false);

    private static Definicion Movimientos() => new("Inventario", """
        SELECT m.[Id], CAST(m.[Secuencia] AS varchar(20)), m.[Tipo], m.[Fecha], m.[ModuloOrigen], m.[ValorTotal]
        FROM [inventario].[MovimientoInventario] m WHERE m.[EmpresaId]=@EmpresaId
        ORDER BY m.[Fecha] DESC, m.[Secuencia] DESC OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY;
        SELECT COUNT(1) FROM [inventario].[MovimientoInventario] WHERE [EmpresaId]=@EmpresaId;
        """, null, false, false);

    private static Definicion Existencias() => new("Inventario", """
        SELECT e.[ProductoId], p.[Codigo], 'Stock', NULL, p.[Nombre], e.[Cantidad]
        FROM [inventario].[Existencia] e
        INNER JOIN [inventario].[Producto] p ON p.[Id]=e.[ProductoId]
        WHERE p.[EmpresaId]=@EmpresaId
        ORDER BY p.[Codigo] OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY;
        SELECT COUNT(1) FROM [inventario].[Existencia] e INNER JOIN [inventario].[Producto] p ON p.[Id]=e.[ProductoId] WHERE p.[EmpresaId]=@EmpresaId;
        """, null, false, false);

    private static Definicion Periodos(string? estado) => new("Inventario", estado is null ? """
        SELECT p.[Id], p.[Estado], p.[Estado], p.[FechaInicio], CONCAT(FORMAT(p.[FechaInicio],'d'),' - ',FORMAT(p.[FechaFin],'d')), NULL
        FROM [inventario].[PeriodoInventario] p WHERE p.[EmpresaId]=@EmpresaId
        ORDER BY p.[FechaInicio] DESC OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY;
        SELECT COUNT(1) FROM [inventario].[PeriodoInventario] WHERE [EmpresaId]=@EmpresaId;
        """ : """
        SELECT p.[Id], p.[Estado], p.[Estado], p.[FechaInicio], CONCAT(FORMAT(p.[FechaInicio],'d'),' - ',FORMAT(p.[FechaFin],'d')), NULL
        FROM [inventario].[PeriodoInventario] p WHERE p.[EmpresaId]=@EmpresaId AND p.[Estado]=@EstadoFiltro
        ORDER BY p.[FechaInicio] DESC OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY;
        SELECT COUNT(1) FROM [inventario].[PeriodoInventario] WHERE [EmpresaId]=@EmpresaId AND [Estado]=@EstadoFiltro;
        """, """
        INSERT INTO [inventario].[PeriodoInventario] ([Id],[TenantId],[EmpresaId],[FechaInicio],[FechaFin],[Estado])
        VALUES (@Id,@TenantId,@EmpresaId,CAST(SYSUTCDATETIME() AS date),DATEADD(day,30,CAST(SYSUTCDATETIME() AS date)),'Abierto');
        """, false, false);

    private static Definicion Recepciones() => new("Compras", """
        SELECT r.[Id], r.[Numero], 'Registrada', r.[FechaRecepcion], CAST(r.[OrdenCompraId] AS varchar(36)), NULL
        FROM [compras].[RecepcionCompra] r WHERE r.[EmpresaId]=@EmpresaId
        ORDER BY r.[FechaRecepcion] DESC OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY;
        SELECT COUNT(1) FROM [compras].[RecepcionCompra] WHERE [EmpresaId]=@EmpresaId;
        """, null, false, false);

    private static Definicion Terceros(string rol) => new("Terceros", $"""
        SELECT t.[Id], i.[Numero], CASE WHEN t.[Activo]=1 THEN 'Activo' ELSE 'Inactivo' END, NULL, t.[RazonSocial], NULL
        FROM [terceros].[Tercero] t
        INNER JOIN [terceros].[Rol] r ON r.[TerceroId]=t.[Id] AND r.[Rol]='{rol}'
        LEFT JOIN [terceros].[Identificacion] i ON i.[TerceroId]=t.[Id] AND i.[EsPrincipal]=1
        WHERE t.[TenantId]=@EmpresaId
        ORDER BY t.[RazonSocial] OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY;
        SELECT COUNT(1) FROM [terceros].[Tercero] t INNER JOIN [terceros].[Rol] r ON r.[TerceroId]=t.[Id] AND r.[Rol]='{rol}' WHERE t.[TenantId]=@EmpresaId;
        """, BorradorTercero(rol), false, false);

    private static Definicion MaestroBodega() => new("Inventario", """
        SELECT t.[Id], t.[Codigo], CASE WHEN t.[Activa]=1 THEN 'Activa' ELSE 'Inactiva' END, NULL, t.[Nombre], NULL
        FROM [inventario].[Bodega] t WHERE t.[EmpresaId]=@EmpresaId
        ORDER BY t.[Codigo] OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY;
        SELECT COUNT(1) FROM [inventario].[Bodega] WHERE [EmpresaId]=@EmpresaId;
        """, """
        INSERT INTO [inventario].[Bodega] ([Id],[TenantId],[EmpresaId],[Codigo],[Nombre])
        VALUES (@Id,@TenantId,@EmpresaId,CONCAT('B-',FORMAT(@Id,'N')[1,8]),N'Bodega borrador');
        """, false, false);

    private static Definicion Cartera(string tipo, string etiquetaEstado) => new("Cartera", $"""
        SELECT d.[Id], d.[Numero], '{etiquetaEstado}', d.[FechaDocumento], CAST(d.[TerceroId] AS varchar(36)), d.[TotalDocumento]
        FROM [cartera].[Documento] d WHERE d.[EmpresaId]=@EmpresaId AND d.[TipoCartera]='{tipo}'
        ORDER BY d.[FechaDocumento] DESC OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY;
        SELECT COUNT(1) FROM [cartera].[Documento] WHERE [EmpresaId]=@EmpresaId AND [TipoCartera]='{tipo}';
        """, BorradorCartera(tipo), false, true);

    private static Definicion Pagos() => new("Cartera", """
        SELECT p.[Id], p.[Numero], p.[TipoCartera], p.[FechaPago], CAST(p.[TerceroId] AS varchar(36)), p.[ImportePago]
        FROM [cartera].[Pago] p WHERE p.[EmpresaId]=@EmpresaId
        ORDER BY p.[FechaPago] DESC OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY;
        SELECT COUNT(1) FROM [cartera].[Pago] WHERE [EmpresaId]=@EmpresaId;
        """, BorradorPago(), false, false);

    private static Definicion TesoreriaCuentas() => new("Tesoreria", """
        SELECT c.[Id], c.[Codigo], c.[Tipo], NULL, c.[Nombre], c.[SaldoInicial]
        FROM [tesoreria].[Cuenta] c WHERE c.[EmpresaId]=@EmpresaId
        ORDER BY c.[Codigo] OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY;
        SELECT COUNT(1) FROM [tesoreria].[Cuenta] WHERE [EmpresaId]=@EmpresaId;
        """, """
        INSERT INTO [tesoreria].[Cuenta] ([Id],[TenantId],[EmpresaId],[Codigo],[Nombre],[Tipo],[MonedaCodigo])
        VALUES (@Id,@TenantId,@EmpresaId,CONCAT('C-',FORMAT(@Id,'N')[1,8]),N'Cuenta borrador','Banco','COP');
        """, false, false);

    private static Definicion TesoreriaMovimientos() => new("Tesoreria", """
        SELECT m.[Id], ISNULL(m.[Referencia], CAST(m.[Id] AS varchar(36))), m.[Tipo], m.[FechaMovimiento], CAST(m.[CuentaId] AS varchar(36)), m.[Importe]
        FROM [tesoreria].[Movimiento] m WHERE m.[EmpresaId]=@EmpresaId
        ORDER BY m.[FechaMovimiento] DESC OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY;
        SELECT COUNT(1) FROM [tesoreria].[Movimiento] WHERE [EmpresaId]=@EmpresaId;
        """, null, false, false);

    private static Definicion Conciliacion() => new("Tesoreria", """
        SELECT c.[Id], CAST(c.[Id] AS varchar(36)), c.[Estado], CAST(c.[CerradaEnUtc] AS date), CAST(c.[CuentaId] AS varchar(36)), c.[Diferencia]
        FROM [tesoreria].[Conciliacion] c WHERE c.[EmpresaId]=@EmpresaId
        ORDER BY c.[CerradaEnUtc] DESC OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY;
        SELECT COUNT(1) FROM [tesoreria].[Conciliacion] WHERE [EmpresaId]=@EmpresaId;
        """, null, false, false);

    private static Definicion Comprobantes() => new("Contabilidad", """
        SELECT c.[Id], CAST(c.[Numero] AS varchar(20)), c.[Estado], c.[Fecha], ISNULL(c.[ModuloOrigen], N'Manual'), NULL
        FROM [contabilidad].[Comprobante] c WHERE c.[EmpresaId]=@EmpresaId
        ORDER BY c.[Fecha] DESC OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY;
        SELECT COUNT(1) FROM [contabilidad].[Comprobante] WHERE [EmpresaId]=@EmpresaId;
        """, BorradorComprobante(), false, true);

    private static Definicion Reportes() => new("Contabilidad", """
        SELECT c.[Id], c.[Codigo], CASE WHEN c.[Activa]=1 THEN 'Activa' ELSE 'Inactiva' END, NULL, c.[Nombre], NULL
        FROM [contabilidad].[CuentaContable] c WHERE c.[EmpresaId]=@EmpresaId
        ORDER BY c.[Codigo] OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY;
        SELECT COUNT(1) FROM [contabilidad].[CuentaContable] WHERE [EmpresaId]=@EmpresaId;
        """, null, false, false);

    private static string BorradorComprobante() => """
        INSERT INTO [contabilidad].[Comprobante]
            ([Id],[TenantId],[EmpresaId],[PeriodoId],[TipoComprobanteId],[Numero],[Fecha],[Estado],[ModuloOrigen])
        SELECT TOP (1) @Id, @TenantId, @EmpresaId, p.[Id], t.[Id],
            (SELECT ISNULL(MAX(c.[Numero]), 0) + 1 FROM [contabilidad].[Comprobante] c WHERE c.[EmpresaId]=@EmpresaId),
            CAST(SYSUTCDATETIME() AS date), 'Borrador', 'Manual'
        FROM [contabilidad].[Periodo] p
        INNER JOIN [contabilidad].[Ejercicio] e ON e.[Id]=p.[EjercicioId]
        CROSS APPLY (SELECT TOP (1) tc.[Id] FROM [contabilidad].[TipoComprobante] tc WHERE tc.[EmpresaId]=@EmpresaId ORDER BY tc.[Codigo]) t
        WHERE e.[EmpresaId]=@EmpresaId AND p.[Estado]='Abierto'
        ORDER BY p.[FechaInicio] DESC;
        """;

    private static string BorradorDoc(string tabla) => tabla switch
    {
        "ventas.Cotizacion" => """
            INSERT INTO [ventas].[Cotizacion]
            ([Id],[TenantId],[EmpresaId],[Estado],[FechaDocumento],[MonedaCodigo],[ClienteTerceroId],[ClienteRazonSocial])
            VALUES (@Id,@TenantId,@EmpresaId,'Borrador',CAST(SYSUTCDATETIME() AS date),'COP',@TerceroId,N'Borrador sin cliente');
            """,
        "ventas.Pedido" => """
            INSERT INTO [ventas].[Pedido]
            ([Id],[TenantId],[EmpresaId],[Estado],[FechaDocumento],[MonedaCodigo],[ClienteTerceroId],[ClienteRazonSocial])
            VALUES (@Id,@TenantId,@EmpresaId,'Borrador',CAST(SYSUTCDATETIME() AS date),'COP',@TerceroId,N'Borrador sin cliente');
            """,
        "compras.OrdenCompra" => """
            INSERT INTO [compras].[OrdenCompra]
            ([Id],[TenantId],[EmpresaId],[Estado],[FechaDocumento],[MonedaCodigo],[ProveedorTerceroId],[ProveedorRazonSocial])
            VALUES (@Id,@TenantId,@EmpresaId,'Borrador',CAST(SYSUTCDATETIME() AS date),'COP',@TerceroId,N'Proveedor borrador');
            """,
        _ => throw new InvalidOperationException($"Sin borrador para {tabla}")
    };

    private static string BorradorMaestro(string tabla, string codigo, string nombre)
    {
        var schema = Schema(tabla);
        var table = Table(tabla);
        return $"""
            INSERT INTO [{schema}].[{table}] ([Id],[TenantId],[EmpresaId],[{codigo}],[{nombre}])
            VALUES (@Id,@TenantId,@EmpresaId,CONCAT('X-',FORMAT(@Id,'N')[1,8]),N'Borrador');
            """;
    }

    private static string BorradorTercero(string rol) => $"""
        INSERT INTO [terceros].[Tercero] ([Id],[TenantId],[Tipo],[RazonSocial],[Activo])
        VALUES (@Id,@TenantId,'Organizacion',N'Tercero borrador',1);
        INSERT INTO [terceros].[Rol] ([TerceroId],[Rol]) VALUES (@Id,'{rol}');
        INSERT INTO [terceros].[Identificacion] ([TenantId],[TerceroId],[Pais],[Tipo],[Numero],[EsPrincipal])
        VALUES (@TenantId,@Id,'CO','NIT',CONCAT('9',FORMAT(@Id,'N')[1,9]),1);
        """;

    private static string BorradorCartera(string tipo) => $"""
        INSERT INTO [cartera].[Documento]
        ([Id],[TenantId],[EmpresaId],[TipoCartera],[TerceroId],[Numero],[FechaDocumento],[MonedaCodigo],[TotalDocumento])
        VALUES (@Id,@TenantId,@EmpresaId,'{tipo}',@TerceroId,CONCAT('DOC-',FORMAT(@Id,'N')[1,8]),CAST(SYSUTCDATETIME() AS date),'COP',0);
        """;

    private static string BorradorPago() => """
        INSERT INTO [cartera].[Pago]
        ([Id],[TenantId],[EmpresaId],[TipoCartera],[TerceroId],[Numero],[FechaPago],[MonedaCodigo],[ImportePago])
        VALUES (@Id,@TenantId,@EmpresaId,'CxC',@TerceroId,CONCAT('PAG-',FORMAT(@Id,'N')[1,8]),CAST(SYSUTCDATETIME() AS date),'COP',1);
        """;

    private static string Schema(string tabla) => tabla.Split('.')[0];
    private static string Table(string tabla) => tabla.Split('.')[1];
}
