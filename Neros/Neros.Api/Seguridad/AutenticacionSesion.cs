using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Neros.Application.Autenticacion;
using Neros.Application.Seguridad;
using Neros.Contracts.Autenticacion;
using Neros.Persistence;

namespace Neros.Api.Seguridad;

public sealed class AutenticacionSesion(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IServicioIdentidad identidad,
    NerosDbContext database) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }
        SesionValidada? sesion;
        try
        {
            sesion = await identidad.ValidarSesionAsync(header[7..], Context.RequestAborted);
        }
        catch (OperationCanceledException) when (Context.RequestAborted.IsCancellationRequested)
        {
            return AuthenticateResult.NoResult();
        }
        if (sesion is null)
        {
            return AuthenticateResult.Fail("Sesion no valida.");
        }
        var usuario = sesion.Usuario;
        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, usuario.Id),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Email, usuario.Correo),
            new Claim(SesionHttp.SesionIdClaim, sesion.SesionId.ToString())
        ];
        if (usuario.DebeCambiarClave) claims.Add(new Claim(SesionHttp.DebeCambiarClaveClaim, "true"));
        if (usuario.DocumentosLegalesPendientes is { Count: > 0 })
        {
            claims.Add(new Claim(SesionHttp.LegalPendienteClaim, "true"));
            claims.AddRange(usuario.DocumentosLegalesPendientes.Select(codigo => new Claim("neros:legal-doc", codigo)));
        }
        claims.AddRange((usuario.Permisos ?? []).Select(permiso => new Claim(Permisos.Claim, permiso)));
        await AgregarPermisosEmpresaAsync(claims, usuario.Id, Context.RequestAborted);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }

    private async Task AgregarPermisosEmpresaAsync(List<Claim> claims, string usuarioId, CancellationToken cancellationToken)
    {
        if (!Request.Headers.TryGetValue(CabecerasCliente.Empresa, out var valor))
            return;
        var textoEmpresa = valor.Count > 0 ? valor[0] : valor.ToString();
        if (!Guid.TryParse(textoEmpresa, out var empresaId))
            return;

        var esGlobal = await database.UserClaims.AnyAsync(c =>
            c.UserId == usuarioId && c.ClaimType == ClaimTypes.Role && c.ClaimValue == Permisos.RolAdministradorGlobal,
            cancellationToken);
        IReadOnlyList<string> permisosEmpresa;
        if (esGlobal)
            permisosEmpresa = Permisos.DeRolEmpresa("Administrador");
        else
        {
            var rol = await database.UsuariosEmpresas.AsNoTracking()
                .Where(m => m.UsuarioId == usuarioId && m.EmpresaId == empresaId && m.Activo)
                .Select(m => m.Rol)
                .SingleOrDefaultAsync(cancellationToken);
            if (rol is null) return;
            permisosEmpresa = Permisos.DeRolEmpresa(rol);
        }

        foreach (var permiso in permisosEmpresa)
        {
            if (claims.Any(c => c.Type == Permisos.Claim && c.Value == permiso)) continue;
            claims.Add(new Claim(Permisos.Claim, permiso));
        }
    }
}
