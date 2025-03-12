using EDIBANK.Models.Monitor;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EDIBANK;

internal static class Extensions
{
    /// <summary>
    /// Añade errores provenientes de una operación de Identity al campo de resumen de validación (asp-validation-summary)
    /// </summary>
    public static void AddModelErrors(this ModelStateDictionary modelState, IdentityResult result)
    {
        foreach (IdentityError error in result.Errors)
        {
            modelState.AddModelError(string.Empty, error.Code switch
            {
                "DuplicateEmail" or "DuplicateUserName" or "LoginAlreadyAssociated" => "Un usuario con este correo ya fué definido.",
                "PasswordRequiresDigit" => "La contraseña debe contener dígitos.",
                "PasswordRequiresLower" => "La contraseña debe contener minúsculas.",
                "PasswordRequiresNonAlphanumeric" => "La contraseña debe contener símbolos.",
                "PasswordRequiresUpper" => "La contraseña debe contener mayúsculas.",
                "PasswordTooShort" => "La contraseña debe tener al menos 8 caracteres.",
                _ => $"Error '{error.Code}'."
            });
        }
    }

    public static async Task<SelectList> SelectListAsync(this IQueryable<EDI> edis, string selectedValue) => new(items: await edis.Ordered().ToListAsync(),
                                                                                                                 dataValueField: nameof(EDI.Id),
                                                                                                                 dataTextField: nameof(EDI.Descripcion),
                                                                                                                 selectedValue: selectedValue);

    public static async Task<SelectList> SelectListAsync(this IQueryable<IdentityRole> roles, string selectedValue) => new(items: await roles.Ordered().ToListAsync(),
                                                                                                                           dataValueField: nameof(IdentityRole.Name),
                                                                                                                           dataTextField: nameof(IdentityRole.Name),
                                                                                                                           selectedValue: selectedValue);

    public static async Task<EDI> DefaultAsync(this IQueryable<EDI> edis) => await edis.Ordered().FirstAsync();

    public static async Task<IdentityRole> DefaultAsync(this IQueryable<IdentityRole> roles) => await roles.Ordered().FirstAsync();

    private static IOrderedQueryable<EDI> Ordered(this IQueryable<EDI> edis) => from e in edis.AsNoTracking()
                                                                                orderby e.Descripcion
                                                                                select e;

    private static IOrderedQueryable<IdentityRole> Ordered(this IQueryable<IdentityRole> roles) => from r in roles.AsNoTracking()
                                                                                                   orderby r.Name descending
                                                                                                   select r;

    public static string Truncar(this string str, int max)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(max);

        char[] arr = str.ToCharArray();
        int len = arr.Length;

        if (len is 0 || Encoding.UTF8.GetByteCount(arr, 0, len) <= max)
        {
            return str;
        }
        do
        {
            arr[len - 1] = '…';
        }
        while (0 < len && max < Encoding.UTF8.GetByteCount(arr, 0, len--));
        return new string(arr, 0, ++len);
    }
}
