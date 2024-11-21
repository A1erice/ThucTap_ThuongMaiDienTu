using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;                                                                                                                                                                                                          
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using ThucTap_ThuongMaiDienTu.Models;
using System.Text;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using ThucTap_ThuongMaiDienTu.Helper;

namespace ThucTap_ThuongMaiDienTu.Controllers
{
    public class HomeController : Controller
    {
        private readonly HisContext db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(HisContext context, ILogger<HomeController> logger)
        {
            db = context;
            _logger = logger;
        }


        public IActionResult Index()
        {
            if (!JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal))
            {
            }
            return View();
        }

        public IActionResult Privacy()
        {
            if (!JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal))
            {
            }
            return View();
        }
        public IActionResult Contact()
        {
            if (!JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal))
            {
            }
            return View();
        }
        public IActionResult Shop()
        {
            if (!JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal))
            {
                return RedirectToAction("Login", "Dashboard");
            }
            return View();
        }

        public IActionResult Checkout()
        {
            if (!JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal))
            {
                return RedirectToAction("Login", "Dashboard");
            }
            return View();
        }
        public IActionResult Cart()
        {
            if (!JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal))
            {
                return RedirectToAction("Login", "Dashboard");
            }
            return View();
        }
        [HttpGet]
        public IActionResult ChangeLanguage(string lang)
        {
            if (string.IsNullOrEmpty(lang))
            {
                lang = "en-US";  // Default to English if no language is selected
            }

            // Set culture for the current request
            var culture = new CultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // Store the selected culture in a cookie
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(lang)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            // Redirect to the referring page
            return Redirect(Request.GetTypedHeaders().Referer?.ToString() ?? "/");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
