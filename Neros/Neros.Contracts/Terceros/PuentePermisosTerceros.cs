using Neros.Contracts.Seguridad;

namespace Neros.Contracts.Terceros;

public static class PuentePermisosTerceros
{
    public static IEnumerable<string> DesdeCodigosEmpresa(IEnumerable<string> codigos)
    {
        foreach (var codigo in codigos)
        {
            if (codigo == CodigosPermiso.TerceroConsultar) yield return PermisosTerceros.Consultar;
            if (codigo is CodigosPermiso.TerceroCrear or CodigosPermiso.TerceroModificar) yield return PermisosTerceros.Escribir;
            if (codigo == CodigosPermiso.TerceroCuentaBancariaConsultar) yield return PermisosTerceros.CuentaBancariaConsultar;
        }
    }

    public static string AlcancesDesdeCodigos(IEnumerable<string> codigos)
    {
        var lista = codigos.ToList();
        var alcances = new List<string>();
        if (lista.Contains(CodigosPermiso.TerceroConsultar)) alcances.Add("terceros.read");
        if (lista.Any(c => c is CodigosPermiso.TerceroCrear or CodigosPermiso.TerceroModificar)) alcances.Add("terceros.write");
        return string.Join(' ', alcances);
    }
}
