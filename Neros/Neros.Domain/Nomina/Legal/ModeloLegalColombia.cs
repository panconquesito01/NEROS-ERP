namespace Neros.Domain.Nomina.Legal;

public enum EstadoPaqueteLegalNomina
{
    Borrador,
    PendienteEspecialista,
    Aprobado
}

public sealed record ParametrosLegalesColombia(
    Guid PaqueteId,
    string CodigoPaquete,
    string VersionPaquete,
    DateOnly VigenciaDesde,
    DateOnly? VigenciaHasta,
    decimal Smmlv,
    decimal TarifaSaludEmpleadoPct,
    decimal TarifaPensionEmpleadoPct,
    decimal TarifaSaludEmpleadorPct,
    decimal TarifaPensionEmpleadorPct,
    decimal TarifaArlEmpleadorPct,
    decimal TarifaCajaEmpleadorPct,
    decimal UmbralRetencionFuente,
    decimal TarifaRetencionFuentePct,
    EstadoPaqueteLegalNomina EstadoAprobacion);

public sealed record ResultadoSeguridadSocialNomina(
    decimal BaseCotizacion,
    decimal AporteSaludEmpleado,
    decimal AportePensionEmpleado,
    decimal AporteSaludEmpleador,
    decimal AportePensionEmpleador,
    decimal AporteArlEmpleador,
    decimal AporteCajaEmpleador);

public sealed record ResultadoRetencionFuenteNomina(decimal BaseRetencion, decimal ImporteRetencion);

public sealed record ResultadoLegalColombiaNomina(
    ResultadoSeguridadSocialNomina SeguridadSocial,
    ResultadoRetencionFuenteNomina RetencionFuente,
    IReadOnlyList<LineaDeduccionLegal> DeduccionesEmpleado);

public sealed record LineaDeduccionLegal(
    string CodigoConcepto,
    string NombreConcepto,
    decimal BaseCalculo,
    decimal Tarifa,
    decimal Importe);
