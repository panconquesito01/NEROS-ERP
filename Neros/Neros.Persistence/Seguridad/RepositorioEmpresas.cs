using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Neros.Application.Autenticacion;
using Neros.Contracts.Autenticacion;

namespace Neros.Persistence.Seguridad;

public sealed class RepositorioEmpresas(NerosDbContext database, TimeProvider reloj) : IRepositorioEmpresas
{
    private IQueryable<UsuarioEmpresa> Membresias(string usuarioId) => database.UsuariosEmpresas.AsNoTracking()
        .Where(membresia => membresia.UsuarioId == usuarioId && membresia.Activo && membresia.Usuario.Activo && membresia.Empresa.Activa);

    private IQueryable<Empresa> EmpresasAutorizadas(string usuarioId) => database.Empresas.AsNoTracking()
        .Where(empresa => empresa.Activa && database.Users.Any(usuario => usuario.Id == usuarioId && usuario.Activo) &&
            (database.UserClaims.Any(permiso => permiso.UserId == usuarioId &&
                permiso.ClaimType == ClaimTypes.Role && permiso.ClaimValue == "AdministradorGlobal") ||
             Membresias(usuarioId).Any(membresia => membresia.EmpresaId == empresa.Id)));

    private IQueryable<EmpresaDisponible> Proyectar(IQueryable<Empresa> empresas, string usuarioId) => empresas
        .Select(empresa => new EmpresaDisponible(empresa.Id, empresa.Codigo, empresa.Nombre, empresa.Identificacion,
            database.UserClaims.Any(permiso => permiso.UserId == usuarioId &&
                permiso.ClaimType == ClaimTypes.Role && permiso.ClaimValue == "AdministradorGlobal")
                ? "Administrador global"
                : database.UsuariosEmpresas.Where(membresia => membresia.UsuarioId == usuarioId && membresia.EmpresaId == empresa.Id)
                    .Select(membresia => membresia.Rol).First()));

    public async Task<IReadOnlyList<EmpresaDisponible>> ListarAsync(string usuarioId, CancellationToken cancellationToken) =>
        await Proyectar(EmpresasAutorizadas(usuarioId).OrderBy(empresa => empresa.Nombre), usuarioId).ToListAsync(cancellationToken);

    public Task<EmpresaDisponible?> ObtenerAutorizadaAsync(string usuarioId, Guid empresaId, CancellationToken cancellationToken) =>
        Proyectar(EmpresasAutorizadas(usuarioId).Where(empresa => empresa.Id == empresaId), usuarioId).SingleOrDefaultAsync(cancellationToken);

    public async Task RegistrarEntradaAsync(string usuarioId, Guid empresaId, CancellationToken cancellationToken)
    {
        if (!await EmpresasAutorizadas(usuarioId).AnyAsync(empresa => empresa.Id == empresaId, cancellationToken))
        {
            throw new UnauthorizedAccessException();
        }

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