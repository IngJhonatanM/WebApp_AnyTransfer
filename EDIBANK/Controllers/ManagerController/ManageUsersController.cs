using EDIBANK.Conf_Db_With_Entity;
using EDIBANK.Models.Users_EdiWeb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;

namespace EDIBANK.Controllers.ManagerController;

// Instance of the ASP.NET Core Identity UserManager available in the controller
[Authorize(Roles = "Admin")]
public class ManageUsersController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context) : Controller
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly AppDbContext _context = context;

    [HttpGet]
    public async Task<IActionResult> Index(string? success, string? failure) => View(new ManageUsersViewModel
    {
        Success = success,
        Failure = failure,
        AppUser = (await _userManager.GetUserAsync(User))!,
        AdminRole = await _userManager.GetUsersInRoleAsync("Admin"),
        UserRole = await _userManager.GetUsersInRoleAsync("User")
    });

    // Create User
    [HttpGet]
    public async Task<IActionResult> Create() => View(new UserViewModel
    {
        Email = string.Empty,
        Password = string.Empty,
        DisplayName = string.Empty,
        RoleName = (await _roleManager.Roles.DefaultAsync()).Name!,
        EDIId = (await _context.EDIs.DefaultAsync()).Id
    });

    [HttpPost]
    public async Task<IActionResult> Create(UserViewModel uVM)
    {
        if (!string.IsNullOrWhiteSpace(uVM.DisplayName) && MailAddress.TryCreate(uVM.Email, uVM.DisplayName, out _))
        {
            if (!string.IsNullOrWhiteSpace(uVM.Password))
            {
                AppUser appUser = new()
                {
                    UserName = uVM.Email,
                    Email = uVM.Email,
                    EmailConfirmed = true,
                    TwoFactorEnabled = true,
                    LockoutEnabled = uVM.RoleName is not "Admin",
                    DisplayName = uVM.DisplayName,
                    EDIId = uVM.EDIId
                };
                IdentityResult result = await _userManager.CreateAsync(appUser, uVM.Password);

                if (result.Succeeded)
                {
                    // Set the appUser role
                    result = await _userManager.AddToRoleAsync(appUser, uVM.RoleName);
                    if (result.Succeeded)
                    {
                        //var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
                        //var confirmationLink = Url.Action("ConfirmEmail", "Email", new { token, email = appUser.Email }, Request.Scheme);
                        //EmailHelper emailHelper = new EmailHelper();
                        //bool emailResponse = emailHelper.SendEmail(appUser.Email, confirmationLink);

                        //if (emailResponse)
                        //{
                        //    return RedirectToAction("Index");
                        //}
                        //else
                        //{
                        //    //log email failed 
                        //}
                        return RedirectToAction("Index", new { Message = "Usuario correctamente creado." });
                    }
                    ModelState.AddModelError(string.Empty, $"No se pudo añadir nuevo usuario al rol '{uVM.RoleName}'.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No se pudo crear nuevo usuario.");
                }
                ModelState.AddModelErrors(result);
            }
            else
            {
                ModelState.AddModelError(nameof(uVM.Password), "Contraseña vacía o inválida.");
            }
        }
        else if (string.IsNullOrWhiteSpace(uVM.DisplayName))
        {
            ModelState.AddModelError(nameof(uVM.DisplayName), "Nombre vacío o inválido.");
        }
        else
        {
            ModelState.AddModelError(nameof(uVM.Email), "Correo vacío o inválido.");
        }
        return View(uVM);
    }

    // Update User
    [HttpGet]
    public async Task<IActionResult> Update(string id) => await _userManager.FindByIdAsync(id) switch
    {
        AppUser appUser => View(appUser),
        _ => RedirectToAction("Index", new { Failure = "Usuario no encontrado." })
    };

    [HttpPost]
    public async Task<IActionResult> Update(string id, string? email, string? displayName, string? password, [FromServices] IPasswordHasher<AppUser> passwordHasher)
    {
        switch (await _userManager.FindByIdAsync(id))
        {
            case AppUser appUser:
                switch ((email?.Trim(), displayName?.Trim(), password))
                {
                    case (string em, string dn, string pw) when (em, dn) != (appUser.Email, appUser.DisplayName) && MailAddress.TryCreate(em, dn, out _):
                        appUser.Email = appUser.UserName = em;
                        appUser.DisplayName = dn;
                        appUser.PasswordHash = passwordHasher.HashPassword(appUser, pw);
                        break;
                    case (string em, string dn, _) when (em, dn) != (appUser.Email, appUser.DisplayName) && MailAddress.TryCreate(em, dn, out _):
                        appUser.Email = appUser.UserName = em;
                        appUser.DisplayName = dn;
                        break;
                    case (_, _, string pw):
                        appUser.PasswordHash = passwordHasher.HashPassword(appUser, pw);
                        break;
                    case (_, _, _):
                        return RedirectToAction("Index", new { Success = "Usuario sin cambios." });
                }
                switch (await _userManager.UpdateAsync(appUser))
                {
                    case IdentityResult result when !result.Succeeded:
                        ModelState.AddModelErrors(result);
                        return View(appUser);
                    default:
                        return RedirectToAction("Index", new { Success = "Usuario correctamente modificado." });
                }
            default:
                return RedirectToAction("Index", new { Failure = "Usuario no encontrado." });
        }
    }

    // Unblock User
    [HttpPost]
    public async Task<IActionResult> UnblockUser(string id)
    {
        switch (await _userManager.FindByIdAsync(id))
        {
            case AppUser appUser when await _userManager.IsLockedOutAsync(appUser):
                // Si el usuario está bloqueado, desbloquearlo
                //await _userManager.SetLockoutEnabledAsync(appUser, false);
                await _userManager.ResetAccessFailedCountAsync(appUser);

                // Volver a habilitar el bloqueo para futuros intentos fallidos
                await _userManager.SetLockoutEndDateAsync(appUser, DateTime.Now.AddMinutes(-1.0));

                // Actualizar la última fecha de inicio de sesión para reiniciar el temporizador de bloqueo
                await _userManager.UpdateSecurityStampAsync(appUser);
                return RedirectToAction("Index", new { Success = "Usuario correctamente desbloqueado." });
            case AppUser:
                return RedirectToAction("Index", new { Success = "Usuario no estaba bloqueado." });
            default:
                return RedirectToAction("Index", new { Failure = "Usuario no encontrado." });
        }
    }

    // Delete User
    [HttpPost]
    public async Task<IActionResult> Delete(string id) => RedirectToAction("Index", await _userManager.FindByIdAsync(id) switch
    {
        AppUser appUser when await _userManager.DeleteAsync(appUser) is IdentityResult result && !result.Succeeded => new { Failure = $"Usuario no eliminado: {result}." },
        AppUser => new { Success = "Usuario correctamente eliminado." },
        _ => new { Failure = "Usuario no encontrado." }
    });
}
