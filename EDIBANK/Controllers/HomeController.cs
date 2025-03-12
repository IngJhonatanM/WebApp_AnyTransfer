using EDIBANK.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EDIBANK.Controllers
{
    //  [Authorize(Roles = "User")]
    //  [Authorize(Roles = "Admin")]

    [Authorize]
    public class HomeController : Controller

    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Auth_Users to back View

        [Authorize]
        public IActionResult Secured()
        {
            return View((object)"Hello");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Contact()

        {
            return View();
        }

        [AllowAnonymous]

        public IActionResult _SessionExpireNotification()

        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}