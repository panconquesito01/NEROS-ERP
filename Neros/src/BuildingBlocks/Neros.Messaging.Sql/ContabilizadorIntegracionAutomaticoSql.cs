using System.Text.Json;
using Microsoft.Data.SqlClient;
using Neros.Domain.Contabilidad;
using Neros.Domain.Globalizacion;

namespace Neros.Messaging.Sql;

internal static class ContabilizadorIntegracionAutomaticoSql
{
    private static readonly JsonSerializerOptions JsonOpciones = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public sealed record LineaJson(string Rol, string Lado, decimal Importe);

    public static async Task<Guid?> IntentarContabilizarAsync(
        SqlConnection conexion,
        SqlTransaction transaccion,
        Guid tenantId,
        Guid empresaId,
        Guid eventoIntegracionId,
        string moduloOrigen,
        string agregadoOrigen,
        string lineasJson,
        CancellationToken cancellationToken)
    {
        await using (var existe = conexion.CreateCommand())
        {
            existe.Transaction = transaccion;
            existe.CommandText = """
                SELECT [ComprobanteId],[Estado] FROM [contabilidad].[SolicitudContabilizacionIntegracion]
                WHERE [EventoIntegracionId]=@EventoId;
                """;
            existe.Parameters.AddWithValue("@EventoId", eventoIntegracionId);
            await using var reader = await existe.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                if (!reader.IsDBNull(0))
                    return reader.GetGuid(0);
            }
            else
                return null;
        }

        var reglas = JsonSerializer.Deserialize<List<LineaJson>>(lineasJson, JsonOpciones);
        if (reglas is null || reglas.Count == 0)
            return null;

        var mapeo = await CargarMapeoRolesAsync(conexion, transaccion, tenantId, empresaId, cancellationToken);
        if (mapeo.Count == 0)
            return null;

        var lineasRegla = new List<MotorContabilizacion.LineaRegla>();
        foreach (var linea in reglas)
        {
            if (!Enum.TryParse<RolContable>(linea.Rol, out var rol))
                return null;
            if (!Enum.TryParse<LadoMovimiento>(linea.Lado, out var lado))
                return null;
            lineasRegla.Add(new MotorContabilizacion.LineaRegla(rol, lado, linea.Importe));
        }

        var movimientos = MotorContabilizacion.ResolverMovimientos(lineasRegla, mapeo);
        var cuentas = await CargarCuentasAsync(conexion, transaccion, movimientos.Select(m => m.CuentaId).Distinct(), cancellationToken);
        var politica = CatalogoIso.PoliticaPorDefecto("COP");
        MotorPartidaDoble.ValidarMovimientos(movimientos, cuentas);

        var periodo = await CargarPeriodoAbiertoAsync(conexion, transaccion, tenantId, empresaId, cancellationToken);
        if (periodo is null)
            return null;
        var tipo = await CargarTipoIntegracionAsync(conexion, transaccion, tenantId, empresaId, cancellationToken);
        if (tipo is null)
            return null;

        var comprobanteId = Guid.NewGuid();
        var fecha = DateOnly.FromDateTime(DateTime.UtcNow);
        var numero = await SiguienteNumeroComprobanteAsync(conexion, transaccion, empresaId, tipo.Value, cancellationToken);
        var borrador = new ComprobanteContable(
            comprobanteId, periodo.Id, fecha, EstadoComprobante.Borrador, movimientos,
            Origen: new OrigenDocumentoContable(moduloOrigen, "Integracion", Guid.TryParse(agregadoOrigen, out var docId) ? docId : Guid.Empty, agregadoOrigen),
            ReglaContabilizacionVersion: 1);
        var contabilizado = MotorPartidaDoble.Contabilizar(borrador, cuentas, periodo, politica);

        await InsertarComprobanteAsync(conexion, transaccion, comprobanteId, tenantId, empresaId, periodo.Id, tipo.Value,
            numero, fecha, contabilizado, moduloOrigen, agregadoOrigen, cancellationToken);
        await InsertarMovimientosAsync(conexion, transaccion, comprobanteId, contabilizado.Movimientos, cancellationToken);

        await using var actualizar = conexion.CreateCommand();
        actualizar.Transaction = transaccion;
        actualizar.CommandText = """
            UPDATE [contabilidad].[SolicitudContabilizacionIntegracion]
            SET [Estado]='Contabilizado',[ComprobanteId]=@ComprobanteId
            WHERE [EventoIntegracionId]=@EventoId;
            """;
        actualizar.Parameters.AddWithValue("@ComprobanteId", comprobanteId);
        actualizar.Parameters.AddWithValue("@EventoId", eventoIntegracionId);
        await actualizar.ExecuteNonQueryAsync(cancellationToken);
        return comprobanteId;
    }

    private static async Task<Dictionary<RolContable, Guid>> CargarMapeoRolesAsync(
        SqlConnection conexion, SqlTransaction transaccion, Guid tenantId, Guid empresaId, CancellationToken cancellationToken)
    {
        var mapa = new Dictionary<RolContable, Guid>();
        await using var cmd = conexion.CreateCommand();
        cmd.Transaction = transaccion;
        cmd.CommandText = """
            SELECT [Rol],[CuentaContableId] FROM [contabilidad].[MapeoRolContable]
            WHERE [TenantId]=@TenantId AND [EmpresaId]=@EmpresaId;
            """;
        cmd.Parameters.AddWithValue("@TenantId", tenantId);
        cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (Enum.TryParse<RolContable>(reader.GetString(0), out var rol))
                mapa[rol] = reader.GetGuid(1);
        }
        return mapa;
    }

    private static async Task<Dictionary<Guid, CuentaContable>> CargarCuentasAsync(
        SqlConnection conexion, SqlTransaction transaccion, IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var cuentas = new Dictionary<Guid, CuentaContable>();
        foreach (var id in ids)
        {
            await using var cmd = conexion.CreateCommand();
            cmd.Transaction = transaccion;
            cmd.CommandText = """
                SELECT [Codigo],[Naturaleza],[Tipo],[AdmiteMovimiento] FROM [contabilidad].[CuentaContable] WHERE [Id]=@Id;
                """;
            cmd.Parameters.AddWithValue("@Id", id);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                throw new InvalidOperationException($"Cuenta {id} no encontrada.");
            var naturaleza = reader.GetString(1) == "D" ? NaturalezaCuenta.Debito : NaturalezaCuenta.Credito;
            var tipo = Enum.Parse<TipoCuenta>(reader.GetString(2));
            cuentas[id] = new CuentaContable(id, reader.GetString(0), naturaleza, tipo, reader.GetBoolean(3));
        }
        return cuentas;
    }

    private static async Task<PeriodoContable?> CargarPeriodoAbiertoAsync(
        SqlConnection conexion, SqlTransaction transaccion, Guid tenantId, Guid empresaId, CancellationToken cancellationToken)
    {
        await using var cmd = conexion.CreateCommand();
        cmd.Transaction = transaccion;
        cmd.CommandText = """
            SELECT TOP (1) p.[Id], p.[FechaInicio], p.[FechaFin], p.[Estado]
            FROM [contabilidad].[Periodo] p
            INNER JOIN [contabilidad].[Ejercicio] e ON e.[Id]=p.[EjercicioId]
            WHERE e.[TenantId]=@TenantId AND e.[EmpresaId]=@EmpresaId AND p.[Estado]='Abierto'
            ORDER BY p.[FechaInicio];
            """;
        cmd.Parameters.AddWithValue("@TenantId", tenantId);
        cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return new PeriodoContable(
            reader.GetGuid(0),
            DateOnly.FromDateTime(reader.GetDateTime(1)),
            DateOnly.FromDateTime(reader.GetDateTime(2)),
            Enum.Parse<EstadoPeriodoContable>(reader.GetString(3)));
    }

    private static async Task<Guid?> CargarTipoIntegracionAsync(
        SqlConnection conexion, SqlTransaction transaccion, Guid tenantId, Guid empresaId, CancellationToken cancellationToken)
    {
        await using var cmd = conexion.CreateCommand();
        cmd.Transaction = transaccion;
        cmd.CommandText = """
            SELECT TOP (1) [Id] FROM [contabilidad].[TipoComprobante]
            WHERE [TenantId]=@TenantId AND [EmpresaId]=@EmpresaId AND [Codigo]='INT' AND [Activo]=1;
            """;
        cmd.Parameters.AddWithValue("@TenantId", tenantId);
        cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
        var valor = await cmd.ExecuteScalarAsync(cancellationToken);
        return valor is Guid g ? g : null;
    }

    private static async Task<int> SiguienteNumeroComprobanteAsync(
        SqlConnection conexion, SqlTransaction transaccion, Guid empresaId, Guid tipoId, CancellationToken cancellationToken)
    {
        await using var cmd = conexion.CreateCommand();
        cmd.Transaction = transaccion;
        cmd.CommandText = """
            SELECT ISNULL(MAX([Numero]),0)+1 FROM [contabilidad].[Comprobante] WITH (UPDLOCK, HOLDLOCK)
            WHERE [EmpresaId]=@EmpresaId AND [TipoComprobanteId]=@Tipo;
            """;
        cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
        cmd.Parameters.AddWithValue("@Tipo", tipoId);
        return (int)(await cmd.ExecuteScalarAsync(cancellationToken))!;
    }

    private static async Task InsertarComprobanteAsync(
        SqlConnection conexion, SqlTransaction transaccion, Guid id, Guid tenantId, Guid empresaId, Guid periodoId, Guid tipoId,
        int numero, DateOnly fecha, ComprobanteContable comprobante, string moduloOrigen, string agregadoOrigen,
        CancellationToken cancellationToken)
    {
        await using var cmd = conexion.CreateCommand();
        cmd.Transaction = transaccion;
        cmd.CommandText = """
            INSERT INTO [contabilidad].[Comprobante]
                ([Id],[TenantId],[EmpresaId],[PeriodoId],[TipoComprobanteId],[Numero],[Fecha],[Estado],
                 [ModuloOrigen],[TipoDocumentoOrigen],[DocumentoOrigenId],[NumeroDocumentoOrigen],[ReglaContabilizacionVersion])
            VALUES (@Id,@TenantId,@EmpresaId,@PeriodoId,@TipoId,@Numero,@Fecha,'Contabilizado',
                 @Modulo,@TipoDoc,@DocId,@NumDoc,@Regla);
            """;
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@TenantId", tenantId);
        cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
        cmd.Parameters.AddWithValue("@PeriodoId", periodoId);
        cmd.Parameters.AddWithValue("@TipoId", tipoId);
        cmd.Parameters.AddWithValue("@Numero", numero);
        cmd.Parameters.AddWithValue("@Fecha", fecha);
        cmd.Parameters.AddWithValue("@Modulo", moduloOrigen);
        cmd.Parameters.AddWithValue("@TipoDoc", "Integracion");
        cmd.Parameters.AddWithValue("@DocId", comprobante.Origen?.DocumentoOrigenId ?? Guid.Empty);
        cmd.Parameters.AddWithValue("@NumDoc", agregadoOrigen);
        cmd.Parameters.AddWithValue("@Regla", comprobante.ReglaContabilizacionVersion ?? 1);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertarMovimientosAsync(
        SqlConnection conexion, SqlTransaction transaccion, Guid comprobanteId, IReadOnlyList<MovimientoLinea> movimientos,
        CancellationToken cancellationToken)
    {
        var linea = 0;
        foreach (var mov in movimientos)
        {
            linea++;
            await using var cmd = conexion.CreateCommand();
            cmd.Transaction = transaccion;
            cmd.CommandText = """
                INSERT INTO [contabilidad].[MovimientoContable]
                    ([ComprobanteId],[Linea],[CuentaContableId],[Debito],[Credito],
                     [ImporteMonedaDocumento],[MonedaDocumento],[TasaCambio],[ImporteMonedaFuncional],[TerceroId])
                VALUES (@ComprobanteId,@Linea,@Cuenta,@Debito,@Credito,@Importe,'COP',1,@Importe,@Tercero);
                """;
            cmd.Parameters.AddWithValue("@ComprobanteId", comprobanteId);
            cmd.Parameters.AddWithValue("@Linea", linea);
            cmd.Parameters.AddWithValue("@Cuenta", mov.CuentaId);
            var debito = mov.Lado == LadoMovimiento.Debito ? mov.ImporteMonedaFuncional : 0m;
            var credito = mov.Lado == LadoMovimiento.Credito ? mov.ImporteMonedaFuncional : 0m;
            cmd.Parameters.AddWithValue("@Debito", debito);
            cmd.Parameters.AddWithValue("@Credito", credito);
            cmd.Parameters.AddWithValue("@Importe", mov.ImporteMonedaFuncional);
            cmd.Parameters.AddWithValue("@Tercero", (object?)mov.TerceroId ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
