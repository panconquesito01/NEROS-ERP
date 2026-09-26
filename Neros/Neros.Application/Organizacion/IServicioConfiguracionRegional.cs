using Neros.Contracts.Organizacion;

namespace Neros.Application.Organizacion;

public interface IServicioConfiguracionRegional
{
    Task<ConfiguracionRegionalEmpresa?> ObtenerAsync(string actorId, Guid empresaId, CancellationToken cancellationToken);
    Task<(ConfiguracionRegionalEmpresa? Configuracion, string? Error)> ActualizarAsync(
        string actorId, Guid empresaId, ConfiguracionRegionalEmpresa solicitud, CancellationToken cancellationToken);
}
