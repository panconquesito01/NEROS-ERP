using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Neros.Application.Autenticacion;
using Neros.Application.Seguridad;
using Neros.Contracts.Autenticacion;

namespace Neros.Persistence.Seguridad;

public sealed class RepositorioEmpresas(NerosDbContext database, TimeProvider reloj) : IRepositorioEmpresas
{
    private IQueryable<UsuarioEmpresa> Membresias(string usuarioId) => database.UsuariosEmpresas.AsNoTracking()
        .Where(membresia => membresia.UsuarioId == usuarioId && membresia.Activo && membresia.Usuario.Activo && membresia.Empresa.Activa);

    private IQueryable<Empresa> EmpresasAutorizadas(string usuarioId) => database.Empresas.AsNoTracking()
        .Where(empresa => empresa.Activa && database.Users.Any(usuario => usuario.Id == usuarioId && usuario.Activo) &&
            (database.UserClaims.Any(permiso => permiso.UserId == usuarioId &&
                permiso.ClaimType == ClaimTypes.Role && permiso.ClaimValue == Permisos.RolAdministradorGlobal) ||
             Membresias(usuarioId).Any(membresia => membresia.EmpresaId == empresa.Id)));

    private Task<bool> EsAdministradorGlobalAsync(string usuarioId, CancellationToken cancellationToken) =>
        database.UserClaims.AnyAsync(permiso => permiso.UserId == usuarioId &&
            permiso.ClaimType == ClaimTypes.Role && permiso.ClaimValue == Permisos.RolAdministradorGlobal, cancellationToken);

    public async Task<IReadOnlyList<EmpresaDisponible>> ListarAsync(string usuarioId, CancellationToken cancellationToken)
    {
        var esGlobal = await EsAdministradorGlobalAsync(usuarioId, cancellationToken);
        var filas = await EmpresasAutorizadas(usuarioId).OrderBy(empresa => empresa.Nombre)
            .Select(empresa => new
            {
                empresa.Id,
                empresa.Codigo,
                empresa.Nombre,
                empresa.Identificacion,
                TieneLogo = empresa.ImagenLogo != null && empresa.ImagenLogo.Length > 0,
                Rol = esGlobal ? "Administrador global"
                    : database.UsuariosEmpresas.Where(membresia => membresia.UsuarioId == usuarioId && membresia.EmpresaId == empresa.Id)
                        .Select(membresia => membresia.Rol).First(),
                ModulosJson = database.UsuariosEmpresas.Where(membresia => membresia.UsuarioId == usuarioId && membresia.EmpresaId == empresa.Id)
                    .Select(membresia => membresia.ModulosHabilitados).FirstOrDefault()
            }).ToListAsync(cancellationToken);

        return filas.Select(f => new EmpresaDisponible(
            f.Id, f.Codigo, f.Nombre, f.Identificacion, f.Rol, f.TieneLogo,
            ModulosEmpresa.Resolver(f.ModulosJson, esGlobal))).ToList();
    }

    public async Task<EmpresaDisponible?> ObtenerAutorizadaAsync(string usuarioId, Guid empresaId, CancellationToken cancellationToken)
    {
        var esGlobal = await EsAdministradorGlobalAsync(usuarioId, cancellationToken);
        var fila = await EmpresasAutorizadas(usuarioId).Where(empresa => empresa.Id == empresaId)
            .Select(empresa => new
            {
                empresa.Id,
                empresa.Codigo,
                empresa.Nombre,
                empresa.Identificacion,
                TieneLogo = empresa.ImagenLogo != null && empresa.ImagenLogo.Length > 0,
                Rol = esGlobal ? "Administrador global"
                    : database.UsuariosEmpresas.Where(membresia => membresia.UsuarioId == usuarioId && membresia.EmpresaId == empresa.Id)
                        .Select(membresia => membresia.Rol).First(),
                ModulosJson = database.UsuariosEmpresas.Where(membresia => membresia.UsuarioId == usuarioId && membresia.EmpresaId == empresa.Id)
                    .Select(membresia => membresia.ModulosHabilitados).FirstOrDefault()
            }).SingleOrDefaultAsync(cancellationToken);

        return fila is null ? null : new EmpresaDisponible(
            fila.Id, fila.Codigo, fila.Nombre, fila.Identificacion, fila.Rol, fila.TieneLogo,
            ModulosEmpresa.Resolver(fila.ModulosJson, esGlobal));
    }

    public async Task<IReadOnlyList<string>> ConsultarModulosHabilitadosAsync(string usuarioId, Guid empresaId, CancellationToken cancellationToken)
    {
        if (!await EmpresasAutorizadas(usuarioId).AnyAsync(empresa => empresa.Id == empresaId, cancellationToken))
            return [];
        var esGlobal = await EsAdministradorGlobalAsync(usuarioId, cancellationToken);
        if (esGlobal)
            return ModulosEmpresa.Resolver(null, true);
        var json = await database.UsuariosEmpresas.AsNoTracking()
            .Where(m => m.UsuarioId == usuarioId && m.EmpresaId == empresaId && m.Activo)
            .Select(m => m.ModulosHabilitados)
            .SingleOrDefaultAsync(cancellationToken);
        return ModulosEmpresa.Resolver(json, false);
    }

    public async Task<bool> ModuloPermitidoAsync(string usuarioId, Guid empresaId, string claveModulo, CancellationToken cancellationToken)
    {
        var habilitados = await ConsultarModulosHabilitadosAsync(usuarioId, empresaId, cancellationToken);
        if (habilitados.Count == 0)
            return false;
        return ModulosEmpresa.ModuloPermitido(habilitados, claveModulo);
    }

    public async Task RegistrarEntradaAsync(string usuarioId, Guid empresaId, CancellationToken cancellationToken)
    {
        if (!await EmpresasAutorizadas(usuarioId).AnyAsync(empresa => empresa.Id == empresaId, cancellationToken))
            throw new UnauthorizedAccessException();

        database.EventosAcceso.Add(new EventoAcceso
        {
            UsuarioId = usuarioId, EmpresaId = empresaId, Fecha = reloj.GetUtcNow(), Accion = "Entrada a empresa"
        });
        await database.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ActividadAcceso>> ConsultarActividadAsync(string usuarioId, Guid empresaId, CancellationToken cancellationToken) =>
        await database.EventosAcceso.AsNoTracking()
            .Where(evento => evento.UsuarioId == usuarioId && evento.EmpresaId == empresaId &&
                EmpresasAutorizadas(usuarioId).Any(empresa => empresa.Id == empresaId))
            .OrderByDescending(evento => evento.Fecha).Take(8)
            .Select(evento => new ActividadAcceso(evento.Fecha, evento.Accion)).ToListAsync(cancellationToken);
}
