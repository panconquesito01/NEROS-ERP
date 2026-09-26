using Neros.Contracts.Globalizacion;

namespace Neros.Application.Globalizacion;

public interface IServicioCatalogosGlobalizacion
{
    Task<CatalogosTransversales> ObtenerAsync(string? paisTiposIdentificacion, CancellationToken cancellationToken);
}
