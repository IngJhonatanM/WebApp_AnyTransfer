using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDIBANK.Models.Users_EdiWeb;

public class AppUser : IdentityUser
{
    [Display(Name = "Nombre"), Column(TypeName = "varchar(256)")]
    public required string DisplayName { get; set; }

    [Display(Name = "EDI asociado"), Column(TypeName = "varchar(15)"), Unicode(false)]
    public required string EDIId { get; init; }
}
