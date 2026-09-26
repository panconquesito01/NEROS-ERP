namespace Neros.Domain.Busqueda;

public static class MotorConsultaBusqueda
{
    public static ResultadoBusqueda Buscar(
        IReadOnlyList<DocumentoIndice> indice,
        IReadOnlySet<string> permisosUsuario,
        CriterioBusqueda criterio,
        IReadOnlyList<CheckpointIngesta> checkpoints)
    {
        ArgumentNullException.ThrowIfNull(indice);
        ArgumentNullException.ThrowIfNull(permisosUsuario);
        ArgumentNullException.ThrowIfNull(criterio);
        if (criterio.MaxResultados <= 0 || criterio.MaxResultados > 100)
            throw new ArgumentOutOfRangeException(nameof(criterio));

        var termino = criterio.Termino.Trim();
        var visibles = MotorAclBusqueda.FiltrarVisibles(indice, permisosUsuario, criterio.TenantId, criterio.EmpresaId);
        if (termino.Length > 0)
        {
            visibles = visibles.Where(d =>
                d.Titulo.Contains(termino, StringComparison.OrdinalIgnoreCase)
                || d.TextoBusqueda.Contains(termino, StringComparison.OrdinalIgnoreCase));
        }

        var ordenados = visibles
            .OrderBy(d => d.Titulo, StringComparer.OrdinalIgnoreCase)
            .ThenBy(d => d.Id)
            .ToList();

        if (criterio.CursorTitulo is not null && criterio.CursorId is not null)
        {
            ordenados = ordenados
                .Where(d => string.Compare(d.Titulo, criterio.CursorTitulo, StringComparison.OrdinalIgnoreCase) > 0
                            || (string.Equals(d.Titulo, criterio.CursorTitulo, StringComparison.OrdinalIgnoreCase)
                                && d.Id.CompareTo(criterio.CursorId.Value) > 0))
                .ToList();
        }

        var pagina = ordenados.Take(criterio.MaxResultados).ToList();
        DocumentoIndice? ultimo = pagina.Count == criterio.MaxResultados ? pagina[^1] : null;
        var frescura = CalcularFrescura(checkpoints ?? []);

        return new ResultadoBusqueda(
            pagina.Select(d => new DocumentoIndiceVisible(
                d.Id, d.TipoEntidad, d.EntidadId, d.Titulo, d.Resumen, d.IndexadoEnUtc)).ToList(),
            ultimo?.Titulo,
            ultimo?.Id,
            frescura);
    }

    public static IReadOnlyList<DocumentoIndiceVisible> Vista360(
        IReadOnlyList<DocumentoIndice> indice,
        IReadOnlyList<EnlaceVista360> enlaces,
        IReadOnlySet<string> permisosUsuario,
        Guid documentoOrigenId)
    {
        ArgumentNullException.ThrowIfNull(enlaces);
        var ids = enlaces.Where(e => e.DocumentoOrigenId == documentoOrigenId)
            .Select(e => e.DocumentoRelacionadoId)
            .ToHashSet();
        return indice.Where(d => ids.Contains(d.Id) && MotorAclBusqueda.PuedeVer(permisosUsuario, d))
            .Select(d => new DocumentoIndiceVisible(
                d.Id, d.TipoEntidad, d.EntidadId, d.Titulo, d.Resumen, d.IndexadoEnUtc))
            .ToList();
    }

    private static FrescuraIndice CalcularFrescura(IReadOnlyList<CheckpointIngesta> checkpoints)
    {
        if (checkpoints.Count == 0) return new FrescuraIndice(null, null);
        var masReciente = checkpoints.OrderByDescending(c => c.UltimoInstanteUtc).First();
        return new FrescuraIndice(masReciente.UltimoInstanteUtc, masReciente.FuenteModulo);
    }
}
