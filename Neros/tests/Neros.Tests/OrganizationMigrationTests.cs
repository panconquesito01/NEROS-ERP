using Neros.Organization.Migration;
using System.Text.Json;
using Xunit;

namespace Neros.Tests;

public sealed class OrganizationMigrationTests
{
    [Fact]
    public void Preparacion_NoAsignaTenantYSuValidacionFalla()
    {
        Guid[] origen = [Guid.NewGuid(), Guid.NewGuid()];
        var plan = ValidadorMigracion.Preparar(origen);
        Assert.All(plan.Companies, empresa => Assert.Null(empresa.TenantId));
        Assert.Contains("empresa_tenant_no_determinado", ValidadorMigracion.Validar(plan, origen));
    }

    [Fact]
    public void Correspondencias_PermitenMultiplesEmpresasPorTenantYGrupoSinCruces()
    {
        Guid[] origen = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var grupo = Guid.NewGuid();
        var plan = new PlanMigracion(1, [new(tenantA, "TENANT-A"), new(tenantB, "TENANT-B")], [new(grupo, tenantA)],
            [new(origen[0], tenantA, grupo, EstadoCorrespondencia.Aprobada),
             new(origen[1], tenantA, grupo, EstadoCorrespondencia.Aprobada),
             new(origen[2], tenantB, null, EstadoCorrespondencia.Aprobada)]);
        Assert.Empty(ValidadorMigracion.Validar(plan, origen));
        Assert.Contains("empresa_grupo_fuera_de_tenant", ValidadorMigracion.Validar(plan with
        {
            Companies = [plan.Companies[0], plan.Companies[1], plan.Companies[2] with { BusinessGroupId = grupo }]
        }, origen));
        Assert.Contains("empresa_duplicada", ValidadorMigracion.Validar(plan with
        {
            Companies = [.. plan.Companies, plan.Companies[0] with { TenantId = tenantB }]
        }, origen));
        Assert.Contains("inventario_sin_correspondencia_exacta", ValidadorMigracion.Validar(plan, [.. origen, Guid.NewGuid()]));
        Assert.Contains("inventario_sin_correspondencia_exacta", ValidadorMigracion.Validar(plan, [origen[0]]));
    }

    [Fact]
    public async Task ManifiestoAutorizado_ConservaIdentificadoresYNoAsignaEmpresasFuturas()
    {
        var json = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures", "company-tenant-map.json"));
        var plan = JsonSerializer.Deserialize<PlanMigracion>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        var empresa = Assert.Single(plan.Companies);
        var tenant = Assert.Single(plan.Tenants);
        Assert.Equal("NEROS-TEST", tenant.Code);
        Assert.Equal(Guid.Parse("f70c8608-2f4a-4d79-90b8-30f1b569bd17"), tenant.TenantId);
        Assert.Equal(Guid.Parse("42336010-6818-443b-9166-d03ab1754478"), empresa.CompanyId);
        Assert.NotEqual(empresa.CompanyId, tenant.TenantId);
        Assert.Equal(tenant.TenantId, empresa.TenantId);
        Assert.Empty(ValidadorMigracion.Validar(plan, [empresa.CompanyId]));
        var futura = Guid.NewGuid();
        Assert.Contains("inventario_sin_correspondencia_exacta", ValidadorMigracion.Validar(plan, [empresa.CompanyId, futura]));
        var sinCorrespondencia = ValidadorMigracion.Preparar([futura]).Companies.Single();
        Assert.Null(sinCorrespondencia.TenantId);
        Assert.Contains("empresa_tenant_no_determinado", ValidadorMigracion.Validar(plan with
        {
            Companies = [empresa, sinCorrespondencia]
        }, [empresa.CompanyId, futura]));
    }

    [Fact]
    public void Correspondencias_RechazanCompanyIdComoTenantId()
    {
        var companyId = Guid.NewGuid();
        var plan = new PlanMigracion(1, [new(companyId, "TENANT-A")], [],
            [new(companyId, companyId, null, EstadoCorrespondencia.Aprobada)]);
        Assert.Contains("tenant_reutiliza_company_id", ValidadorMigracion.Validar(plan, [companyId]));
    }
}