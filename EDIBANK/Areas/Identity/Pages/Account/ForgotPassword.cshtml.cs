// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using EDIBANK.Conf_Db_With_Entity;
using EDIBANK.Models.Users_EdiWeb;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EDIBANK.Areas.Identity.Pages.Account;

public class ForgotPasswordModel(UserManager<AppUser> userManager, SignInManager<AppUser> signinMgr) : PageModel
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly SignInManager<AppUser> _signInManager = signinMgr;

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
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public async Task<IActionResult> OnPostAsync([FromServices] AppDbContext context)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        if (await _userManager.FindByEmailAsync(Input.Email) is AppUser appUser && await _userManager.IsEmailConfirmedAsync(appUser))
        {
            // For more information on how to enable account confirmation and password reset please
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            string code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await _userManager.GeneratePasswordResetTokenAsync(appUser)));
            string link = Url.Page("/Account/ResetPassword", "ResetPassword", new { code, email = appUser.Email }, Request.Scheme);

            await new Helper<ForgotPasswordModel>(context, appUser, HttpContext).SendPasswordResetEmailAsync(link);

            // "Reset Password",
            //  $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(link)}'>clicking here</a>.");
        }

        // Don't reveal that the user does not exist or is not confirmed
        return RedirectToPage("./ForgotPasswordConfirmation");
    }
}
