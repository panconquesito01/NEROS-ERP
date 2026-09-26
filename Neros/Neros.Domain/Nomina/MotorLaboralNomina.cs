namespace Neros.Domain.Nomina;

/// <summary>Motor laboral; paquete Colombia pendiente de validacion (plan §53, fase 19).</summary>
public static class MotorLaboralNomina
{
    public const string VersionReglasGenerica = "NOMINA-GENERICO-1";

    public static (bool AplicaReglasLegalesColombia, string? Advertencia) Evaluar(string? paqueteLegalCodigo)
    {
        if (string.Equals(paqueteLegalCodigo, Legal.MotorPaqueteLegalColombia.CodigoPaquete, StringComparison.OrdinalIgnoreCase))
            return (true, null);
        return (false, "No aplica reglas legales de Colombia hasta contar con el paquete validado por especialista.");
    }

    public static string VersionReglasDesdePaquete(string? paqueteLegalCodigo, string? versionPaquete) =>
        AplicaReglasLegalesColombia(paqueteLegalCodigo)
            ? versionPaquete ?? "CO-LEGAL-SIN-VERSION"
            : VersionReglasGenerica;

    private static bool AplicaReglasLegalesColombia(string? paqueteLegalCodigo) =>
        string.Equals(paqueteLegalCodigo, Legal.MotorPaqueteLegalColombia.CodigoPaquete, StringComparison.OrdinalIgnoreCase);
}
