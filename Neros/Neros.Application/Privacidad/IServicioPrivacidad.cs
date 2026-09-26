using Neros.Application.Seguridad;
using Neros.Contracts.Privacidad;

namespace Neros.Application.Privacidad;

public interface IServicioPrivacidad
{
    Task<IReadOnlyList<DefinicionCookiePublica>> ListarCookiesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<DocumentoLegalPendiente>> ListarPendientesAsync(string usuarioId, CancellationToken cancellationToken);
    Task<DocumentoLegalPublicado?> ObtenerVigenteAsync(string codigo, CancellationToken cancellationToken);
    Task<(bool Correcto, string? Error)> AceptarAsync(string usuarioId, string codigo, SolicitudAceptacionLegal solicitud,
        ContextoCliente cliente, CancellationToken cancellationToken);
}
