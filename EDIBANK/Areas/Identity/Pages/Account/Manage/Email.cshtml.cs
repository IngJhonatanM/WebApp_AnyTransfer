// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using EDIBANK.Models.Users_EdiWeb;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace EDIBANK.Areas.Identity.Pages.Account.Manage;

public class EmailModel(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailSender emailSender) : PageModel
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly SignInManager<AppUser> _signInManager = signInManager;
    private readonly IEmailSender _emailSender = emailSender;

    /// <summary>
    /// This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    /// directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    /// directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public bool IsEmailConfirmed { get; set; }

    /// <summary>
    /// This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    /// directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string StatusMessage { get; set; }

    /// <summary>
    /// This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    /// directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>
    /// This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    /// directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        /// This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        /// directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "New email")]
        public string NewEmail { get; set; }
    }

    private async Task LoadAsync(AppUser user)
    {
        string email = await _userManager.GetEmailAsync(user);

        Email = email;
        Input = new InputModel
        {
            NewEmail = email,
        };
        IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
    }

    public async Task<IActionResult> OnGetAsync()
    {
        AppUser user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }
        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostChangeEmailAsync()
    {
        AppUser user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }
        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        string newEmail = Input.NewEmail;

        // Actualizar el username con el nuevo email
        user.UserName = newEmail;
        if (newEmail != await _userManager.GetEmailAsync(user))
        {
            // Update the user's email without sending confirmation or verification email
            user.Email = newEmail;
            user.UserName = newEmail;
            IdentityResult result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                StatusMessage = "Email updated successfully.";
                return RedirectToPage();
            }

            // Handle errors if updating the email fails
            //AddErrors(result);
        }
        else
        {
            StatusMessage = "Your email is unchanged.";
        }
        return RedirectToPage();
    }
}
