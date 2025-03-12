// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using EDIBANK.Conf_Db_With_Entity;
using EDIBANK.Models.Monitor;
using EDIBANK.Models.Users_EdiWeb;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace EDIBANK.Areas.Identity.Pages.Account;

public class LoginModel(SignInManager<AppUser> signInManager, ILogger<LoginModel> logger) : PageModel
{
    private readonly SignInManager<AppUser> _signInManager = signInManager;
    private readonly ILogger<LoginModel> _logger = logger;

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
    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    /// <summary>
    /// This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    /// directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string ReturnUrl { get; set; }

    /// <summary>
    /// This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    /// directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string ErrorMessage { get; set; }

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
        [Required(ErrorMessage = "Por favor, ingrese su correo.")]
        [EmailAddress(ErrorMessage = "Correo no es válido.")]
        //[RegularExpression("""^[0-9A-Za-z\._-]+@([0-9A-Za-z-]+\.)+[A-Za-z]{2,6}$""")]
        // See: https://github.com/dotnet/runtime/issues/45670 and https://github.com/dotnet/runtime/issues/46580
        public string Email { get; set; }

        /// <summary>
        /// This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        /// directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required(ErrorMessage = "Por favor, ingrese su contraseña.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        /// directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Display(Name = "Recordar mis datos.")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }
        returnUrl ??= Url.Content("~/");

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        ExternalLogins = [.. await _signInManager.GetExternalAuthenticationSchemesAsync()];
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl, [FromServices] IConfiguration configuration, [FromServices] RoleManager<IdentityRole> roleManager, [FromServices] UserManager<AppUser> userManager, [FromServices] AppDbContext context)
    {
        await OneTimeSeedingAsync(configuration, roleManager, userManager, context);
        returnUrl ??= Url.Content("~/");
        ExternalLogins = [.. await _signInManager.GetExternalAuthenticationSchemesAsync()];
        if (ModelState.IsValid)
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return LocalRedirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, id = Input.Email, Input.RememberMe });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }
            ModelState.AddModelError(string.Empty, "Intento de inicio de sesión inválido, por favor revise sus datos.");
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }

    /// <summary>
    /// <para>
    /// Verifica la existencia de varias entradas en la base de datos, necesarias para el funcionamiento de la aplicación;
    /// de no existir, las mismas son creadas con valores pre-configurados:
    /// </para>
    /// <para>
    /// * Rol 'User' (Identity)
    /// </para>
    /// <para>
    /// * Rol 'Admin' (Identity)
    /// </para>
    /// <para>
    /// * Usuario inicial en rol 'Admin' (Identity) a partir de las claves 'AdminEmail', 'AdminDisplayName' y 'AdminPassword' en la sección de configuración 'OneTimeSeeding'.
    /// </para>
    /// <para>
    /// * Entrada única en la tabla 'Parametro', columnas 'ServiceEmail', 'ServiceDisplayName', 'SmtpHost', 'SmtpPort', 'SmtpPassword' a partir de las claves homónimas en la sección de configuración 'OneTimeSeeding'.
    /// </para>
    /// <para>
    /// Una vez creadas estas entradas en la base de datos, la sección de configuración 'OneTimeSeeding' debe ser redactada para remover información sensitiva.
    /// </para>
    /// </summary>
    private async Task OneTimeSeedingAsync(IConfiguration configuration, RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager, AppDbContext context)
    {
        IdentityResult result;

        if (!await roleManager.RoleExistsAsync("User"))
        {
            result = await roleManager.CreateAsync(new IdentityRole("User"));
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "No pudo crear rol 'User'.");
                ModelState.AddModelErrors(result);
            }
        }
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            result = await roleManager.CreateAsync(new IdentityRole("Admin"));
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "No pudo crear rol 'Admin'.");
                ModelState.AddModelErrors(result);
            }
        }

        IConfigurationSection ots = configuration.GetSection("OneTimeSeeding");

        if (await roleManager.RoleExistsAsync("Admin") && !(await userManager.GetUsersInRoleAsync("Admin")).Any())
        {
            if (!string.IsNullOrWhiteSpace(ots["AdminDisplayName"]) && MailAddress.TryCreate(ots["AdminEmail"], ots["AdminDisplayName"], out _))
            {
                if (!string.IsNullOrWhiteSpace(ots["AdminPassword"]))
                {
                    AppUser adminUser = new()
                    {
                        UserName = ots["AdminEmail"],
                        Email = ots["AdminEmail"],
                        TwoFactorEnabled = true,
                        EmailConfirmed = true,
                        LockoutEnabled = false,
                        DisplayName = ots["AdminDisplayName"],
                        EDIId = (await context.EDIs.DefaultAsync()).Id
                    };

                    result = await userManager.CreateAsync(adminUser, ots["AdminPassword"]);
                    if (result.Succeeded)
                    {
                        result = await userManager.AddToRoleAsync(adminUser, "Admin");
                        if (!result.Succeeded)
                        {
                            ModelState.AddModelError(string.Empty, "No pudo añadir primer administrador al rol 'Admin'.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "No pudo crear primer administrador.");
                    }
                    ModelState.AddModelErrors(result);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Contraseña de primer administrador vacía o inválida (OneTimeSeeding.AdminPassword).");
                }
            }
            else if (string.IsNullOrWhiteSpace(ots["AdminDisplayName"]))
            {
                ModelState.AddModelError(string.Empty, "Nombre de primer administrador vacío o inválido (OneTimeSeeding.AdminDisplayName).");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Correo de primer administrador vacío o inválido (OneTimeSeeding.AdminEmail).");
            }
        }
        if (!await context.Parametros.AnyAsync())
        {
            if (!string.IsNullOrWhiteSpace(ots["ServiceDisplayName"]) && MailAddress.TryCreate(ots["ServiceEmail"], ots["ServiceDisplayName"], out _))
            {
                if (!string.IsNullOrWhiteSpace(ots["SmtpHost"]))
                {
                    if (int.TryParse(ots["SmtpPort"], out int otsSMTPPort))
                    {
                        if (!string.IsNullOrWhiteSpace(ots["SmtpPassword"]))
                        {
                            try
                            {
                                context.Add(new Parametro
                                {
                                    ServiceEmail = ots["ServiceEmail"],
                                    ServiceDisplayName = ots["ServiceDisplayName"],
                                    SmtpHost = ots["SmtpHost"],
                                    SmtpPort = otsSMTPPort,
                                    SmtpPassword = ots["SmtpPassword"]
                                });
                                await context.SaveChangesAsync();
                            }
                            catch (Exception e)
                            {
                                if (!await context.Parametros.AnyAsync())
                                {
                                    ModelState.AddModelError(string.Empty, "No pudo crear registro de parámetros.");
                                }
                                ModelState.AddModelError(string.Empty, e.Message);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Contraseña SMTP vacía o inválida (OneTimeSeeding.SmtpPassword).");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Puerto SMTP vacío o inválido (OneTimeSeeding.SmtpPort).");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Anfitrión SMTP vacío o inválido (OneTimeSeeding.SmtpHost).");
                }
            }
            else if (string.IsNullOrWhiteSpace(ots["ServiceDisplayName"]))
            {
                ModelState.AddModelError(string.Empty, "Nombre de servicio vacío o inválido (OneTimeSeeding.ServiceDisplayName).");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Correo de servicio vacío o inválido (OneTimeSeeding.ServiceEmail).");
            }
        }
    }
}