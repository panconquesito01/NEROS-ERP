using Microsoft.EntityFrameworkCore;
using Neros.Contracts.Terceros;
using Neros.Domain.Terceros;

namespace Neros.Terceros.Persistence;

public sealed class ServicioTerceros(TercerosDbContext context)
{
    public async Task<PaginaTerceros> BuscarAsync(Guid tenantId, string? texto, int pagina, int tamano, CancellationToken cancellationToken)
    {
        pagina = Math.Max(1, pagina);
        tamano = Math.Clamp(tamano, 1, 100);
        var consulta = context.Terceros.AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .Include(t => t.Identificaciones)
            .Include(t => t.Roles)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(texto))
        {
            var filtro = $"%{texto.Trim()}%";
            consulta = consulta.Where(t => EF.Functions.Like(t.RazonSocial, filtro)
                || t.Identificaciones.Any(i => EF.Functions.Like(i.Numero, filtro)));
        }
        var total = await consulta.CountAsync(cancellationToken);
        var elementos = await consulta.OrderBy(t => t.RazonSocial).Skip((pagina - 1) * tamano).Take(tamano)
            .Select(t => new TerceroResumen(t.Id, t.Tipo, t.RazonSocial,
                t.Identificaciones.Where(i => i.EsPrincipal).Select(i => i.Tipo + " " + i.Numero).FirstOrDefault()
                ?? t.Identificaciones.Select(i => i.Tipo + " " + i.Numero).FirstOrDefault(),
                t.Roles.Select(r => r.Rol).ToList(), t.Activo))
            .ToListAsync(cancellationToken);
        return new PaginaTerceros(elementos, total, pagina, tamano);
    }

    public async Task<TerceroDetalle?> ObtenerAsync(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        var tercero = await context.Terceros.AsNoTracking()
            .Include(t => t.Identificaciones)
            .Include(t => t.Roles)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id, cancellationToken);
        return tercero is null ? null : MapearDetalle(tercero);
    }

    public async Task<(TerceroDetalle? Detalle, ErrorValidacionTercero? Error)> CrearAsync(
        Guid tenantId, SolicitudCrearTercero solicitud, CancellationToken cancellationToken)
    {
        if (await ValidarIdentificacionesAsync(tenantId, null, solicitud.Identificaciones, cancellationToken) is { } errorValidacion)
            return (null, errorValidacion);
        var id = Guid.NewGuid();
        var tercero = new TerceroEntidad
        {
            Id = id,
            TenantId = tenantId,
            Tipo = solicitud.Tipo,
            RazonSocial = solicitud.RazonSocial.Trim(),
            NombreComercial = string.IsNullOrWhiteSpace(solicitud.NombreComercial) ? null : solicitud.NombreComercial.Trim(),
            Activo = true
        };
        AplicarIdentificaciones(tercero, tenantId, solicitud.Identificaciones);
        AplicarRoles(tercero, solicitud.Roles);
        context.Terceros.Add(tercero);
        await context.SaveChangesAsync(cancellationToken);
        return (MapearDetalle(tercero), null);
    }

    public async Task<(TerceroDetalle? Detalle, ErrorValidacionTercero? Error)> ActualizarAsync(
        Guid tenantId, Guid id, SolicitudActualizarTercero solicitud, CancellationToken cancellationToken)
    {
        if (!await context.Terceros.AnyAsync(t => t.TenantId == tenantId && t.Id == id, cancellationToken))
            return (null, null);
        if (await ValidarIdentificacionesAsync(tenantId, id, solicitud.Identificaciones, cancellationToken) is { } errorValidacion)
            return (null, errorValidacion);
        context.ChangeTracker.Clear();
        await context.Identificaciones.Where(i => i.TerceroId == id).ExecuteDeleteAsync(cancellationToken);
        await context.Roles.Where(r => r.TerceroId == id).ExecuteDeleteAsync(cancellationToken);
        var razonSocial = solicitud.RazonSocial.Trim();
        var nombreComercial = string.IsNullOrWhiteSpace(solicitud.NombreComercial) ? null : solicitud.NombreComercial.Trim();
        await context.Terceros.Where(t => t.TenantId == tenantId && t.Id == id).ExecuteUpdateAsync(setters => setters
            .SetProperty(t => t.RazonSocial, razonSocial)
            .SetProperty(t => t.NombreComercial, nombreComercial)
            .SetProperty(t => t.Activo, solicitud.Activo), cancellationToken);
        var identificaciones = solicitud.Identificaciones.Select(identificacion => new IdentificacionEntidad
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TerceroId = id,
            Pais = identificacion.Pais.ToUpperInvariant(),
            Tipo = identificacion.Tipo.ToUpperInvariant(),
            Numero = identificacion.Numero.Trim(),
            DigitoVerificacion = identificacion.DigitoVerificacion?.ToString(),
            EsPrincipal = identificacion.EsPrincipal
        }).ToList();
        var roles = solicitud.Roles.Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(rol => new RolEntidad { TerceroId = id, Rol = rol }).ToList();
        context.Identificaciones.AddRange(identificaciones);
        context.Roles.AddRange(roles);
        await context.SaveChangesAsync(cancellationToken);
        return (await ObtenerAsync(tenantId, id, cancellationToken), null);
    }

    public async Task<IReadOnlyList<VersionHistoricaTercero>> HistorialAsync(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        var conexion = context.Database.GetDbConnection();
        await conexion.OpenAsync(cancellationToken);
        await using var comando = conexion.CreateCommand();
        comando.CommandText = """
            SELECT [ValidFrom], [ValidTo], [RazonSocial], [Activo]
            FROM [terceros].[Tercero] FOR SYSTEM_TIME ALL
            WHERE [TenantId] = @TenantId AND [Id] = @Id
            ORDER BY [ValidFrom]
            """;
        var tenantParam = comando.CreateParameter();
        tenantParam.ParameterName = "@TenantId";
        tenantParam.Value = tenantId;
        comando.Parameters.Add(tenantParam);
        var idParam = comando.CreateParameter();
        idParam.ParameterName = "@Id";
        idParam.Value = id;
        comando.Parameters.Add(idParam);
        var versiones = new List<VersionHistoricaTercero>();
        await using var lector = await comando.ExecuteReaderAsync(cancellationToken);
        while (await lector.ReadAsync(cancellationToken))
        {
            versiones.Add(new VersionHistoricaTercero(
                new DateTimeOffset(DateTime.SpecifyKind(lector.GetDateTime(0), DateTimeKind.Utc)),
                new DateTimeOffset(DateTime.SpecifyKind(lector.GetDateTime(1), DateTimeKind.Utc)),
                lector.GetString(2),
                lector.GetBoolean(3)));
        }
        return versiones;
    }

    private async Task<ErrorValidacionTercero?> ValidarIdentificacionesAsync(
        Guid tenantId, Guid? terceroId, IReadOnlyList<IdentificacionTercero> identificaciones, CancellationToken cancellationToken)
    {
        if (identificaciones.Count == 0)
            return new ErrorValidacionTercero("identificacion_requerida", "Al menos una identificacion es obligatoria.");
        foreach (var identificacion in identificaciones)
        {
            if (identificacion.Pais.Equals("CO", StringComparison.OrdinalIgnoreCase)
                && !ValidadorIdentificacionColombia.EsValida(identificacion.Tipo, identificacion.Numero, identificacion.DigitoVerificacion))
                return new ErrorValidacionTercero("identificacion_invalida", $"Identificacion {identificacion.Tipo} invalida para Colombia.");
            var duplicada = await context.Identificaciones.AnyAsync(i =>
                i.TenantId == tenantId
                && i.Pais == identificacion.Pais.ToUpperInvariant()
                && i.Tipo == identificacion.Tipo.ToUpperInvariant()
                && i.Numero == identificacion.Numero.Trim()
                && (terceroId == null || i.TerceroId != terceroId), cancellationToken);
            if (duplicada)
                return new ErrorValidacionTercero("identificacion_duplicada", "Ya existe un tercero con esa identificacion.");
        }
        return null;
    }

    private static void AplicarIdentificaciones(TerceroEntidad tercero, Guid tenantId, IReadOnlyList<IdentificacionTercero> identificaciones)
    {
        foreach (var identificacion in identificaciones)
        {
            tercero.Identificaciones.Add(new IdentificacionEntidad
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TerceroId = tercero.Id,
                Pais = identificacion.Pais.ToUpperInvariant(),
                Tipo = identificacion.Tipo.ToUpperInvariant(),
                Numero = identificacion.Numero.Trim(),
                DigitoVerificacion = identificacion.DigitoVerificacion?.ToString(),
                EsPrincipal = identificacion.EsPrincipal
            });
        }
    }

    private static void AplicarRoles(TerceroEntidad tercero, IReadOnlyList<string> roles)
    {
        foreach (var rol in roles.Distinct(StringComparer.OrdinalIgnoreCase))
            tercero.Roles.Add(new RolEntidad { TerceroId = tercero.Id, Rol = rol });
    }

    private static TerceroDetalle MapearDetalle(TerceroEntidad tercero) => new(
        tercero.Id, tercero.Tipo, tercero.RazonSocial, tercero.NombreComercial, tercero.Activo,
        tercero.Identificaciones.Select(i => new IdentificacionTercero(
            i.Pais, i.Tipo, i.Numero, i.DigitoVerificacion is { Length: 1 } dv ? dv[0] : null, i.EsPrincipal)).ToList(),
        tercero.Roles.Select(r => r.Rol).ToList());
}
