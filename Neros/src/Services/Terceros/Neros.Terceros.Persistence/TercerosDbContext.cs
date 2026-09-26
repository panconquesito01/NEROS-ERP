using Microsoft.EntityFrameworkCore;

namespace Neros.Terceros.Persistence;

public sealed class TercerosDbContext(DbContextOptions<TercerosDbContext> options) : DbContext(options)
{
    public DbSet<TerceroEntidad> Terceros => Set<TerceroEntidad>();
    public DbSet<IdentificacionEntidad> Identificaciones => Set<IdentificacionEntidad>();
    public DbSet<RolEntidad> Roles => Set<RolEntidad>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TerceroEntidad>(entity =>
        {
            entity.ToTable("Tercero", "terceros", tb => tb.IsTemporal(t =>
            {
                t.UseHistoryTable("TerceroHistorial", "terceros");
                t.HasPeriodStart("ValidFrom");
                t.HasPeriodEnd("ValidTo");
            }));
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Tipo).HasMaxLength(20);
            entity.Property(t => t.RazonSocial).HasMaxLength(200);
            entity.Property(t => t.NombreComercial).HasMaxLength(160);
            entity.HasIndex(t => new { t.TenantId, t.RazonSocial });
        });
        modelBuilder.Entity<IdentificacionEntidad>(entity =>
        {
            entity.ToTable("Identificacion", "terceros");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Pais).HasMaxLength(2).IsFixedLength();
            entity.Property(i => i.Tipo).HasMaxLength(20);
            entity.Property(i => i.Numero).HasMaxLength(30);
            entity.Property(i => i.DigitoVerificacion).HasMaxLength(1).IsFixedLength();
            entity.HasIndex(i => new { i.TenantId, i.Pais, i.Tipo, i.Numero }).IsUnique();
            entity.HasOne(i => i.Tercero).WithMany(t => t.Identificaciones).HasForeignKey(i => i.TerceroId);
        });
        modelBuilder.Entity<RolEntidad>(entity =>
        {
            entity.ToTable("Rol", "terceros");
            entity.HasKey(r => new { r.TerceroId, r.Rol });
            entity.Property(r => r.Rol).HasMaxLength(20);
            entity.HasOne(r => r.Tercero).WithMany(t => t.Roles).HasForeignKey(r => r.TerceroId);
        });
    }
}
