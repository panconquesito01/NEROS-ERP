using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Neros.Application.Cuenta;
using Neros.Application.Privacidad;
using Neros.Application.Seguridad;
using Neros.Contracts.Autenticacion;
using Neros.Contracts.Cuenta;

namespace Neros.Persistence.Seguridad;

public sealed class ServicioCuenta(
    NerosDbContext database,
    UserManager<Usuario> usuarios,
    SignInManager<Usuario> acceso,
    SesionesUsuario sesiones,
    RegistroAuditoria auditoria,
    IServicioPrivacidad privacidad,
    TimeProvider reloj) : IServicioCuenta
{
    private const string Accion = "Cuenta.CambiarClave";

    public async Task<ResultadoCambioClave> CambiarClaveAsync(string usuarioId, Guid sesionActual, SolicitudCambioClave solicitud,
        ContextoCliente cliente, CancellationToken cancellationToken)
    {
        var usuario = await usuarios.FindByIdAsync(usuarioId) ?? throw new InvalidOperationException("Usuario autenticado inexistente.");
        var verificacion = await acceso.CheckPasswordSignInAsync(usuario, solicitud.ClaveActual, lockoutOnFailure: true);
        if (!verificacion.Succeeded)
        {
            var codigo = verificacion.IsLockedOut ? CodigosCuenta.Bloqueado : CodigosCuenta.ClaveActual;
            return await RechazarAsync(usuario.Id, codigo, cliente, cancellationToken);
        }
        if (solicitud.ClaveNueva == solicitud.ClaveActual)
        {
            return await RechazarAsync(usuario.Id, CodigosCuenta.MismaClave, cliente, cancellationToken);
        }

        var expira = await database.Sesiones.Where(sesion => sesion.Id == sesionActual && sesion.UsuarioId == usuarioId)
            .Select(sesion => sesion.Expira).SingleAsync(cancellationToken);
        await using var transaccion = await database.Database.BeginTransactionAsync(cancellationToken);
        usuario.DebeCambiarClave = false;
        var cambio = await usuarios.ChangePasswordAsync(usuario, solicitud.ClaveActual, solicitud.ClaveNueva);
        if (!cambio.Succeeded)
        {
            await transaccion.RollbackAsync(cancellationToken);
            database.ChangeTracker.Clear();
            var detalles = cambio.Errors.Select(error => error.Code).ToList();
            return await RechazarAsync(usuario.Id, CodigosCuenta.Politica, cliente, cancellationToken, detalles);
        }

        await sesiones.RevocarAsync(usuario.Id, null, "CambioClave", cancellationToken);
        var (token, _) = sesiones.Emitir(usuario, expira, cliente);
        auditoria.Agregar(Accion, RegistroAuditoria.Correcto, usuario.Id, cliente, "Usuario", usuario.Id);
        await database.SaveChangesAsync(cancellationToken);
        await transaccion.CommitAsync(cancellationToken);
        var administrador = await database.UserClaims.AnyAsync(permiso => permiso.UserId == usuario.Id &&
            permiso.ClaimType == ClaimTypes.Role && permiso.ClaimValue == Permisos.RolAdministradorGlobal, cancellationToken);
        var permisos = Permisos.DePlataforma(administrador);
        var pendientes = await privacidad.ListarPendientesAsync(usuario.Id, cancellationToken);
        return new ResultadoCambioClave(
            new AccesoConcedido(token, expira, new UsuarioActual(usuario.Id, usuario.Nombre, usuario.Email!, false, permisos, pendientes.Select(p => p.Codigo).ToList())),
            null);
    }

    public async Task<IReadOnlyList<SesionActiva>> ListarSesionesAsync(string usuarioId, Guid sesionActual, CancellationToken cancellationToken)
    {
        var ahora = reloj.GetUtcNow();
        return await database.Sesiones.AsNoTracking()
            .Where(sesion => sesion.UsuarioId == usuarioId && sesion.RevocadaEnUtc == null && sesion.Expira > ahora &&
                sesion.SelloSeguridad == sesion.Usuario.SecurityStamp)
            .OrderByDescending(sesion => sesion.Id == sesionActual).ThenByDescending(sesion => sesion.UltimaActividadUtc)
            .Take(50)
            .Select(sesion => new SesionActiva(sesion.Id, sesion.InicioUtc, sesion.UltimaActividadUtc, sesion.Expira,
                sesion.Ip, sesion.AgenteUsuario, sesion.Id == sesionActual))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CerrarSesionAsync(string usuarioId, Guid sesionId, ContextoCliente cliente, CancellationToken cancellationToken)
    {
        var ahora = reloj.GetUtcNow().UtcDateTime;
        var cerradas = await database.Sesiones
            .Where(sesion => sesion.Id == sesionId && sesion.UsuarioId == usuarioId && sesion.RevocadaEnUtc == null)
            .ExecuteUpdateAsync(cambios => cambios
                .SetProperty(sesion => sesion.RevocadaEnUtc, ahora)
                .SetProperty(sesion => sesion.MotivoRevocacion, "CierreRemoto"), cancellationToken);
        if (cerradas == 0)
        {
            return false;
        }
        auditoria.Agregar("Cuenta.CerrarSesion", RegistroAuditoria.Correcto, usuarioId, cliente, "Sesion", sesionId.ToString());
        await database.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> CerrarSesionesAsync(string usuarioId, Guid? excepto, ContextoCliente cliente, CancellationToken cancellationToken)
    {
        var cerradas = await sesiones.RevocarAsync(usuarioId, excepto, excepto is null ? "CierreTotal" : "CierreOtras", cancellationToken);
        auditoria.Agregar(excepto is null ? "Cuenta.CerrarTodasSesiones" : "Cuenta.CerrarOtrasSesiones", RegistroAuditoria.Correcto,
            usuarioId, cliente, "Usuario", usuarioId, $"sesiones={cerradas}");
        await database.SaveChangesAsync(cancellationToken);
        return cerradas;
    }

    private async Task<ResultadoCambioClave> RechazarAsync(string usuarioId, string codigo, ContextoCliente cliente,
        CancellationToken cancellationToken, IReadOnlyList<string>? detalles = null)
    {
        auditoria.Agregar(Accion, RegistroAuditoria.Rechazado, usuarioId, cliente, "Usuario", usuarioId, codigo);
        await database.SaveChangesAsync(cancellationToken);
        return new ResultadoCambioClave(null, new ErrorOperacion(codigo, detalles));
    }
}
