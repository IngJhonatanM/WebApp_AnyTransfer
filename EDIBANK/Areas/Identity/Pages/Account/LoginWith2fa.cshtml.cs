// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using EDIBANK.Conf_Db_With_Entity;
using EDIBANK.Models.Users_EdiWeb;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace EDIBANK.Areas.Identity.Pages.Account;

public class LoginWith2faModel(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, ILogger<LoginWith2faModel> logger) : PageModel
{
    private readonly SignInManager<AppUser> _signInManager = signInManager;
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly ILogger<LoginWith2faModel> _logger = logger;

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
    public bool RememberMe { get; set; }

    /// <summary>
    /// This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    /// directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string ReturnUrl { get; set; }

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
        [Required(ErrorMessage = "Por favor, ingresar token.")]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Código de autenticación")]
        public string TwoFactorCode { get; set; }

        /// <summary>
        /// This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        /// directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Display(Name = "Remember this machine")]
        public bool RememberMachine { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(bool rememberMe, [FromServices] AppDbContext context, string returnUrl = null)
    {
        // Ensure the user has gone through the username & password screen first
        AppUser appUser = await _signInManager.GetTwoFactorAuthenticationUserAsync() ?? throw new InvalidOperationException("Unable to load two-factor authentication user.");
        IList<string> providers = await _userManager.GetValidTwoFactorProvidersAsync(appUser);

        if (providers.Any(static bool (string s) => s is "Email"))
        {
            // Generate the 2fa token
            string token = await _userManager.GenerateTwoFactorTokenAsync(appUser, "Email");

            // Send the user the 2fa token via email
            await new Helper<LoginWith2faModel>(context, appUser, HttpContext).SendTwoFactorCodeEmailAsync(token);
        }
        ReturnUrl = returnUrl;
        RememberMe = rememberMe;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            //returnUrl ??= Url.Content("~/");

            AppUser user = await _signInManager.GetTwoFactorAuthenticationUserAsync() ?? throw new InvalidOperationException("Unable to load two-factor authentication user.");
            string authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty)
                                                          .Replace("-", string.Empty);
            //Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, Input.RememberMachine);
            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.TwoFactorSignInAsync("Email", authenticatorCode, false, false);
            //string userId = await _userManager.GetUserIdAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
                //return LocalRedirect(returnUrl);
                return LocalRedirect("~/Monitor/Intercambios");
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return RedirectToPage("./Lockout");
            }
            _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
            ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
        }
        return Page();
    }
}
