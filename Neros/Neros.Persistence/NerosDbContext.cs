using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Neros.Persistence.Seguridad;

namespace Neros.Persistence;

public sealed class NerosDbContext(DbContextOptions<NerosDbContext> options) : IdentityUserContext<Usuario>(options)
{
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<UsuarioEmpresa> UsuariosEmpresas => Set<UsuarioEmpresa>();
    public DbSet<Sesion> Sesiones => Set<Sesion>();
    public DbSet<EventoAcceso> EventosAcceso => Set<EventoAcceso>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Usuario>().Property(usuario => usuario.Nombre).HasMaxLength(160);
        builder.Entity<Empresa>(empresa =>
        {
            empresa.HasKey(entidad => entidad.Id);
            empresa.Property(entidad => entidad.Codigo).HasMaxLength(20).IsRequired();
            empresa.Property(entidad => entidad.Nombre).HasMaxLength(160).IsRequired();
            empresa.Property(entidad => entidad.Identificacion).HasMaxLength(30).IsRequired();
            empresa.HasIndex(entidad => entidad.Codigo).IsUnique();
        });
        builder.Entity<UsuarioEmpresa>(membresia =>
        {
            membresia.HasKey(entidad => new { entidad.UsuarioId, entidad.EmpresaId });
            membresia.Property(entidad => entidad.Rol).HasMaxLength(30).IsRequired();
            membresia.HasOne(entidad => entidad.Usuario).WithMany().HasForeignKey(entidad => entidad.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            membresia.HasOne(entidad => entidad.Empresa).WithMany().HasForeignKey(entidad => entidad.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<Sesion>(sesion =>
        {
            sesion.HasKey(entidad => entidad.TokenHash);
            sesion.Property(entidad => entidad.TokenHash).HasMaxLength(64).IsUnicode(false);
            sesion.Property(entidad => entidad.SelloSeguridad).HasMaxLength(256).IsRequired();
            sesion.HasOne(entidad => entidad.Usuario).WithMany().HasForeignKey(entidad => entidad.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            sesion.HasIndex(entidad => entidad.Expira);
        });
        builder.Entity<EventoAcceso>(evento =>
        {
            evento.Property(entidad => entidad.UsuarioId).HasMaxLength(450);
            evento.Property(entidad => entidad.Accion).HasMaxLength(80).IsRequired();
            evento.HasIndex(entidad => new { entidad.UsuarioId, entidad.EmpresaId, entidad.Fecha });
            evento.HasOne<Usuario>().WithMany().HasForeignKey(entidad => entidad.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            evento.HasOne<Empresa>().WithMany().HasForeignKey(entidad => entidad.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}