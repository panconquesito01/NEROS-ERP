using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Neros.Application.Autenticacion;
using Neros.Contracts.Autenticacion;

namespace Neros.Persistence.Seguridad;

public sealed class ServicioIdentidad(
    NerosDbContext database,
    UserManager<Usuario> usuarios,
    SignInManager<Usuario> acceso,
    IPasswordHasher<Usuario> hasher,
    TimeProvider reloj) : IServicioIdentidad
{
    private static readonly Usuario UsuarioSenuelo = new();
    private static readonly string HashSenuelo = new PasswordHasher<Usuario>().HashPassword(UsuarioSenuelo, Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));

    public async Task<AccesoConcedido?> IniciarSesionAsync(SolicitudAcceso solicitud, CancellationToken cancellationToken)
    {
        var usuario = await usuarios.FindByEmailAsync(solicitud.Correo.Trim());
        if (usuario is null || !usuario.Activo)
        {
            hasher.VerifyHashedPassword(UsuarioSenuelo, HashSenuelo, solicitud.Clave);
            await AuditarAsync(null, "Acceso rechazado", cancellationToken);
            return null;
        }

        var resultado = await acceso.CheckPasswordSignInAsync(usuario, solicitud.Clave, lockoutOnFailure: true);
        if (!resultado.Succeeded)
        {
            await AuditarAsync(usuario.Id, "Acceso rechazado", cancellationToken);
            return null;
        }

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var expira = reloj.GetUtcNow().Add(solicitud.Recordarme ? TimeSpan.FromDays(7) : TimeSpan.FromHours(8));
        database.Sesiones.Add(new Sesion
        {
            TokenHash = CalcularHash(token),
            UsuarioId = usuario.Id,
            SelloSeguridad = usuario.SecurityStamp!,
            Expira = expira
        });
        await AuditarAsync(usuario.Id, "Inicio de sesion", cancellationToken);
        return new AccesoConcedido(token, expira, new UsuarioActual(usuario.Id, usuario.Nombre, usuario.Email!));
    }

    public async Task<UsuarioActual?> ValidarSesionAsync(string token, CancellationToken cancellationToken)
    {
        if (token.Length != 64 || !token.All(Uri.IsHexDigit))
        {
            return null;
        }

        var hash = CalcularHash(token);
        var ahora = reloj.GetUtcNow();
        return await database.Sesiones.AsNoTracking()
            .Where(sesion => sesion.TokenHash == hash && sesion.Expira > ahora && sesion.Usuario.Activo &&
                sesion.SelloSeguridad == sesion.Usuario.SecurityStamp &&
                (!sesion.Usuario.LockoutEnd.HasValue || sesion.Usuario.LockoutEnd <= ahora))
            .Select(sesion => new UsuarioActual(sesion.UsuarioId, sesion.Usuario.Nombre, sesion.Usuario.Email!))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task CerrarSesionAsync(string token, CancellationToken cancellationToken)
    {
        var hash = CalcularHash(token);
        var sesion = await database.Sesiones.SingleOrDefaultAsync(entidad => entidad.TokenHash == hash, cancellationToken);
        if (sesion is null)
        {
            return;
        }

        database.Sesiones.Remove(sesion);
        await AuditarAsync(sesion.UsuarioId, "Cierre de sesion", cancellationToken);
    }

    private async Task AuditarAsync(string? usuarioId, string accion, CancellationToken cancellationToken)
    {
        database.EventosAcceso.Add(new EventoAcceso { UsuarioId = usuarioId, Fecha = reloj.GetUtcNow(), Accion = accion });
        await database.SaveChangesAsync(cancellationToken);
    }

    private static string CalcularHash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}