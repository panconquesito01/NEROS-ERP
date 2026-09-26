using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Neros.Persistence.Privacidad;
using Neros.Persistence.Seguridad;

namespace Neros.Persistence;

public sealed class NerosDbContext(DbContextOptions<NerosDbContext> options) : IdentityUserContext<Usuario>(options)
{
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<UsuarioEmpresa> UsuariosEmpresas => Set<UsuarioEmpresa>();
    public DbSet<Sesion> Sesiones => Set<Sesion>();
    public DbSet<EventoAcceso> EventosAcceso => Set<EventoAcceso>();
    public DbSet<EventoAuditoria> EventosAuditoria => Set<EventoAuditoria>();
    public DbSet<CatalogoDatoPersonal> CatalogoDatosPersonales => Set<CatalogoDatoPersonal>();
    public DbSet<DocumentoLegal> DocumentosLegales => Set<DocumentoLegal>();
    public DbSet<DocumentoLegalVersion> DocumentoLegalVersiones => Set<DocumentoLegalVersion>();
    public DbSet<AceptacionLegal> AceptacionesLegales => Set<AceptacionLegal>();
    public DbSet<DefinicionCookie> DefinicionesCookie => Set<DefinicionCookie>();
    public DbSet<RetencionPolicy> RetencionPolicies => Set<RetencionPolicy>();
    public DbSet<LegalHold> LegalHolds => Set<LegalHold>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Usuario>(usuario =>
        {
            usuario.Property(u => u.Nombre).HasMaxLength(160);
            usuario.Property(u => u.PrimerNombre).HasMaxLength(80);
            usuario.Property(u => u.SegundoNombre).HasMaxLength(80);
            usuario.Property(u => u.PrimerApellido).HasMaxLength(80);
            usuario.Property(u => u.SegundoApellido).HasMaxLength(80);
            usuario.Property(u => u.TipoDocumento).HasMaxLength(10).IsUnicode(false);
            usuario.Property(u => u.NumeroDocumento).HasMaxLength(30).IsUnicode(false);
            usuario.Property(u => u.Ciudad).HasMaxLength(120);
            usuario.Property(u => u.Direccion).HasMaxLength(256);
        });
        builder.Entity<Empresa>(empresa =>
        {
            empresa.HasKey(entidad => entidad.Id);
            empresa.Property(entidad => entidad.Codigo).HasMaxLength(20).IsRequired();
            empresa.Property(entidad => entidad.Nombre).HasMaxLength(160).IsRequired();
            empresa.Property(entidad => entidad.Identificacion).HasMaxLength(30).IsRequired();
            empresa.HasIndex(entidad => entidad.Codigo).IsUnique();
            empresa.Property(entidad => entidad.ImagenLogoContentType).HasMaxLength(100);
        });
        builder.Entity<UsuarioEmpresa>(membresia =>
        {
            membresia.HasKey(entidad => new { entidad.UsuarioId, entidad.EmpresaId });
            membresia.Property(entidad => entidad.Rol).HasMaxLength(30).IsRequired();
            membresia.Property(entidad => entidad.ModulosHabilitados).HasMaxLength(500).IsUnicode(false);
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
            sesion.HasIndex(entidad => entidad.Id).IsUnique().HasDatabaseName("UX_Sesiones_Id");
            sesion.Property(entidad => entidad.InicioUtc).HasColumnType("datetime2(3)");
            sesion.Property(entidad => entidad.UltimaActividadUtc).HasColumnType("datetime2(3)");
            sesion.Property(entidad => entidad.RevocadaEnUtc).HasColumnType("datetime2(3)");
            sesion.Property(entidad => entidad.Ip).HasMaxLength(45);
            sesion.Property(entidad => entidad.AgenteUsuario).HasMaxLength(256);
            sesion.Property(entidad => entidad.MotivoRevocacion).HasMaxLength(40).IsUnicode(false);
        });
        builder.Entity<EventoAuditoria>(evento =>
        {
            evento.ToTable("Evento", "auditoria", tabla => tabla.UseSqlOutputClause(false));
            evento.HasKey(entidad => entidad.Id).IsClustered(false);
            evento.Property(entidad => entidad.Id).ValueGeneratedNever();
            evento.Property(entidad => entidad.FechaUtc).HasColumnType("datetime2(3)");
            evento.Property(entidad => entidad.Modulo).HasMaxLength(40).IsUnicode(false);
            evento.Property(entidad => entidad.Accion).HasMaxLength(80).IsUnicode(false);
            evento.Property(entidad => entidad.Resultado).HasMaxLength(20).IsUnicode(false);
            evento.Property(entidad => entidad.ActorId).HasMaxLength(450);
            evento.Property(entidad => entidad.Entidad).HasMaxLength(60).IsUnicode(false);
            evento.Property(entidad => entidad.EntidadId).HasMaxLength(450);
            evento.Property(entidad => entidad.Ip).HasMaxLength(45);
            evento.Property(entidad => entidad.AgenteUsuario).HasMaxLength(256);
            evento.Property(entidad => entidad.CorrelationId).HasMaxLength(64).IsUnicode(false);
            evento.Property(entidad => entidad.Detalle).HasMaxLength(1000);
            evento.HasIndex(entidad => entidad.FechaUtc).IsClustered();
            evento.HasIndex(entidad => new { entidad.ActorId, entidad.FechaUtc });
            evento.HasIndex(entidad => new { entidad.Entidad, entidad.EntidadId, entidad.FechaUtc });
        });
        builder.Entity<EventoAcceso>(evento =>
        {
            evento.Property(entidad => entidad.UsuarioId).HasMaxLength(450);
            evento.Property(entidad => entidad.Accion).HasMaxLength(80).IsRequired();
            evento.HasIndex(entidad => new { entidad.UsuarioId, entidad.EmpresaId, entidad.Fecha });
            evento.HasOne<Usuario>().WithMany().HasForeignKey(entidad => entidad.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            evento.HasOne<Empresa>().WithMany().HasForeignKey(entidad => entidad.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<CatalogoDatoPersonal>(catalogo =>
        {
            catalogo.ToTable("CatalogoDatoPersonal", "privacidad");
            catalogo.Property(entidad => entidad.Campo).HasMaxLength(120).IsUnicode(false);
            catalogo.Property(entidad => entidad.Modulo).HasMaxLength(40).IsUnicode(false);
            catalogo.Property(entidad => entidad.Clasificacion).HasMaxLength(20).IsUnicode(false);
            catalogo.Property(entidad => entidad.Finalidad).HasMaxLength(500);
            catalogo.Property(entidad => entidad.Origen).HasMaxLength(200);
            catalogo.Property(entidad => entidad.RetencionCodigo).HasMaxLength(40).IsUnicode(false);
            catalogo.Property(entidad => entidad.PermisoRequerido).HasMaxLength(80).IsUnicode(false);
            catalogo.HasIndex(entidad => new { entidad.Campo, entidad.Modulo }).IsUnique().HasDatabaseName("UX_CatalogoDatoPersonal_Campo_Modulo");
        });
        builder.Entity<DocumentoLegal>(documento =>
        {
            documento.ToTable("DocumentoLegal", "privacidad");
            documento.Property(entidad => entidad.Codigo).HasMaxLength(40).IsUnicode(false);
            documento.Property(entidad => entidad.Nombre).HasMaxLength(160);
            documento.HasIndex(entidad => entidad.Codigo).IsUnique().HasDatabaseName("UX_DocumentoLegal_Codigo");
        });
        builder.Entity<DocumentoLegalVersion>(version =>
        {
            version.ToTable("DocumentoLegalVersion", "privacidad");
            version.Property(entidad => entidad.HashContenido).HasMaxLength(64).IsUnicode(false).IsFixedLength();
            version.Property(entidad => entidad.VigenteDesde).HasColumnType("datetime2(3)");
            version.Property(entidad => entidad.VigenteHasta).HasColumnType("datetime2(3)");
            version.Property(entidad => entidad.PublicadoPor).HasMaxLength(128);
            version.HasIndex(entidad => new { entidad.DocumentoId, entidad.Version }).IsUnique().HasDatabaseName("UX_DocumentoLegalVersion_Documento_Version");
        });
        builder.Entity<AceptacionLegal>(aceptacion =>
        {
            aceptacion.ToTable("AceptacionLegal", "privacidad");
            aceptacion.Property(entidad => entidad.UsuarioId).HasMaxLength(450);
            aceptacion.Property(entidad => entidad.FechaUtc).HasColumnType("datetime2(3)");
            aceptacion.Property(entidad => entidad.Ip).HasMaxLength(45);
            aceptacion.Property(entidad => entidad.AgenteUsuario).HasMaxLength(256);
            aceptacion.Property(entidad => entidad.HashContenido).HasMaxLength(64).IsUnicode(false).IsFixedLength();
            aceptacion.Property(entidad => entidad.FormaAceptacion).HasMaxLength(40).IsUnicode(false);
            aceptacion.HasIndex(entidad => new { entidad.UsuarioId, entidad.VersionId }).IsUnique().HasDatabaseName("UX_AceptacionLegal_Usuario_Version");
        });
        builder.Entity<DefinicionCookie>(def =>
        {
            def.ToTable("DefinicionCookie", "privacidad");
            def.Property(entidad => entidad.Nombre).HasMaxLength(120);
            def.Property(entidad => entidad.Almacenamiento).HasMaxLength(20).IsUnicode(false);
            def.Property(entidad => entidad.Proveedor).HasMaxLength(80);
            def.Property(entidad => entidad.Categoria).HasMaxLength(20).IsUnicode(false);
            def.Property(entidad => entidad.Finalidad).HasMaxLength(500);
            def.Property(entidad => entidad.Duracion).HasMaxLength(120);
            def.Property(entidad => entidad.Dominio).HasMaxLength(120);
            def.Property(entidad => entidad.UrlPolitica).HasMaxLength(500);
            def.HasIndex(entidad => new { entidad.Nombre, entidad.Almacenamiento }).IsUnique().HasDatabaseName("UX_DefinicionCookie_Nombre_Almacenamiento");
        });
        builder.Entity<RetencionPolicy>(politica =>
        {
            politica.ToTable("RetencionPolicy", "privacidad");
            politica.HasKey(entidad => entidad.Codigo);
            politica.Property(entidad => entidad.Codigo).HasMaxLength(40).IsUnicode(false);
            politica.Property(entidad => entidad.TipoInformacion).HasMaxLength(120);
            politica.Property(entidad => entidad.Jurisdiccion).HasMaxLength(10).IsUnicode(false);
            politica.Property(entidad => entidad.PeriodoDescripcion).HasMaxLength(200);
            politica.Property(entidad => entidad.InicioComputo).HasMaxLength(120);
            politica.Property(entidad => entidad.Fundamento).HasMaxLength(500);
            politica.Property(entidad => entidad.AccionFinal).HasMaxLength(20).IsUnicode(false);
        });
        builder.Entity<LegalHold>(hold =>
        {
            hold.ToTable("LegalHold", "privacidad");
            hold.Property(entidad => entidad.Referencia).HasMaxLength(120);
            hold.Property(entidad => entidad.Alcance).HasMaxLength(500);
            hold.Property(entidad => entidad.Motivo).HasMaxLength(500);
            hold.Property(entidad => entidad.FechaInicioUtc).HasColumnType("datetime2(3)");
            hold.Property(entidad => entidad.FechaFinUtc).HasColumnType("datetime2(3)");
        });
    }
}