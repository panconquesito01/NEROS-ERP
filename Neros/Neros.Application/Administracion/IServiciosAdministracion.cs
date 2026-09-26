using Neros.Application.Cuenta;
using Neros.Application.Seguridad;
using Neros.Contracts.Administracion;
using Neros.Contracts.Cuenta;

namespace Neros.Application.Administracion;

public interface IServicioAdministracionPlataforma
{
    Task<PaginaEmpresas> ListarEmpresasAsync(string? buscar, int pagina, int tamano, CancellationToken cancellationToken);
    Task<(EmpresaAdministrada? Empresa, string? Error)> CrearEmpresaAsync(
        string actorId, SolicitudCrearEmpresa solicitud, ContextoCliente cliente, CancellationToken cancellationToken);
    Task<(bool Correcto, string? Error)> ActualizarLogoAsync(
        Guid empresaId, byte[] contenido, string contentType, ContextoCliente cliente, CancellationToken cancellationToken);
    Task<(UsuarioCreado? Usuario, string? Error)> CrearUsuarioAsync(
        string actorId, SolicitudCrearUsuarioPlataforma solicitud,
        ContextoCliente cliente, CancellationToken cancellationToken);
}

public interface IServicioAdministracionEmpresa
{
    Task<PaginaMiembrosEmpresa> ListarMiembrosAsync(
        string actorId, Guid empresaId, string? buscar, int pagina, int tamano, CancellationToken cancellationToken);
    Task<(UsuarioCreado? Usuario, string? Error)> CrearMiembroAsync(
        string actorId, Guid empresaId, SolicitudCrearMiembroEmpresa solicitud,
        ContextoCliente cliente, CancellationToken cancellationToken);
    Task<(EstadoAdministracion Estado, string? ClaveTemporal)> RestablecerClaveAsync(
        string actorId, Guid empresaId, string usuarioId, ContextoCliente cliente, CancellationToken cancellationToken);
    Task<EstadoAdministracion> DesbloquearAsync(
        string actorId, Guid empresaId, string usuarioId, ContextoCliente cliente, CancellationToken cancellationToken);
}
