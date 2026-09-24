using Neros.Contracts.Autenticacion;

namespace Neros.Application.Autenticacion;

public sealed class ServicioEmpresas(IRepositorioEmpresas empresas)
{
    public Task<IReadOnlyList<EmpresaDisponible>> ListarAsync(string usuarioId, CancellationToken cancellationToken) =>
        empresas.ListarAsync(usuarioId, cancellationToken);

    public async Task<EmpresaDisponible?> SeleccionarAsync(string usuarioId, Guid empresaId, CancellationToken cancellationToken)
    {
        var empresa = await empresas.ObtenerAutorizadaAsync(usuarioId, empresaId, cancellationToken);
        if (empresa is null)
        {
            return null;
        }

        await empresas.RegistrarEntradaAsync(usuarioId, empresaId, cancellationToken);
        return empresa;
    }

    public async Task<InicioEmpresa?> ObtenerInicioAsync(string usuarioId, Guid empresaId, CancellationToken cancellationToken)
    {
        var empresa = await empresas.ObtenerAutorizadaAsync(usuarioId, empresaId, cancellationToken);
        if (empresa is null)
        {
            return null;
        }

        var actividad = await empresas.ConsultarActividadAsync(usuarioId, empresaId, cancellationToken);
        return new InicioEmpresa(empresa, actividad);
    }
}