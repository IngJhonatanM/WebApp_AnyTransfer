// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using EDIBANK.Models.Users_EdiWeb;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;

namespace EDIBANK.Areas.Identity.Pages.Account.Manage;

public class EnableAuthenticatorModel(UserManager<AppUser> userManager, ILogger<EnableAuthenticatorModel> logger, UrlEncoder urlEncoder) : PageModel
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly ILogger<EnableAuthenticatorModel> _logger = logger;
    private readonly UrlEncoder _urlEncoder = urlEncoder;

    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string SharedKey { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string AuthenticatorUri { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string[] RecoveryCodes { get; set; }

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
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Verification Code")]
        public string Code { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        AppUser user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }
        await LoadSharedKeyAndQrCodeUriAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        AppUser user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }
        if (!ModelState.IsValid)
        {
            await LoadSharedKeyAndQrCodeUriAsync(user);
            return Page();
        }

        // Strip spaces and hyphens
        string verificationCode = Input.Code.Replace(" ", string.Empty)
                                            .Replace("-", string.Empty);
        bool is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(user: user,
                                                                            tokenProvider: _userManager.Options.Tokens.AuthenticatorTokenProvider,
                                                                            token: verificationCode);

        if (!is2faTokenValid)
        {
            ModelState.AddModelError("Input.Code", "Verification code is invalid.");
            await LoadSharedKeyAndQrCodeUriAsync(user);
            return Page();
        }
        await _userManager.SetTwoFactorEnabledAsync(user, true);

        string userId = await _userManager.GetUserIdAsync(user);

        _logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", userId);
        StatusMessage = "Your authenticator app has been verified.";
        if (await _userManager.CountRecoveryCodesAsync(user) is 0)
        {
            IEnumerable<string> recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

            RecoveryCodes = recoveryCodes.ToArray();
            return RedirectToPage("./ShowRecoveryCodes");
        }
        return RedirectToPage("./TwoFactorAuthentication");
    }

    private async Task LoadSharedKeyAndQrCodeUriAsync(AppUser user)
    {
        // Load the authenticator key & QR code URI to display on the form
        string unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);

        if (unformattedKey is null or [])
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }
        SharedKey = FormatKey(unformattedKey);

        string email = await _userManager.GetEmailAsync(user);

        AuthenticatorUri = GenerateQrCodeUri(email, unformattedKey);
    }

    private string FormatKey(string unformattedKey)
    {
        StringBuilder result = new();
        int currentPosition = 0;

        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }
        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string unformattedKey) => string.Format(provider: CultureInfo.InvariantCulture,
                                                                                           format: AuthenticatorUriFormat,
                                                                                           arg0: _urlEncoder.Encode("Microsoft.AspNetCore.Identity.UI"),
                                                                                           arg1: _urlEncoder.Encode(email),
                                                                                           arg2: unformattedKey);
}
