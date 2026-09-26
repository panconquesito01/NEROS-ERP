using Microsoft.EntityFrameworkCore;
using Neros.Application.Privacidad;
using Neros.Application.Seguridad;
using Neros.Contracts.Privacidad;

namespace Neros.Persistence.Privacidad;

public sealed class ServicioPrivacidad(NerosDbContext database, TimeProvider reloj) : IServicioPrivacidad
{
    public async Task<IReadOnlyList<DefinicionCookiePublica>> ListarCookiesAsync(CancellationToken cancellationToken) =>
        await database.DefinicionesCookie.AsNoTracking().Where(def => def.Activo)
            .OrderBy(def => def.Orden).ThenBy(def => def.Nombre)
            .Select(def => new DefinicionCookiePublica(def.Nombre, def.Almacenamiento, def.Proveedor, def.Categoria, def.Finalidad,
                def.Duracion, def.PrimeraParte, def.Dominio, def.Esencial, def.UrlPolitica))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DocumentoLegalPendiente>> ListarPendientesAsync(string usuarioId, CancellationToken cancellationToken)
    {
        var ahora = reloj.GetUtcNow().UtcDateTime;
        var vigentes = await VersionesVigentesQuery(ahora).ToListAsync(cancellationToken);
        if (vigentes.Count == 0) return [];
        var aceptadas = await database.AceptacionesLegales.AsNoTracking()
            .Where(a => a.UsuarioId == usuarioId && vigentes.Select(v => v.VersionId).Contains(a.VersionId))
            .Select(a => a.VersionId)
            .ToListAsync(cancellationToken);
        return vigentes.Where(v => !aceptadas.Contains(v.VersionId))
            .Select(v => new DocumentoLegalPendiente(v.Codigo, v.Nombre, v.Version, v.VersionId, v.HashContenido.Trim()))
            .ToList();
    }

    public async Task<DocumentoLegalPublicado?> ObtenerVigenteAsync(string codigo, CancellationToken cancellationToken)
    {
        var ahora = reloj.GetUtcNow().UtcDateTime;
        var vigente = (await VersionesVigentesQuery(ahora).ToListAsync(cancellationToken)).FirstOrDefault(v => v.Codigo == codigo);
        return vigente is null
            ? null
            : new DocumentoLegalPublicado(vigente.Codigo, vigente.Nombre, vigente.Version, vigente.VersionId, vigente.Contenido,
                vigente.HashContenido.Trim(), vigente.RequiereAceptacion);
    }

    public async Task<(bool Correcto, string? Error)> AceptarAsync(string usuarioId, string codigo, SolicitudAceptacionLegal solicitud,
        ContextoCliente cliente, CancellationToken cancellationToken)
    {
        var vigente = await ObtenerVigenteAsync(codigo, cancellationToken);
        if (vigente is null) return (false, CodigosPrivacidad.DocumentoNoEncontrado);
        if (vigente.VersionId != solicitud.VersionId || !string.Equals(vigente.HashContenido, solicitud.HashContenido, StringComparison.OrdinalIgnoreCase))
            return (false, CodigosPrivacidad.VersionInvalida);

        var existe = await database.AceptacionesLegales.AnyAsync(
            a => a.UsuarioId == usuarioId && a.VersionId == vigente.VersionId, cancellationToken);
        if (existe) return (true, null);

        var documentoId = await database.DocumentosLegales.AsNoTracking().Where(d => d.Codigo == codigo).Select(d => d.Id).SingleAsync(cancellationToken);
        database.AceptacionesLegales.Add(new AceptacionLegal
        {
            DocumentoId = documentoId,
            VersionId = vigente.VersionId,
            UsuarioId = usuarioId,
            FechaUtc = reloj.GetUtcNow().UtcDateTime,
            Ip = cliente.Ip,
            AgenteUsuario = cliente.AgenteUsuario,
            HashContenido = vigente.HashContenido.Trim(),
            FormaAceptacion = FormasAceptacionLegal.Explicita
        });
        await database.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    private IQueryable<VigenteProjection> VersionesVigentesQuery(DateTime ahora) =>
        from version in database.DocumentoLegalVersiones.AsNoTracking()
        join documento in database.DocumentosLegales.AsNoTracking() on version.DocumentoId equals documento.Id
        where documento.Activo && documento.RequiereAceptacion
              && version.VigenteDesde <= ahora && (version.VigenteHasta == null || version.VigenteHasta > ahora)
              && version.Version == database.DocumentoLegalVersiones
                  .Where(v => v.DocumentoId == documento.Id && v.VigenteDesde <= ahora && (v.VigenteHasta == null || v.VigenteHasta > ahora))
                  .Max(v => v.Version)
        orderby documento.Orden
        select new VigenteProjection(documento.Codigo, documento.Nombre, documento.RequiereAceptacion, version.Version, version.Id,
            version.Contenido, version.HashContenido);

    private sealed record VigenteProjection(
        string Codigo, string Nombre, bool RequiereAceptacion, int Version, Guid VersionId, string Contenido, string HashContenido);
}
