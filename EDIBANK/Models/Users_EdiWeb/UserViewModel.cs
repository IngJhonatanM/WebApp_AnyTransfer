using System.ComponentModel.DataAnnotations;

namespace EDIBANK.Models.Users_EdiWeb;

public class UserViewModel
{
    [Display(Name = "Nombre")]
    public required string? DisplayName { get; init; }

    [Display(Name = "Correo")]
    public required string? Email { get; init; }

    [Display(Name = "Contraseña")]
    public required string? Password { get; init; }

    [Display(Name = "Rol asociado")]
    public required string RoleName { get; init; }

    [Display(Name = "EDI asociado")]
    public required string EDIId { get; init; }
}
