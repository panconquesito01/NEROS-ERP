using Neros.Application.Seguridad;
using Neros.Contracts.Autenticacion;

namespace Neros.Application.Autenticacion;

public sealed record SesionValidada(UsuarioActual Usuario, Guid SesionId);

public interface IServicioIdentidad
{
    Task<AccesoConcedido?> IniciarSesionAsync(SolicitudAcceso solicitud, ContextoCliente cliente, CancellationToken cancellationToken);
    Task<SesionValidada?> ValidarSesionAsync(string token, CancellationToken cancellationToken);
    Task CerrarSesionAsync(string token, ContextoCliente cliente, CancellationToken cancellationToken);
}

public interface IRepositorioEmpresas
{
    Task<IReadOnlyList<EmpresaDisponible>> ListarAsync(string usuarioId, CancellationToken cancellationToken);
    Task<EmpresaDisponible?> ObtenerAutorizadaAsync(string usuarioId, Guid empresaId, CancellationToken cancellationToken);
    Task RegistrarEntradaAsync(string usuarioId, Guid empresaId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ActividadAcceso>> ConsultarActividadAsync(string usuarioId, Guid empresaId, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> ConsultarModulosHabilitadosAsync(string usuarioId, Guid empresaId, CancellationToken cancellationToken);
    Task<bool> ModuloPermitidoAsync(string usuarioId, Guid empresaId, string claveModulo, CancellationToken cancellationToken);
}
