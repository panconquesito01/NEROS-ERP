using Microsoft.EntityFrameworkCore;

namespace Neros.Organization.Persistence;

public sealed class OrganizationDbContext(DbContextOptions<OrganizationDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<GrupoEmpresarial> GruposEmpresariales => Set<GrupoEmpresarial>();
    public DbSet<EmpresaOrganizacion> Empresas => Set<EmpresaOrganizacion>();
    public DbSet<Sucursal> Sucursales => Set<Sucursal>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Tenant>(tenant =>
        {
            tenant.ToTable("Tenant", "organizacion");
            tenant.HasKey(entidad => entidad.Id);
            tenant.Property(entidad => entidad.Codigo).HasMaxLength(40).IsUnicode(false);
            tenant.Property(entidad => entidad.Nombre).HasMaxLength(160);
            tenant.HasIndex(entidad => entidad.Codigo).IsUnique().HasDatabaseName("UX_Tenant_Codigo");
        });
        builder.Entity<GrupoEmpresarial>(grupo =>
        {
            grupo.ToTable("GrupoEmpresarial", "organizacion");
            grupo.HasKey(entidad => entidad.Id);
            grupo.Property(entidad => entidad.Codigo).HasMaxLength(40).IsUnicode(false);
            grupo.Property(entidad => entidad.Nombre).HasMaxLength(160);
            grupo.HasIndex(entidad => new { entidad.TenantId, entidad.Codigo }).IsUnique()
                .HasDatabaseName("UX_GrupoEmpresarial_Tenant_Codigo");
        });
        builder.Entity<EmpresaOrganizacion>(empresa =>
        {
            empresa.ToTable("Empresa", "organizacion");
            empresa.HasKey(entidad => entidad.Id);
            empresa.Property(entidad => entidad.Codigo).HasMaxLength(20).IsUnicode(false);
            empresa.Property(entidad => entidad.Nombre).HasMaxLength(160);
            empresa.Property(entidad => entidad.Identificacion).HasMaxLength(30);
            empresa.Property(entidad => entidad.Pais).HasMaxLength(2).IsUnicode(false).IsFixedLength();
            empresa.Property(entidad => entidad.MonedaFuncional).HasMaxLength(3).IsUnicode(false).IsFixedLength();
            empresa.Property(entidad => entidad.ZonaHoraria).HasMaxLength(64).IsUnicode(false);
            empresa.Property(entidad => entidad.CulturaFormato).HasMaxLength(10).IsUnicode(false);
            empresa.Property(entidad => entidad.MarcoContable).HasMaxLength(20).IsUnicode(false);
            empresa.HasIndex(entidad => new { entidad.TenantId, entidad.Codigo }).HasDatabaseName("IX_Empresa_Tenant");
            empresa.HasOne(entidad => entidad.Tenant).WithMany().HasForeignKey(entidad => entidad.TenantId);
        });
        builder.Entity<Sucursal>(sucursal =>
        {
            sucursal.ToTable("Sucursal", "organizacion");
            sucursal.HasKey(entidad => entidad.Id);
            sucursal.Property(entidad => entidad.Codigo).HasMaxLength(20).IsUnicode(false);
            sucursal.Property(entidad => entidad.Nombre).HasMaxLength(160);
            sucursal.HasIndex(entidad => new { entidad.EmpresaId, entidad.Codigo }).IsUnique()
                .HasDatabaseName("UX_Sucursal_Empresa_Codigo");
            sucursal.HasIndex(entidad => new { entidad.TenantId, entidad.EmpresaId }).HasDatabaseName("IX_Sucursal_Tenant");
        });
    }
}
