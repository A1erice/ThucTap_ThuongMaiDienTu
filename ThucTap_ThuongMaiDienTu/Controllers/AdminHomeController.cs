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
using ThucTap_ThuongMaiDienTu.ViewModels;

namespace ThucTap_ThuongMaiDienTu.Controllers
{
    public class AdminHomeController : Controller
    {
        private readonly HisContext db;
        private readonly ILogger<HomeController> _logger;

        public AdminHomeController(HisContext context, ILogger<HomeController> logger)
        {
            db = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (!JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal) || !User.IsInRole("admin"))
            {
                return RedirectToAction("Login", "Dashboard");
            }
            // Fetch recent orders from the last week
            var oneWeekAgo = DateTime.Now.AddDays(-7);
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var recentOrders = await db.Orders
                .Include(o => o.Account) // Include related Account information
                .Where(o => o.Date >= oneWeekAgo) // Filter by recent orders
                .OrderByDescending(o => o.Date) // Order by newest first
                .ToListAsync();

            var totalProducts = await db.Medicines.CountAsync();
            var totalPendingOrders = await db.Orders.CountAsync(o => o.Status == false);
            var totalSalesThisMonth = await db.Orders
                .Where(o => o.Date >= startOfMonth)
                .SumAsync(o => o.Total ?? 0); // Default to 0 if TotalAmount is null

            var totalSalesAllTime = await db.Orders.SumAsync(o => o.Total ?? 0);
            // Pass the recent orders to the view using ViewData or ViewModel
            var model = new AdminHomeVM
            {
                TotalProducts = totalProducts,
                TotalPendingOrders = totalPendingOrders,
                TotalSalesThisMonth = totalSalesThisMonth,
                TotalSalesAllTime = totalSalesAllTime,
                RecentOrders = recentOrders
            };

            return View(model);
        }

    }
}
