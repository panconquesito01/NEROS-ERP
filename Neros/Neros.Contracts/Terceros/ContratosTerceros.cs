namespace Neros.Contracts.Terceros;

public static class TercerosAudiencias
{
    public const string Api = "neros.terceros";
}

public static class PermisosTerceros
{
    public const string Consultar = "Terceros.Party.Read";
    public const string Escribir = "Terceros.Party.Write";
    public const string CuentaBancariaConsultar = "Terceros.BankAccount.Read";
}

public sealed record IdentificacionTercero(string Pais, string Tipo, string Numero, char? DigitoVerificacion, bool EsPrincipal);

public sealed record TerceroResumen(
    Guid Id, string Tipo, string RazonSocial, string? IdentificacionPrincipal, IReadOnlyList<string> Roles, bool Activo);

public sealed record TerceroDetalle(
    Guid Id, string Tipo, string RazonSocial, string? NombreComercial, bool Activo,
    IReadOnlyList<IdentificacionTercero> Identificaciones, IReadOnlyList<string> Roles);

public sealed record SolicitudCrearTercero(
    string Tipo, string RazonSocial, string? NombreComercial,
    IReadOnlyList<IdentificacionTercero> Identificaciones, IReadOnlyList<string> Roles);

public sealed record SolicitudActualizarTercero(
    string RazonSocial, string? NombreComercial, bool Activo,
    IReadOnlyList<IdentificacionTercero> Identificaciones, IReadOnlyList<string> Roles);

public sealed record PaginaTerceros(IReadOnlyList<TerceroResumen> Elementos, int Total, int Pagina, int TamanoPagina);

public sealed record VersionHistoricaTercero(DateTimeOffset ValidoDesde, DateTimeOffset ValidoHasta, string RazonSocial, bool Activo);

public sealed record ErrorValidacionTercero(string Codigo, string Mensaje);
