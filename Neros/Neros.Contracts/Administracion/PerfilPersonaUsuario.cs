using System.ComponentModel.DataAnnotations;

namespace Neros.Contracts.Administracion;

public sealed class PerfilPersonaUsuario
{
    [Required, StringLength(80, MinimumLength = 2)]
    public string PrimerNombre { get; set; } = string.Empty;

    [StringLength(80)]
    public string? SegundoNombre { get; set; }

    [Required, StringLength(80, MinimumLength = 2)]
    public string PrimerApellido { get; set; } = string.Empty;

    [StringLength(80)]
    public string? SegundoApellido { get; set; }

    [Required, StringLength(10, MinimumLength = 2)]
    public string TipoDocumento { get; set; } = "CC";

    [Required, StringLength(30, MinimumLength = 3)]
    public string NumeroDocumento { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Ciudad { get; set; }

    [StringLength(256)]
    public string? Direccion { get; set; }

    public string NombreCompleto() =>
        string.Join(" ", new[] { PrimerNombre, SegundoNombre, PrimerApellido, SegundoApellido }
            .Where(parte => !string.IsNullOrWhiteSpace(parte)));
}
