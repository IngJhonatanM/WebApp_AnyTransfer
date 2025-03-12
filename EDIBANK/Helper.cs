using EDIBANK.Conf_Db_With_Entity;
using EDIBANK.Models.Monitor;
using EDIBANK.Models.Users_EdiWeb;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;

namespace EDIBANK;

public class Helper<T>(AppDbContext appDbContext, AppUser appUser, HttpContext httpContext)
{
    private readonly AppDbContext _appDbContext = appDbContext;
    private readonly string _userName = appUser.UserName!.Truncar(192);
    private readonly string _email = appUser.Email!;
    private readonly string _displayName = appUser.DisplayName;
    private readonly string _remoteIP = httpContext.Connection.RemoteIpAddress switch
    {
        IPAddress ip when ip.IsIPv4MappedToIPv6 => $"{ip.MapToIPv4()}",
        IPAddress ip => $"{ip}",
        _ => string.Empty
    };
    private readonly string _modulo = typeof(T).FullName!.Truncar(192);

#if THIS_CODE_IS_NEEDED
    // Methods for confirm email
    public async Task<bool> SendConfirmEmailAsync(string link) => await SendEmailAsync("Confirme su correo", link, true);
#endif

    // Methods for login with 2FA
    public async Task<bool> SendTwoFactorCodeEmailAsync(string token) => await SendEmailAsync("Código de autenticación de dos factores", $"""
        <html>
        <head>
        </head>
        <body>
        <p>Hola, este es su código de autenticación de dos factores para su cuenta ANYTRANSFER:</p>
        <p style="font-weight: bold; font-size: 18px;">{token}</p>
        <p>Ingrese este código en la página de inicio de sesión para completar su autenticación.</p>
        <p>Para su seguridad, este código es válido por tiempo limitado.</p>
        <p>Si no solicitó este código, ignore este correo.</p>
        <p>Atentamente,</p>
        <p>Eniac Corporation.</p>
        </body>
        </html>
        """, true);

    // Methods for forgotten password
    public async Task<bool> SendPasswordResetEmailAsync(string link) => await SendEmailAsync("Reinicio de contraseña", link, true);

    private async Task<bool> SendEmailAsync(string subject, string body, bool isBodyHtml, [CallerMemberName] string memberName = "*DESCONOCIDO*")
    {
        try
        {
            switch (await _appDbContext.Parametros.AsNoTracking()
                                                  .FirstOrDefaultAsync())
            {
                case Parametro p:
                    new SmtpClient(p.SmtpHost, p.SmtpPort)
                    {
                        Credentials = new NetworkCredential(p.ServiceEmail, p.SmtpPassword),
                        EnableSsl = true
                    }.Send(new MailMessage(new MailAddress(p.ServiceEmail, p.ServiceDisplayName), new MailAddress(_email, _displayName))
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = isBodyHtml
                    });
                    Audit([$@"Correo ""{subject}"" enviado a {_email}"], memberName);
                    return true;
                default:
                    Audit([$@"No pudo obtener parámetros SMTP para enviar correo ""{subject}"" a {_email}"], memberName);
                    break;
            }
        }
        catch (Exception e)
        {
            Audit([$@"Error enviando correo ""{subject}"" a {_email}", e.Message], memberName);
        }
        finally
        {
            try
            {
                await _appDbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                // TODO: logger
            }
        }
        return false;
    }

    public void Audit(IEnumerable<string> comments, [CallerMemberName] string memberName = "*DESCONOCIDO*")
    {
        foreach (string comment in comments)
        {
            _appDbContext.Add(new Auditoria
            {
                Fecha = DateTime.Now,
                Usuario = _userName,
                IPRemota = _remoteIP,
                Modulo = _modulo,
                Operacion = memberName.Truncar(48),
                Comentario = comment.Truncar(768)
            });
        }
    }
}
