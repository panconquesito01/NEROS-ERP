namespace Neros.Domain.Globalizacion;

/// <summary>Fecha de negocio en zona IANA de la empresa (plan §12).</summary>
public static class FechasNegocio
{
    public static DateOnly DesdeInstanteUtc(DateTime instanteUtc, string zonaHorariaIana)
    {
        if (instanteUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("El instante debe estar en UTC.", nameof(instanteUtc));
        ArgumentException.ThrowIfNullOrWhiteSpace(zonaHorariaIana);
        var zona = TimeZoneInfo.FindSystemTimeZoneById(zonaHorariaIana.Trim());
        var local = TimeZoneInfo.ConvertTimeFromUtc(instanteUtc, zona);
        return DateOnly.FromDateTime(local);
    }

    public static DateOnly HoyEnZona(string zonaHorariaIana, TimeProvider reloj) =>
        DesdeInstanteUtc(reloj.GetUtcNow().UtcDateTime, zonaHorariaIana);
}
