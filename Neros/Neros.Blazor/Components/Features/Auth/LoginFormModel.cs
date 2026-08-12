using System.ComponentModel.DataAnnotations;

namespace Neros.Blazor.Components.Features.Auth;

public sealed class LoginFormModel
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo valido.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "La contrasena es obligatoria.")]
    [MinLength(6, ErrorMessage = "La contrasena debe tener al menos 6 caracteres.")]
    public string? Password { get; set; }

    public bool RememberMe { get; set; }
}