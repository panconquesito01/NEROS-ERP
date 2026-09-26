using Neros.Contracts.Seguridad;

namespace Neros.Contracts.Busqueda;

public static class PuentePermisosBusqueda
{
    public static IEnumerable<string> DesdeCodigosEmpresa(IEnumerable<string> codigos)
    {
        foreach (var codigo in codigos)
            if (codigo == CodigosPermiso.BusquedaIndiceConsultar)
                yield return PermisosBusqueda.Consultar;
    }

    public static string AlcanceDesdeCodigos(IEnumerable<string> codigos) =>
        codigos.Any(c => c == CodigosPermiso.BusquedaIndiceConsultar) ? "search.read" : "";
}
