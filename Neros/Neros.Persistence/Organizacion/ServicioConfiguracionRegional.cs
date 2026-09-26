using Microsoft.EntityFrameworkCore;
using Neros.Application.Autenticacion;
using Neros.Application.Globalizacion;
using Neros.Application.Organizacion;
using Neros.Contracts.Organizacion;
using Neros.Organization.Persistence;

namespace Neros.Persistence.Organizacion;

public sealed class ServicioConfiguracionRegional(
    OrganizationDbContext organizacion,
    IRepositorioEmpresas empresas,
    IServicioCatalogosGlobalizacion catalogos) : IServicioConfiguracionRegional
{
    public async Task<ConfiguracionRegionalEmpresa?> ObtenerAsync(string actorId, Guid empresaId, CancellationToken cancellationToken)
    {
        if (await empresas.ObtenerAutorizadaAsync(actorId, empresaId, cancellationToken) is null)
            return null;
        var fila = await organizacion.Empresas.AsNoTracking()
            .Where(e => e.Id == empresaId && e.Activa)
            .Select(e => new ConfiguracionRegionalEmpresa(
                e.Pais.Trim(), e.MonedaFuncional.Trim(), e.ZonaHoraria.Trim(), e.CulturaFormato, e.MarcoContable))
            .SingleOrDefaultAsync(cancellationToken);
        return fila ?? new ConfiguracionRegionalEmpresa("CO", "COP", "America/Bogota", "es-CO", "Local");
    }

    public async Task<(ConfiguracionRegionalEmpresa? Configuracion, string? Error)> ActualizarAsync(
        string actorId, Guid empresaId, ConfiguracionRegionalEmpresa solicitud, CancellationToken cancellationToken)
    {
        if (await empresas.ObtenerAutorizadaAsync(actorId, empresaId, cancellationToken) is null)
            return (null, "sin_acceso");
        var error = await ValidarCatalogosAsync(solicitud, cancellationToken);
        if (error is not null) return (null, error);

        var empresa = await organizacion.Empresas.SingleOrDefaultAsync(e => e.Id == empresaId, cancellationToken);
        if (empresa is null) return (null, "sin_organizacion");

        empresa.Pais = solicitud.Pais.Trim().ToUpperInvariant();
        empresa.MonedaFuncional = solicitud.MonedaFuncional.Trim().ToUpperInvariant();
        empresa.ZonaHoraria = solicitud.ZonaHoraria.Trim();
        empresa.CulturaFormato = solicitud.CulturaFormato.Trim();
        empresa.MarcoContable = solicitud.MarcoContable.Trim();
        await organizacion.SaveChangesAsync(cancellationToken);
        return (solicitud, null);
    }

    private async Task<string?> ValidarCatalogosAsync(ConfiguracionRegionalEmpresa solicitud, CancellationToken cancellationToken)
    {
        var cat = await catalogos.ObtenerAsync(solicitud.Pais, cancellationToken);
        if (!cat.Paises.Any(p => p.Codigo.Equals(solicitud.Pais, StringComparison.OrdinalIgnoreCase)))
            return "pais_invalido";
        if (!cat.Monedas.Any(m => m.Codigo.Equals(solicitud.MonedaFuncional, StringComparison.OrdinalIgnoreCase)))
            return "moneda_invalida";
        if (!cat.ZonasHorarias.Any(z => z.Id.Equals(solicitud.ZonaHoraria, StringComparison.OrdinalIgnoreCase)))
            return "zona_invalida";
        if (string.IsNullOrWhiteSpace(solicitud.CulturaFormato) || solicitud.CulturaFormato.Length > 10)
            return "cultura_invalida";
        if (string.IsNullOrWhiteSpace(solicitud.MarcoContable) || solicitud.MarcoContable.Length > 30)
            return "marco_invalido";
        return null;
    }
}
