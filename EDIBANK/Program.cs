using EDIBANK.Conf_Db_With_Entity;
using EDIBANK.Models.Users_EdiWeb;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EDIBANK;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
        builder.Services.AddSession(static void (SessionOptions options) =>
                        {
                            options.IdleTimeout = TimeSpan.FromMinutes(10);
                        });
        builder.Services.AddAuthorization();
        builder.Services.AddMvc()
                        .AddRazorPagesOptions(static void (RazorPagesOptions options) =>
                        {
                            options.Conventions.AddAreaPageRoute("Identity", "/Account/Login", string.Empty);
                        });
        builder.Services.AddDbContextPool<AppDbContext>(void (DbContextOptionsBuilder options) =>
                        {
                            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), static void (SqlServerDbContextOptionsBuilder options) =>
                            {
                                options.EnableRetryOnFailure();
                            });
                        });

        // Add Identity Framework Core & Setting for Identity "AppUser"
        builder.Services.AddDefaultIdentity<AppUser>(static void (IdentityOptions options) =>
                        {
                            options.SignIn.RequireConfirmedAccount = false;
                        })
                        .AddDefaultTokenProviders()
                        .AddRoles<IdentityRole>()
                        .AddEntityFrameworkStores<AppDbContext>();

        // Add Identity Setting for Username (Email) , Password Policy, Lock & Unloc User Accounts
        builder.Services.Configure(static void (IdentityOptions options) =>
                        {
                            options.User.RequireUniqueEmail = true;
                            //options.SignIn.RequireConfirmedEmail = true;
                            //options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyz";
                            options.Password.RequiredLength = 8;
                            options.Password.RequireDigit = true;
                            options.Password.RequireNonAlphanumeric = true;
                            options.Password.RequireLowercase = true;
                            options.Password.RequireUppercase = true;
                            options.Lockout.AllowedForNewUsers = true;
                            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                            options.Lockout.MaxFailedAccessAttempts = 3;
                        });

        // Add Setting for Identity the time of token validity
        // builder.Services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(10));

        // Add Identity Framework Core & Setting for Identity Cookie
        builder.Services.ConfigureApplicationCookie(static void (CookieAuthenticationOptions options) =>
                        {
                            options.Cookie.Name = ".AspNetCore.Identity.Application";
                            options.Cookie.HttpOnly = true;
                            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                            options.Cookie.SameSite = SameSiteMode.Strict;
                            options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
                            options.Cookie.MaxAge = options.ExpireTimeSpan;
                            options.SlidingExpiration = true;
                            options.LoginPath = "/Account/Login";  //set the login page.
                            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                        });

        WebApplication webApp = builder.Build();

        // Configure the HTTP request pipeline.
        if (!webApp.Environment.IsDevelopment())
        {
            webApp.UseExceptionHandler("/Home/Error");

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            webApp.UseHsts();
        }
        webApp.UseHttpsRedirection();
        webApp.MapStaticAssets();
        webApp.UseRouting();
        webApp.UseAuthentication();
        webApp.Use(static async Task (HttpContext context, RequestDelegate next) =>
            {
                try
                {
                    await next(context);
                }
                catch
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "text/html";
                    context.Response.Redirect("/Home/Error");
                    //await context.Response.WriteAsync("<html lang=\"en\"><body>\r\n");
                    //await context.Response.WriteAsync("<h1>ERROR!</h1><br><br>\r\n");

                    //var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

                    //if (exceptionHandlerPathFeature?.Error is FileNotFoundException)
                    //{
                    //    await context.Response.WriteAsync(
                    //    "File error thrown!<br><br>\r\n");
                    //}
                    //await context.Response.WriteAsync("<a href=\"/\">Home</a><br>\r\n");
                    //await context.Response.WriteAsync("</body></html>\r\n");
                    //await context.Response.WriteAsync(new string(' ', 512));
                }
            });
        webApp.UseSession();
        webApp.UseAuthorization();
        webApp.MapRazorPages().WithStaticAssets();
        //webApp.MapControllerRoute(
        //    name: "Bienvenida",
        //    pattern: "{area:Defaultlogin}/{controller=login}/{action=index}/{id?}");
        //webApp.Run();
        webApp.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();
        webApp.Run();
    }
}
