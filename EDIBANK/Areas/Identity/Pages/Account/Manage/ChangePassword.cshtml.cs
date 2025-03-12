// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using EDIBANK.Models.Users_EdiWeb;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace EDIBANK.Areas.Identity.Pages.Account.Manage;

public class ChangePasswordModel(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<ChangePasswordModel> logger) : PageModel
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly SignInManager<AppUser> _signInManager = signInManager;
    private readonly ILogger<ChangePasswordModel> _logger = logger;

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string StatusMessage { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required(ErrorMessage = "Por favor, ingresar tu contraseña actual. ")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string OldPassword { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required(ErrorMessage = "Por favor, ingresar tu nueva contraseña. ")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NewPassword { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [Compare("NewPassword", ErrorMessage = "La nueva contraseña y la contraseña de confirmación no coinciden.")]
        public string ConfirmPassword { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        AppUser user = await _userManager.GetUserAsync(User);

        return user is null
            ? NotFound($"Unable to load appUser with ID '{_userManager.GetUserId(User)}'.")
            : await _userManager.HasPasswordAsync(user)
                ? Page()
                : RedirectToPage("./SetPassword");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        if (await _userManager.GetUserAsync(User) is not AppUser appUser)
        {
            return NotFound($"Unable to load appUser with ID '{_userManager.GetUserId(User)}'.");
        }

        IdentityResult result = await _userManager.ChangePasswordAsync(appUser, Input.OldPassword, Input.NewPassword);

        if (!result.Succeeded)
        {
            ModelState.AddModelErrors(result);
            return Page();
        }
        await _signInManager.RefreshSignInAsync(appUser);
        _logger.LogInformation("El usuario cambió su contraseña exitosamente.");
        StatusMessage = "Tu contraseña ha sido cambiada.";
        return RedirectToPage();
    }
}
