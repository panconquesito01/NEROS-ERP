using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Neros.Application.Seguridad;

namespace Neros.Persistence.Seguridad;

/// <summary>Emite y revoca sesiones opacas. Solo se guarda el hash SHA-256 del token.</summary>
public sealed class SesionesUsuario(NerosDbContext database, TimeProvider reloj)
{
    public (string Token, Sesion Sesion) Emitir(Usuario usuario, DateTimeOffset expira, ContextoCliente cliente)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var ahora = reloj.GetUtcNow().UtcDateTime;
        var sesion = new Sesion
        {
            TokenHash = Hash(token),
            UsuarioId = usuario.Id,
            SelloSeguridad = usuario.SecurityStamp!,
            Expira = expira,
            InicioUtc = ahora,
            UltimaActividadUtc = ahora,
            Ip = cliente.Ip,
            AgenteUsuario = cliente.AgenteUsuario
        };
        database.Sesiones.Add(sesion);
        return (token, sesion);
    }

    public Task<int> RevocarAsync(string usuarioId, Guid? excepto, string motivo, CancellationToken cancellationToken)
    {
        var ahora = reloj.GetUtcNow().UtcDateTime;
        return database.Sesiones
            .Where(sesion => sesion.UsuarioId == usuarioId && sesion.RevocadaEnUtc == null && (excepto == null || sesion.Id != excepto))
            .ExecuteUpdateAsync(cambios => cambios
                .SetProperty(sesion => sesion.RevocadaEnUtc, ahora)
                .SetProperty(sesion => sesion.MotivoRevocacion, motivo), cancellationToken);
    }

    public static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
