using EDIBANK.Conf_Db_With_Entity;
using EDIBANK.Models.Users_EdiWeb;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EDIBANK.Controllers.AuthEmail;

/// <summary>
/// This class is used to confirm the user's email.
/// </summary>
public class EmailController(UserManager<AppUser> userManager, AppDbContext context) : Controller
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly AppDbContext _context = context;

    public async Task<IActionResult> ConfirmEmail(string token, string email) => View(await _userManager.FindByEmailAsync(email) switch
    {
        AppUser user when (await _userManager.ConfirmEmailAsync(user, token)).Succeeded => "ConfirmEmail",
        _ => "Error"
    });
}
