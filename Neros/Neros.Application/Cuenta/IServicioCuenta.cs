using Neros.Application.Seguridad;
using Neros.Contracts.Autenticacion;
using Neros.Contracts.Cuenta;

namespace Neros.Application.Cuenta;

/// <summary>Resultado del cambio de clave: la sesion renovada o un error de <see cref="CodigosCuenta"/>.</summary>
public sealed record ResultadoCambioClave(AccesoConcedido? Acceso, ErrorOperacion? Error);

public interface IServicioCuenta
{
    Task<ResultadoCambioClave> CambiarClaveAsync(string usuarioId, Guid sesionActual, SolicitudCambioClave solicitud, ContextoCliente cliente, CancellationToken cancellationToken);
    Task<IReadOnlyList<SesionActiva>> ListarSesionesAsync(string usuarioId, Guid sesionActual, CancellationToken cancellationToken);
    Task<bool> CerrarSesionAsync(string usuarioId, Guid sesionId, ContextoCliente cliente, CancellationToken cancellationToken);
    Task<int> CerrarSesionesAsync(string usuarioId, Guid? excepto, ContextoCliente cliente, CancellationToken cancellationToken);
}

public enum EstadoAdministracion { Correcto, NoEncontrado, PropiaCuenta, RolInvalido, FueraEmpresa }

public interface IServicioAdministracionUsuarios
{
    Task<PaginaUsuarios> ListarAsync(string? buscar, int pagina, int tamano, CancellationToken cancellationToken);
    Task<DetalleUsuarioPlataforma?> ObtenerAsync(string usuarioId, CancellationToken cancellationToken);
    Task<(EstadoAdministracion Estado, string? ClaveTemporal)> RestablecerClaveAsync(string actorId, string usuarioId, ContextoCliente cliente, CancellationToken cancellationToken);
    Task<EstadoAdministracion> DesbloquearAsync(string actorId, string usuarioId, ContextoCliente cliente, CancellationToken cancellationToken);
    Task<EstadoAdministracion> ActualizarAsync(string actorId, string usuarioId, SolicitudActualizarUsuarioPlataforma solicitud, ContextoCliente cliente, CancellationToken cancellationToken);
    Task<EstadoAdministracion> CambiarEstadoAsync(string actorId, string usuarioId, bool activo, ContextoCliente cliente, CancellationToken cancellationToken);
    Task<EstadoAdministracion> EliminarPermanenteAsync(string actorId, string usuarioId, ContextoCliente cliente, CancellationToken cancellationToken);
}
