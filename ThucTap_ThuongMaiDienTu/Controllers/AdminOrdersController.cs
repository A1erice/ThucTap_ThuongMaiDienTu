using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ThucTap_ThuongMaiDienTu.Models;

namespace ThucTap_ThuongMaiDienTu.Controllers
{
    public class AdminOrdersController : Controller
    {
        private readonly HisContext _context;

        public AdminOrdersController(HisContext context)
        {
            _context = context;
        }

        // GET: AdminOrders
        public async Task<IActionResult> Index()
        {
            var hisContext = _context.Orders.Include(o => o.Account); 
            ViewBag.ListType = "All";
            return View(await hisContext.ToListAsync());
        }
        public async Task<IActionResult> ShippingOrders()
        {
            var hisContext = _context.Orders
                .Include(o => o.Account)
                .Where(o => o.Status); // Filters orders with Status = true
            ViewBag.ListType = "Shipping";
            return View("Index", await hisContext.ToListAsync());
        }
        public async Task<IActionResult> PendingOrders()
        {
            var hisContext = _context.Orders
                .Include(o => o.Account)
                .Where(o => o.Status == false); // Filters orders with Status = true
            ViewBag.ListType = "Pending";
            return View("Index", await hisContext.ToListAsync());
        }
        public async Task<IActionResult> NewPendingOrders()
        {
            var oneWeekAgo = DateTime.Now.AddDays(-7); // Calculate the date one week ago

            var hisContext = _context.Orders
                .Include(o => o.Account)
                .Where(o => o.Status == false && o.Date >= oneWeekAgo); // Add date filter

            return View("Index", await hisContext.ToListAsync());
        }

        // GET: AdminOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal))
            {
                return RedirectToAction("Login", "Dashboard");
            }

            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var userIdClaim = (principal as ClaimsPrincipal)?.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(); // Handle unauthorized access
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Medicine) // Include medicine details
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null || order.AccountId != userId)
            {
                TempData["ErrorMessage"] = "You are not authorized to view this order.";
                return RedirectToAction("OrderList"); // Redirect to the list of orders
            }

            return View(order);
        }
        [HttpPost]
        public async Task<IActionResult> Approve(int orderId)
        {
            // Retrieve the order from the database using the provided orderId
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                // If the order is not found, return an error or redirect
                return NotFound();
            }

            // Change the status of the order (from false to true)
            order.Status = true;

            // Save changes to the database
            _context.Update(order);
            await _context.SaveChangesAsync();

            // Redirect to the order list page or a specific order details page
            return Json(new { reload = true });
        }
    }
}
