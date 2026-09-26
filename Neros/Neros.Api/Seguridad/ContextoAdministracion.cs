using Neros.Application.Seguridad;
using Neros.Contracts.Autenticacion;
using Neros.Contracts.Seguridad;

namespace Neros.Api.Seguridad;

public static class ContextoAdministracion
{
    public static bool EsAdministradorPlataforma(this HttpContext context) =>
        context.User.HasClaim(CodigosPermiso.Claim, CodigosPermiso.UsuarioConsultar);

    public static bool PuedeAdministrarEmpresa(this HttpContext context) =>
        context.User.HasClaim(CodigosPermiso.Claim, CodigosPermiso.EmpresaMiembroAdministrar)
        || context.EsAdministradorPlataforma();

    public static Guid? EmpresaActiva(this HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CabecerasCliente.Empresa, out var valor)
            && Guid.TryParse(valor.ToString(), out var id))
            return id;
        return null;
    }

    public static Guid EmpresaActivaRequerida(this HttpContext context)
    {
        var id = context.EmpresaActiva();
        if (id is null) throw new InvalidOperationException("Falta cabecera X-Neros-Company-Id.");
        return id.Value;
    }

    public static bool CoincideEmpresaActiva(this HttpContext context, Guid empresaId) =>
        context.EmpresaActiva() is { } activa && activa == empresaId;
}
