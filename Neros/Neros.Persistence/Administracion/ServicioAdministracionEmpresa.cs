using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Neros.Application.Administracion;
using Neros.Application.Cuenta;
using Neros.Application.Seguridad;
using Neros.Contracts.Administracion;
using Neros.Persistence.Seguridad;

namespace Neros.Persistence.Administracion;

public sealed class ServicioAdministracionEmpresa(
    NerosDbContext database,
    UserManager<Usuario> usuarios,
    SesionesUsuario sesiones,
    RegistroAuditoria auditoria,
    TimeProvider reloj) : IServicioAdministracionEmpresa
{
    public async Task<PaginaMiembrosEmpresa> ListarMiembrosAsync(
        string actorId, Guid empresaId, string? buscar, int pagina, int tamano, CancellationToken cancellationToken)
    {
        if (!await ActorAdministraEmpresaAsync(actorId, empresaId, cancellationToken))
            throw new UnauthorizedAccessException();
        pagina = Math.Max(1, pagina);
        tamano = Math.Clamp(tamano, 1, 100);
        var ahora = reloj.GetUtcNow();
        var consulta = database.UsuariosEmpresas.AsNoTracking()
            .Where(m => m.EmpresaId == empresaId && m.Activo)
            .Select(m => m.Usuario);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim()[..Math.Min(buscar.Trim().Length, 100)];
            var normalizado = texto.ToUpperInvariant();
            consulta = consulta.Where(u => u.Nombre.Contains(texto) || u.NormalizedEmail!.Contains(normalizado));
        }
        var total = await consulta.CountAsync(cancellationToken);
        var miembros = await consulta.OrderBy(u => u.Nombre).Skip((pagina - 1) * tamano).Take(tamano)
            .Select(u => new UsuarioEmpresaAdministrado(
                u.Id, u.Nombre, u.Email!, database.UsuariosEmpresas
                    .Where(m => m.UsuarioId == u.Id && m.EmpresaId == empresaId).Select(m => m.Rol).First(),
                u.Activo,
                u.LockoutEnd.HasValue && u.LockoutEnd > ahora,
                u.DebeCambiarClave,
                database.Sesiones.Where(s => s.UsuarioId == u.Id).Max(s => (DateTime?)s.InicioUtc)))
            .ToListAsync(cancellationToken);
        return new PaginaMiembrosEmpresa(miembros, total, pagina, tamano);
    }

    public async Task<(UsuarioCreado? Usuario, string? Error)> CrearMiembroAsync(
        string actorId, Guid empresaId, SolicitudCrearMiembroEmpresa solicitud,
        ContextoCliente cliente, CancellationToken cancellationToken)
    {
        var rol = solicitud.Rol;
        if (!await ActorAdministraEmpresaAsync(actorId, empresaId, cancellationToken))
            throw new UnauthorizedAccessException();
        if (!ServicioAdministracionPlataforma.EsRolValido(rol))
            return (null, CodigosAdministracion.RolInvalido);
        var correo = solicitud.Correo.Trim();
        var existente = await usuarios.FindByEmailAsync(correo);
        string clave;
        Usuario usuario;
        if (existente is null)
        {
            clave = ServicioAdministracionUsuarios.GenerarClaveTemporal();
            usuario = new Usuario
            {
                UserName = correo,
                Email = correo,
                EmailConfirmed = true,
                DebeCambiarClave = true,
                Activo = true
            };
            PerfilUsuarioMapper.AplicarPerfil(usuario, solicitud.Perfil);
            var crear = await usuarios.CreateAsync(usuario, clave);
            if (!crear.Succeeded)
                throw new InvalidOperationException(string.Join("; ", crear.Errors.Select(e => e.Code)));
        }
        else
        {
            usuario = existente;
            if (await database.UsuariosEmpresas.AnyAsync(m => m.UsuarioId == usuario.Id && m.EmpresaId == empresaId, cancellationToken))
                return (null, CodigosAdministracion.MembresiaDuplicada);
            clave = string.Empty;
        }
        database.UsuariosEmpresas.Add(new UsuarioEmpresa
        {
            UsuarioId = usuario.Id,
            EmpresaId = empresaId,
            Rol = ServicioAdministracionPlataforma.NormalizarRol(rol),
            ModulosHabilitados = PerfilUsuarioMapper.SerializarModulos(solicitud.ModulosHabilitados),
            Activo = true
        });
        auditoria.Agregar("Empresa.Miembro.Crear", RegistroAuditoria.Correcto, actorId, cliente, "Usuario", usuario.Id, $"empresa={empresaId}");
        await database.SaveChangesAsync(cancellationToken);
        return clave.Length > 0 ? (new UsuarioCreado(usuario.Id, clave), null) : (new UsuarioCreado(usuario.Id, string.Empty), null);
    }

    public async Task<(EstadoAdministracion Estado, string? ClaveTemporal)> RestablecerClaveAsync(
        string actorId, Guid empresaId, string usuarioId, ContextoCliente cliente, CancellationToken cancellationToken)
    {
        if (actorId == usuarioId) return (EstadoAdministracion.PropiaCuenta, null);
        if (!await ActorAdministraEmpresaAsync(actorId, empresaId, cancellationToken)
            || !await EsMiembroEmpresaAsync(usuarioId, empresaId, cancellationToken))
            return (EstadoAdministracion.NoEncontrado, null);
        var usuario = await usuarios.FindByIdAsync(usuarioId);
        if (usuario is null) return (EstadoAdministracion.NoEncontrado, null);
        var clave = ServicioAdministracionUsuarios.GenerarClaveTemporal();
        await using var tx = await database.Database.BeginTransactionAsync(cancellationToken);
        var quitar = await usuarios.RemovePasswordAsync(usuario);
        if (!quitar.Succeeded) throw new InvalidOperationException("Identity rechazo quitar clave.");
        var agregar = await usuarios.AddPasswordAsync(usuario, clave);
        if (!agregar.Succeeded) throw new InvalidOperationException("Identity rechazo clave.");
        usuario.DebeCambiarClave = true;
        usuario.LockoutEnd = null;
        usuario.AccessFailedCount = 0;
        await sesiones.RevocarAsync(usuario.Id, null, "RestablecimientoClave", cancellationToken);
        auditoria.Agregar("Empresa.Miembro.RestablecerClave", RegistroAuditoria.Correcto, actorId, cliente, "Usuario", usuario.Id, $"empresa={empresaId}");
        await usuarios.UpdateAsync(usuario);
        await tx.CommitAsync(cancellationToken);
        return (EstadoAdministracion.Correcto, clave);
    }

    public async Task<EstadoAdministracion> DesbloquearAsync(
        string actorId, Guid empresaId, string usuarioId, ContextoCliente cliente, CancellationToken cancellationToken)
    {
        if (!await ActorAdministraEmpresaAsync(actorId, empresaId, cancellationToken)
            || !await EsMiembroEmpresaAsync(usuarioId, empresaId, cancellationToken))
            return EstadoAdministracion.NoEncontrado;
        var usuario = await usuarios.FindByIdAsync(usuarioId);
        if (usuario is null) return EstadoAdministracion.NoEncontrado;
        usuario.LockoutEnd = null;
        usuario.AccessFailedCount = 0;
        auditoria.Agregar("Empresa.Miembro.Desbloquear", RegistroAuditoria.Correcto, actorId, cliente, "Usuario", usuario.Id, $"empresa={empresaId}");
        await usuarios.UpdateAsync(usuario);
        return EstadoAdministracion.Correcto;
    }

    private async Task<bool> ActorAdministraEmpresaAsync(string actorId, Guid empresaId, CancellationToken cancellationToken)
    {
        if (await EsAdministradorGlobalAsync(actorId, cancellationToken)) return true;
        return await database.UsuariosEmpresas.AnyAsync(m =>
            m.UsuarioId == actorId && m.EmpresaId == empresaId && m.Activo && m.Rol == "Administrador", cancellationToken);
    }

    private Task<bool> EsMiembroEmpresaAsync(string usuarioId, Guid empresaId, CancellationToken cancellationToken) =>
        database.UsuariosEmpresas.AnyAsync(m => m.UsuarioId == usuarioId && m.EmpresaId == empresaId && m.Activo, cancellationToken);

    private Task<bool> EsAdministradorGlobalAsync(string actorId, CancellationToken cancellationToken) =>
        database.UserClaims.AnyAsync(c =>
            c.UserId == actorId && c.ClaimType == System.Security.Claims.ClaimTypes.Role
            && c.ClaimValue == Permisos.RolAdministradorGlobal, cancellationToken);
}
