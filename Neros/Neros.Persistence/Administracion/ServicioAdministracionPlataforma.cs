using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Neros.Application.Administracion;
using Neros.Application.Seguridad;
using Neros.Contracts.Administracion;
using Neros.Persistence.Seguridad;

namespace Neros.Persistence.Administracion;

public sealed class ServicioAdministracionPlataforma(
    NerosDbContext database,
    UserManager<Usuario> usuarios,
    RegistroAuditoria auditoria) : IServicioAdministracionPlataforma
{
    private static readonly HashSet<string> TiposLogo = new(StringComparer.OrdinalIgnoreCase) { "image/png", "image/jpeg", "image/webp" };
    private const int MaxLogoBytes = 512 * 1024;

    public async Task<PaginaEmpresas> ListarEmpresasAsync(string? buscar, int pagina, int tamano, CancellationToken cancellationToken)
    {
        pagina = Math.Max(1, pagina);
        tamano = Math.Clamp(tamano, 1, 100);
        var consulta = database.Empresas.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim()[..Math.Min(buscar.Trim().Length, 100)];
            consulta = consulta.Where(e => e.Nombre.Contains(texto) || e.Codigo.Contains(texto) || e.Identificacion.Contains(texto));
        }
        var total = await consulta.CountAsync(cancellationToken);
        var lista = await consulta.OrderBy(e => e.Nombre).Skip((pagina - 1) * tamano).Take(tamano)
            .Select(e => new EmpresaAdministrada(e.Id, e.Codigo, e.Nombre, e.Identificacion, e.Activa, e.ImagenLogo != null))
            .ToListAsync(cancellationToken);
        return new PaginaEmpresas(lista, total, pagina, tamano);
    }

    public async Task<(EmpresaAdministrada? Empresa, string? Error)> CrearEmpresaAsync(
        string actorId, SolicitudCrearEmpresa solicitud, ContextoCliente cliente, CancellationToken cancellationToken)
    {
        var codigo = solicitud.Codigo.Trim().ToUpperInvariant();
        if (await database.Empresas.AnyAsync(e => e.Codigo == codigo, cancellationToken))
            return (null, CodigosAdministracion.EmpresaDuplicada);
        var empresa = new Empresa
        {
            Codigo = codigo,
            Nombre = solicitud.Nombre.Trim(),
            Identificacion = solicitud.Identificacion.Trim(),
            Activa = true
        };
        database.Empresas.Add(empresa);
        auditoria.Agregar("Plataforma.Empresa.Crear", RegistroAuditoria.Correcto, actorId, cliente, "Empresa", empresa.Id.ToString());
        await database.SaveChangesAsync(cancellationToken);
        return (new EmpresaAdministrada(empresa.Id, empresa.Codigo, empresa.Nombre, empresa.Identificacion, empresa.Activa, false), null);
    }

    public async Task<(bool Correcto, string? Error)> ActualizarLogoAsync(
        Guid empresaId, byte[] contenido, string contentType, ContextoCliente cliente, CancellationToken cancellationToken)
    {
        if (contenido.Length == 0 || contenido.Length > MaxLogoBytes || !TiposLogo.Contains(contentType))
            return (false, CodigosAdministracion.LogoInvalido);
        var empresa = await database.Empresas.SingleOrDefaultAsync(e => e.Id == empresaId, cancellationToken);
        if (empresa is null) return (false, null);
        empresa.ImagenLogo = contenido;
        empresa.ImagenLogoContentType = contentType;
        auditoria.Agregar("Plataforma.Empresa.Logo", RegistroAuditoria.Correcto, null, cliente, "Empresa", empresaId.ToString());
        await database.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(UsuarioCreado? Usuario, string? Error)> CrearUsuarioAsync(
        string actorId, SolicitudCrearUsuarioPlataforma solicitud,
        ContextoCliente cliente, CancellationToken cancellationToken)
    {
        var membresiaInicial = solicitud.MembresiaInicial;
        var correo = solicitud.Correo.Trim();
        if (await usuarios.FindByEmailAsync(correo) is not null)
            return (null, CodigosAdministracion.CorreoDuplicado);
        if (membresiaInicial is not null && !EsRolValido(membresiaInicial.Rol))
            return (null, CodigosAdministracion.RolInvalido);
        var clave = ServicioAdministracionUsuarios.GenerarClaveTemporal();
        var usuario = new Usuario
        {
            UserName = correo,
            Email = correo,
            EmailConfirmed = true,
            DebeCambiarClave = true,
            Activo = true
        };
        PerfilUsuarioMapper.AplicarPerfil(usuario, solicitud.Perfil);
        await using var tx = await database.Database.BeginTransactionAsync(cancellationToken);
        var resultado = await usuarios.CreateAsync(usuario, clave);
        if (!resultado.Succeeded)
            throw new InvalidOperationException(string.Join("; ", resultado.Errors.Select(e => e.Code)));
        if (membresiaInicial is not null)
        {
            if (!await database.Empresas.AnyAsync(e => e.Id == membresiaInicial.EmpresaId && e.Activa, cancellationToken))
                return (null, CodigosAdministracion.FueraEmpresa);
            database.UsuariosEmpresas.Add(new UsuarioEmpresa
            {
                UsuarioId = usuario.Id,
                EmpresaId = membresiaInicial.EmpresaId,
                Rol = NormalizarRol(membresiaInicial.Rol),
                ModulosHabilitados = PerfilUsuarioMapper.SerializarModulos(solicitud.ModulosHabilitados),
                Activo = true
            });
        }
        if (solicitud.AdministradorGlobal)
        {
            var permiso = await usuarios.AddClaimAsync(usuario, new Claim(ClaimTypes.Role, Permisos.RolAdministradorGlobal));
            if (!permiso.Succeeded)
                throw new InvalidOperationException(string.Join("; ", permiso.Errors.Select(error => error.Code)));
        }
        auditoria.Agregar("Plataforma.Usuario.Crear", RegistroAuditoria.Correcto, actorId, cliente, "Usuario", usuario.Id);
        await database.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return (new UsuarioCreado(usuario.Id, clave), null);
    }

    internal static bool EsRolValido(string rol) =>
        rol is "Administrador" or "Operador" or "Consulta";

    internal static string NormalizarRol(string rol) => rol switch
    {
        "Administrador" or "Operador" or "Consulta" => rol,
        _ => throw new ArgumentException("Rol invalido.", nameof(rol))
    };
}
