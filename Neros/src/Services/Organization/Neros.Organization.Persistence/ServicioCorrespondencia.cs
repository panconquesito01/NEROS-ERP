using Microsoft.EntityFrameworkCore;
using Neros.Contracts.Organizacion;

namespace Neros.Organization.Persistence;

public sealed class ServicioCorrespondencia(OrganizationDbContext database)
{
    public async Task<CorrespondenciaEmpresa?> ObtenerAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var fila = await database.Empresas.AsNoTracking()
            .Include(empresa => empresa.Tenant)
            .Where(empresa => empresa.Id == companyId && empresa.Activa && empresa.Tenant.Activo)
            .Select(empresa => new
            {
                empresa.Id,
                empresa.TenantId,
                empresa.Tenant.Codigo,
                empresa.GrupoEmpresarialId,
                empresa.Pais,
                empresa.MonedaFuncional,
                empresa.ZonaHoraria,
                empresa.CulturaFormato,
                empresa.MarcoContable
            })
            .SingleOrDefaultAsync(cancellationToken);
        return fila is null
            ? null
            : new CorrespondenciaEmpresa(fila.Id, fila.TenantId, fila.Codigo, fila.GrupoEmpresarialId,
                new ConfiguracionRegionalEmpresa(fila.Pais.Trim(), fila.MonedaFuncional.Trim(), fila.ZonaHoraria,
                    fila.CulturaFormato, fila.MarcoContable));
    }

    public async Task UpsertEmpresaAsync(Guid tenantId, EmpresaOrganizacion empresa, CancellationToken cancellationToken)
    {
        var existente = await database.Empresas.FindAsync([empresa.Id], cancellationToken);
        if (existente is null)
        {
            empresa.TenantId = tenantId;
            database.Empresas.Add(empresa);
        }
        else
        {
            existente.TenantId = tenantId;
            existente.Codigo = empresa.Codigo;
            existente.Nombre = empresa.Nombre;
            existente.Identificacion = empresa.Identificacion;
            existente.Activa = empresa.Activa;
            existente.Pais = empresa.Pais;
            existente.MonedaFuncional = empresa.MonedaFuncional;
            existente.ZonaHoraria = empresa.ZonaHoraria;
            existente.CulturaFormato = empresa.CulturaFormato;
            existente.MarcoContable = empresa.MarcoContable;
        }
        await database.SaveChangesAsync(cancellationToken);
    }
}
