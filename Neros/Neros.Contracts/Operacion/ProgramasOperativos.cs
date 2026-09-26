namespace Neros.Contracts.Operacion;

public sealed record FilaProgramaOperativo(
    Guid Id,
    string? Numero,
    string Estado,
    DateOnly? Fecha,
    string? Referencia,
    decimal? Total);

public sealed record PaginaProgramaOperativo(
    IReadOnlyList<FilaProgramaOperativo> Filas,
    int Total,
    bool ModuloDisponible,
    string? AvisoModulo);

public sealed record BorradorProgramaCreado(string Id, string? Numero);

public sealed record LineaProgramaOperativo(
    Guid Id,
    int LineaNumero,
    string Descripcion,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal TotalLinea);

public sealed record DetalleProgramaOperativo(
    string Programa,
    Guid Id,
    IReadOnlyDictionary<string, string?> Campos,
    IReadOnlyList<LineaProgramaOperativo> Lineas,
    bool Editable,
    IReadOnlyList<string> AccionesDisponibles);

public sealed class SolicitudGuardarPrograma
{
    public Dictionary<string, string?> Campos { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class SolicitudLineaPrograma
{
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; } = 1;
    public decimal PrecioUnitario { get; set; }
}

public static class CodigosProgramaOperativo
{
    public const string ModuloNoConfigurado = "modulo_no_configurado";
    public const string ProgramaDesconocido = "programa_desconocido";
    public const string SinPermiso = "sin_permiso";
    public const string NoEncontrado = "no_encontrado";
    public const string NoEditable = "no_editable";
}

public static class AccionesPrograma
{
    public const string Confirmar = "confirmar";
    public const string Anular = "anular";
}
