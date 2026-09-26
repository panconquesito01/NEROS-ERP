using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Neros.Application.Cuenta;
using Neros.Application.Seguridad;
using Neros.Contracts.Cuenta;
using Neros.Persistence.Administracion;

namespace Neros.Persistence.Seguridad;

public sealed class ServicioAdministracionUsuarios(
    NerosDbContext database,
    UserManager<Usuario> usuarios,
    SesionesUsuario sesiones,
    RegistroAuditoria auditoria,
    TimeProvider reloj) : IServicioAdministracionUsuarios
{
    private const string Mayusculas = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Minusculas = "abcdefghijkmnpqrstuvwxyz";
    private const string Digitos = "23456789";
    private const string Simbolos = "!@#$%*?-_+";

    public async Task<PaginaUsuarios> ListarAsync(string? buscar, int pagina, int tamano, CancellationToken cancellationToken)
    {
        pagina = Math.Max(1, pagina);
        tamano = Math.Clamp(tamano, 1, 100);
        var ahora = reloj.GetUtcNow();
        var consulta = database.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim()[..Math.Min(buscar.Trim().Length, 100)];
            var normalizado = texto.ToUpperInvariant();
            consulta = consulta.Where(usuario => usuario.Nombre.Contains(texto) || usuario.NormalizedEmail!.Contains(normalizado));
        }

        var total = await consulta.CountAsync(cancellationToken);
        var lista = await consulta.OrderBy(usuario => usuario.Nombre).ThenBy(usuario => usuario.Id)
            .Skip((pagina - 1) * tamano).Take(tamano)
            .Select(usuario => new UsuarioAdministrado(
                usuario.Id, usuario.Nombre, usuario.Email!, usuario.Activo,
                usuario.LockoutEnd.HasValue && usuario.LockoutEnd > ahora,
                usuario.DebeCambiarClave,
                database.UserClaims.Any(permiso => permiso.UserId == usuario.Id &&
                    permiso.ClaimType == ClaimTypes.Role && permiso.ClaimValue == Permisos.RolAdministradorGlobal),
                database.Sesiones.Where(sesion => sesion.UsuarioId == usuario.Id).Max(sesion => (DateTime?)sesion.InicioUtc)))
            .ToListAsync(cancellationToken);
        return new PaginaUsuarios(lista, total, pagina, tamano);
    }

    public async Task<DetalleUsuarioPlataforma?> ObtenerAsync(string usuarioId, CancellationToken cancellationToken)
    {
        var ahora = reloj.GetUtcNow();
        var usuario = await database.Users.AsNoTracking().FirstOrDefaultAsync(entidad => entidad.Id == usuarioId, cancellationToken);
        if (usuario is null)
        {
            return null;
        }

        var administradorGlobal = await database.UserClaims.AsNoTracking().AnyAsync(permiso =>
            permiso.UserId == usuario.Id && permiso.ClaimType == ClaimTypes.Role && permiso.ClaimValue == Permisos.RolAdministradorGlobal,
            cancellationToken);
        var membresias = await (
            from membresia in database.UsuariosEmpresas.AsNoTracking()
            join empresa in database.Empresas.AsNoTracking() on membresia.EmpresaId equals empresa.Id
            where membresia.UsuarioId == usuarioId && membresia.Activo
            orderby empresa.Nombre
            select new MembresiaUsuarioPlataforma(
                empresa.Id,
                empresa.Codigo,
                empresa.Nombre,
                membresia.Rol,
                ModulosEmpresa.Deserializar(membresia.ModulosHabilitados))
        ).ToListAsync(cancellationToken);
        return new DetalleUsuarioPlataforma(
            usuario.Id,
            usuario.Email!,
            usuario.Activo,
            usuario.LockoutEnd.HasValue && usuario.LockoutEnd > ahora,
            usuario.DebeCambiarClave,
            administradorGlobal,
            PerfilUsuarioMapper.LeerPerfil(usuario),
            membresias);
    }

    public async Task<(EstadoAdministracion Estado, string? ClaveTemporal)> RestablecerClaveAsync(
        string actorId, string usuarioId, ContextoCliente cliente, CancellationToken cancellationToken)
    {
        if (actorId == usuarioId)
        {
            return (EstadoAdministracion.PropiaCuenta, null);
        }
        var usuario = await usuarios.FindByIdAsync(usuarioId);
        if (usuario is null)
        {
            return (EstadoAdministracion.NoEncontrado, null);
        }

        var clave = GenerarClaveTemporal();
        await using var transaccion = await database.Database.BeginTransactionAsync(cancellationToken);
        Exigir(await usuarios.RemovePasswordAsync(usuario));
        Exigir(await usuarios.AddPasswordAsync(usuario, clave));
        usuario.DebeCambiarClave = true;
        usuario.LockoutEnd = null;
        usuario.AccessFailedCount = 0;
        var revocadas = await sesiones.RevocarAsync(usuario.Id, null, "RestablecimientoClave", cancellationToken);
        auditoria.Agregar("Usuario.RestablecerClave", RegistroAuditoria.Correcto, actorId, cliente, "Usuario", usuario.Id, $"sesiones={revocadas}");
        Exigir(await usuarios.UpdateAsync(usuario));
        await transaccion.CommitAsync(cancellationToken);
        return (EstadoAdministracion.Correcto, clave);
    }

    public async Task<EstadoAdministracion> DesbloquearAsync(string actorId, string usuarioId, ContextoCliente cliente, CancellationToken cancellationToken)
    {
        var usuario = await usuarios.FindByIdAsync(usuarioId);
        if (usuario is null)
        {
            return EstadoAdministracion.NoEncontrado;
        }
        usuario.LockoutEnd = null;
        usuario.AccessFailedCount = 0;
        auditoria.Agregar("Usuario.Desbloquear", RegistroAuditoria.Correcto, actorId, cliente, "Usuario", usuario.Id);
        Exigir(await usuarios.UpdateAsync(usuario));
        return EstadoAdministracion.Correcto;
    }

    public async Task<EstadoAdministracion> ActualizarAsync(
        string actorId, string usuarioId, SolicitudActualizarUsuarioPlataforma solicitud, ContextoCliente cliente, CancellationToken cancellationToken)
    {
        var usuario = await usuarios.FindByIdAsync(usuarioId);
        if (usuario is null)
        {
            return EstadoAdministracion.NoEncontrado;
        }

        foreach (var membresia in solicitud.Membresias)
        {
            if (!ServicioAdministracionPlataforma.EsRolValido(membresia.Rol))
                return EstadoAdministracion.RolInvalido;
        }

        await using var transaccion = await database.Database.BeginTransactionAsync(cancellationToken);
        PerfilUsuarioMapper.AplicarPerfil(usuario, solicitud.Perfil);
        Exigir(await usuarios.UpdateAsync(usuario));
        await SincronizarAdministradorGlobalAsync(usuario, solicitud.AdministradorGlobal, cancellationToken);
        foreach (var membresia in solicitud.Membresias)
        {
            if (!await database.Empresas.AnyAsync(empresa => empresa.Id == membresia.EmpresaId && empresa.Activa, cancellationToken))
                return EstadoAdministracion.FueraEmpresa;
            var existente = await database.UsuariosEmpresas.FindAsync([usuario.Id, membresia.EmpresaId], cancellationToken);
            if (existente is null)
            {
                database.UsuariosEmpresas.Add(new UsuarioEmpresa
                {
                    UsuarioId = usuario.Id,
                    EmpresaId = membresia.EmpresaId,
                    Rol = ServicioAdministracionPlataforma.NormalizarRol(membresia.Rol),
                    ModulosHabilitados = PerfilUsuarioMapper.SerializarModulos(membresia.ModulosHabilitados),
                    Activo = true
                });
            }
            else
            {
                existente.Rol = ServicioAdministracionPlataforma.NormalizarRol(membresia.Rol);
                existente.ModulosHabilitados = PerfilUsuarioMapper.SerializarModulos(membresia.ModulosHabilitados);
                existente.Activo = true;
            }
        }

        auditoria.Agregar("Usuario.Actualizar", RegistroAuditoria.Correcto, actorId, cliente, "Usuario", usuario.Id);
        await database.SaveChangesAsync(cancellationToken);
        await transaccion.CommitAsync(cancellationToken);
        return EstadoAdministracion.Correcto;
    }

    private async Task SincronizarAdministradorGlobalAsync(Usuario usuario, bool administradorGlobal, CancellationToken cancellationToken)
    {
        var claims = await usuarios.GetClaimsAsync(usuario);
        var actual = claims.FirstOrDefault(permiso =>
            permiso.Type == ClaimTypes.Role && permiso.Value == Permisos.RolAdministradorGlobal);
        if (administradorGlobal && actual is null)
            Exigir(await usuarios.AddClaimAsync(usuario, new Claim(ClaimTypes.Role, Permisos.RolAdministradorGlobal)));
        else if (!administradorGlobal && actual is not null)
            Exigir(await usuarios.RemoveClaimAsync(usuario, actual));
        _ = cancellationToken;
    }

    public async Task<EstadoAdministracion> CambiarEstadoAsync(
        string actorId, string usuarioId, bool activo, ContextoCliente cliente, CancellationToken cancellationToken)
    {
        if (actorId == usuarioId)
        {
            return EstadoAdministracion.PropiaCuenta;
        }

        var usuario = await usuarios.FindByIdAsync(usuarioId);
        if (usuario is null)
        {
            return EstadoAdministracion.NoEncontrado;
        }

        if (usuario.Activo == activo)
        {
            return EstadoAdministracion.Correcto;
        }

        await using var transaccion = await database.Database.BeginTransactionAsync(cancellationToken);
        usuario.Activo = activo;
        if (!activo)
        {
            var revocadas = await sesiones.RevocarAsync(usuario.Id, null, "CuentaInactiva", cancellationToken);
            auditoria.Agregar("Usuario.Inactivar", RegistroAuditoria.Correcto, actorId, cliente, "Usuario", usuario.Id, $"sesiones={revocadas}");
        }
        else
        {
            auditoria.Agregar("Usuario.Activar", RegistroAuditoria.Correcto, actorId, cliente, "Usuario", usuario.Id);
        }

        Exigir(await usuarios.UpdateAsync(usuario));
        await transaccion.CommitAsync(cancellationToken);
        return EstadoAdministracion.Correcto;
    }

    public async Task<EstadoAdministracion> EliminarPermanenteAsync(
        string actorId, string usuarioId, ContextoCliente cliente, CancellationToken cancellationToken)
    {
        if (actorId == usuarioId)
        {
            return EstadoAdministracion.PropiaCuenta;
        }

        var usuario = await usuarios.FindByIdAsync(usuarioId);
        if (usuario is null)
        {
            return EstadoAdministracion.NoEncontrado;
        }

        await using var transaccion = await database.Database.BeginTransactionAsync(cancellationToken);
        await sesiones.RevocarAsync(usuario.Id, null, "EliminacionCuenta", cancellationToken);
        await database.UsuariosEmpresas.Where(membresia => membresia.UsuarioId == usuario.Id).ExecuteDeleteAsync(cancellationToken);
        await database.EventosAcceso.Where(evento => evento.UsuarioId == usuario.Id).ExecuteDeleteAsync(cancellationToken);
        await database.AceptacionesLegales.Where(aceptacion => aceptacion.UsuarioId == usuario.Id).ExecuteDeleteAsync(cancellationToken);
        auditoria.Agregar("Usuario.Eliminar", RegistroAuditoria.Correcto, actorId, cliente, "Usuario", usuario.Id);
        Exigir(await usuarios.DeleteAsync(usuario));
        await transaccion.CommitAsync(cancellationToken);
        return EstadoAdministracion.Correcto;
    }

    /// <summary>Clave de 20 caracteres con mayuscula, minuscula, digito y simbolo; sin caracteres ambiguos.</summary>
    public static string GenerarClaveTemporal()
    {
        const string todos = Mayusculas + Minusculas + Digitos + Simbolos;
        var caracteres = new List<char>
        {
            Elegir(Mayusculas), Elegir(Minusculas), Elegir(Digitos), Elegir(Simbolos)
        };
        while (caracteres.Count < 20) caracteres.Add(Elegir(todos));
        for (var i = caracteres.Count - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (caracteres[i], caracteres[j]) = (caracteres[j], caracteres[i]);
        }
        return new string([.. caracteres]);
    }

    private static char Elegir(string alfabeto) => alfabeto[RandomNumberGenerator.GetInt32(alfabeto.Length)];

    private static void Exigir(IdentityResult resultado)
    {
        if (!resultado.Succeeded)
            throw new InvalidOperationException($"Identity rechazo la operacion: {string.Join(", ", resultado.Errors.Select(error => error.Code))}");
    }
}
