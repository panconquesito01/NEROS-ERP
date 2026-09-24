using Neros.Contracts.Autenticacion;

namespace Neros.Application.Autenticacion;

public interface IServicioIdentidad
{
    Task<AccesoConcedido?> IniciarSesionAsync(SolicitudAcceso solicitud, CancellationToken cancellationToken);
    Task<UsuarioActual?> ValidarSesionAsync(string token, CancellationToken cancellationToken);
    Task CerrarSesionAsync(string token, CancellationToken cancellationToken);
}

public interface IRepositorioEmpresas
{
    Task<IReadOnlyList<EmpresaDisponible>> ListarAsync(string usuarioId, CancellationToken cancellationToken);
    Task<EmpresaDisponible?> ObtenerAutorizadaAsync(string usuarioId, Guid empresaId, CancellationToken cancellationToken);
    Task RegistrarEntradaAsync(string usuarioId, Guid empresaId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ActividadAcceso>> ConsultarActividadAsync(string usuarioId, Guid empresaId, CancellationToken cancellationToken);
}