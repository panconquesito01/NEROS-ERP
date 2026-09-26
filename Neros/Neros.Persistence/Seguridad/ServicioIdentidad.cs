using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Neros.Application.Autenticacion;
using Neros.Application.Privacidad;
using Neros.Application.Seguridad;
using Neros.Contracts.Autenticacion;

namespace Neros.Persistence.Seguridad;

public sealed class ServicioIdentidad(
    NerosDbContext database,
    UserManager<Usuario> usuarios,
    SignInManager<Usuario> acceso,
    IPasswordHasher<Usuario> hasher,
    SesionesUsuario sesiones,
    RegistroAuditoria auditoria,
    IServicioPrivacidad privacidad,
    TimeProvider reloj) : IServicioIdentidad
{
    private static readonly TimeSpan IntervaloActividad = TimeSpan.FromMinutes(5);
    private static readonly Usuario UsuarioSenuelo = new();
    private static readonly string HashSenuelo = new PasswordHasher<Usuario>().HashPassword(UsuarioSenuelo, Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));

    public async Task<AccesoConcedido?> IniciarSesionAsync(SolicitudAcceso solicitud, ContextoCliente cliente, CancellationToken cancellationToken)
    {
        var usuario = await usuarios.FindByEmailAsync(solicitud.Correo.Trim());
        if (usuario is null || !usuario.Activo)
        {
            hasher.VerifyHashedPassword(UsuarioSenuelo, HashSenuelo, solicitud.Clave);
            await AuditarAccesoAsync(null, "Acceso rechazado", "Acceso.Iniciar", RegistroAuditoria.Rechazado, cliente, cancellationToken);
            return null;
        }

        var resultado = await acceso.CheckPasswordSignInAsync(usuario, solicitud.Clave, lockoutOnFailure: true);
        if (!resultado.Succeeded)
        {
            await AuditarAccesoAsync(usuario.Id, "Acceso rechazado", "Acceso.Iniciar", RegistroAuditoria.Rechazado, cliente, cancellationToken,
                resultado.IsLockedOut ? "bloqueado" : "credenciales");
            return null;
        }

        var expira = reloj.GetUtcNow().Add(solicitud.Recordarme ? TimeSpan.FromDays(7) : TimeSpan.FromHours(8));
        var (token, _) = sesiones.Emitir(usuario, expira, cliente);
        await AuditarAccesoAsync(usuario.Id, "Inicio de sesion", "Acceso.Iniciar", RegistroAuditoria.Correcto, cliente, cancellationToken);
        var permisos = Permisos.DePlataforma(await EsAdministradorGlobalAsync(usuario.Id, cancellationToken));
        return new AccesoConcedido(token, expira, await UsuarioActualAsync(usuario.Id, usuario.Nombre, usuario.Email!, usuario.DebeCambiarClave, permisos, cancellationToken));
    }

    public async Task<SesionValidada?> ValidarSesionAsync(string token, CancellationToken cancellationToken)
    {
        if (token.Length != 64 || !token.All(Uri.IsHexDigit))
        {
            return null;
        }

        var hash = SesionesUsuario.Hash(token);
        var ahora = reloj.GetUtcNow();
        var sesion = await database.Sesiones.AsNoTracking()
            .Where(sesion => sesion.TokenHash == hash && sesion.RevocadaEnUtc == null && sesion.Expira > ahora && sesion.Usuario.Activo &&
                sesion.SelloSeguridad == sesion.Usuario.SecurityStamp &&
                (!sesion.Usuario.LockoutEnd.HasValue || sesion.Usuario.LockoutEnd <= ahora))
            .Select(sesion => new
            {
                sesion.Id,
                sesion.UsuarioId,
                sesion.Usuario.Nombre,
                Correo = sesion.Usuario.Email!,
                sesion.Usuario.DebeCambiarClave,
                sesion.UltimaActividadUtc,
                AdministradorGlobal = database.UserClaims.Any(permiso => permiso.UserId == sesion.UsuarioId &&
                    permiso.ClaimType == ClaimTypes.Role && permiso.ClaimValue == Permisos.RolAdministradorGlobal)
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (sesion is null)
        {
            return null;
        }

        if (ahora.UtcDateTime - sesion.UltimaActividadUtc > IntervaloActividad)
        {
            await database.Sesiones.Where(entidad => entidad.Id == sesion.Id)
                .ExecuteUpdateAsync(cambios => cambios.SetProperty(entidad => entidad.UltimaActividadUtc, ahora.UtcDateTime), cancellationToken);
        }
        return new SesionValidada(
            await UsuarioActualAsync(sesion.UsuarioId, sesion.Nombre, sesion.Correo, sesion.DebeCambiarClave,
                Permisos.DePlataforma(sesion.AdministradorGlobal), cancellationToken),
            sesion.Id);
    }

    public async Task CerrarSesionAsync(string token, ContextoCliente cliente, CancellationToken cancellationToken)
    {
        var hash = SesionesUsuario.Hash(token);
        var sesion = await database.Sesiones.SingleOrDefaultAsync(entidad => entidad.TokenHash == hash && entidad.RevocadaEnUtc == null, cancellationToken);
        if (sesion is null)
        {
            return;
        }

        sesion.RevocadaEnUtc = reloj.GetUtcNow().UtcDateTime;
        sesion.MotivoRevocacion = "Cierre";
        await AuditarAccesoAsync(sesion.UsuarioId, "Cierre de sesion", "Acceso.Cerrar", RegistroAuditoria.Correcto, cliente, cancellationToken);
    }

    private async Task<UsuarioActual> UsuarioActualAsync(string id, string nombre, string correo, bool debeCambiarClave,
        IReadOnlyList<string> permisos, CancellationToken cancellationToken)
    {
        if (debeCambiarClave)
        {
            return new UsuarioActual(id, nombre, correo, true, permisos);
        }
        var pendientes = await privacidad.ListarPendientesAsync(id, cancellationToken);
        return new UsuarioActual(id, nombre, correo, false, permisos, pendientes.Select(p => p.Codigo).ToList());
    }

    private Task<bool> EsAdministradorGlobalAsync(string usuarioId, CancellationToken cancellationToken) =>
        database.UserClaims.AnyAsync(permiso => permiso.UserId == usuarioId &&
            permiso.ClaimType == ClaimTypes.Role && permiso.ClaimValue == Permisos.RolAdministradorGlobal, cancellationToken);

    private async Task AuditarAccesoAsync(string? usuarioId, string actividad, string accion, string resultado,
        ContextoCliente cliente, CancellationToken cancellationToken, string? detalle = null)
    {
        database.EventosAcceso.Add(new EventoAcceso { UsuarioId = usuarioId, Fecha = reloj.GetUtcNow(), Accion = actividad });
        auditoria.Agregar(accion, resultado, usuarioId, cliente, "Usuario", usuarioId, detalle);
        await database.SaveChangesAsync(cancellationToken);
    }
}
