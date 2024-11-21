using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThucTap_ThuongMaiDienTu.Models;
using ThucTap_ThuongMaiDienTu.ViewModels;

namespace ThucTap_ThuongMaiDienTu.Controllers
{
    public class BuyController : Controller
    {
        private readonly HisContext db;

        public BuyController(HisContext context) => db = context;
        public IActionResult Index()
        {
            if (!JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal))
            {
                return RedirectToAction("Login", "Dashboard");
            }

            // Get the user's account ID from the JWT token
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(); // Handle unauthorized access
            }

            CreateCartIfNotExists(userId);

            // Retrieve the cart and cart details
            var cart = db.Carts
                .Include(c => c.Account) // Eager load the associated account
                .Include(c => c.CartDetails)
                .ThenInclude(cd => cd.Medicine)
                .FirstOrDefault(c => c.AccountId == userId);

            if (cart == null)
            {
                ViewBag.Message = "Your cart is empty.";
                return View();
            }

            // Recalculate the total by summing the 'Total' field of all cart details
            cart.Total = cart.CartDetails.Sum(cd => cd.Total);

            // Update the cart in the database with the new total
            db.Carts.Update(cart);
            db.SaveChanges();

            // Pass the cart and cart details to the view
            return View(cart);
        }
        private void CreateCartIfNotExists(int userId)
        {
            var existingCart = db.Carts.SingleOrDefault(c => c.AccountId == userId);
            if (existingCart == null)
            {
                var newCart = new Cart
                {
                    AccountId = userId,
                    Total = 0 // Initialize total or set as required
                };
                db.Carts.Add(newCart);
                db.SaveChanges(); // Save the new cart
            }
        }
        [HttpPost]
        public IActionResult UpdateCart(int medicineId, int cartId, string action)
        {
            if (!JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal))
            {
                return Json(new { success = false, message = "Please log in." });
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            var cartDetail = db.CartDetails
                .Include(cd => cd.Medicine)
                .FirstOrDefault(cd => cd.CartId == cartId && cd.MedicineId == medicineId);

            if (cartDetail == null)
            {
                return Json(new { success = false, message = "Item not found in your cart." });
            }

            // Adjust quantity based on action
            if (action == "increase" && cartDetail.Medicine.Quantity > cartDetail.Amount)
            {
                cartDetail.Amount++;
            }
            else if (action == "decrease" && cartDetail.Amount > 1)
            {
                cartDetail.Amount--;
            }
            else
            {
                return Json(new { success = false, message = $"Cannot adjust quantity for {cartDetail.Medicine.Name}." });
            }

            // Update total and save changes
            cartDetail.Total = cartDetail.Amount * cartDetail.Medicine.Price;
            db.SaveChanges();

            var cartTotal = db.CartDetails.Where(cd => cd.CartId == cartId).Sum(cd => cd.Total);
            // Return updated data
            return Json(new { success = true, newAmount = cartDetail.Amount, newTotal = cartDetail.Total, cartTotal = cartTotal });

        }


        public IActionResult Checkout(string address)
        {
            if (!JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal))
            {
                return RedirectToAction("Login", "Dashboard");
            }

            // Get the user's account ID from the JWT token
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(); // Handle unauthorized access
            }

            // Retrieve the cart and its details
            var cart = db.Carts
                .Include(c => c.CartDetails)
                .ThenInclude(cd => cd.Medicine)
                .FirstOrDefault(c => c.AccountId == userId);

            if (cart == null || !cart.CartDetails.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            foreach (var cartDetail in cart.CartDetails)
            {
                var medicine = cartDetail.Medicine;

                if (medicine.Quantity < cartDetail.Amount)
                {
                    TempData["ErrorMessage"] = $"Not enough stock for {medicine.Name}. Available: {medicine.Quantity}, Requested: {cartDetail.Amount}.";
                    return RedirectToAction("Index");
                }

                // Decrease the quantity
                medicine.Quantity -= cartDetail.Amount;
                medicine.Sold += cartDetail.Amount;
            }

            // Create a new order
            var newOrder = new Order
            {
                Status = false,
                Address = address,
                AccountId = userId,
                Date = DateTime.Now,
                Total = cart.Total,
                OrderDetails = cart.CartDetails.Select(cd => new OrderDetail
                {
                    MedicineId = cd.MedicineId,
                    Amount = cd.Amount,
                    MedicinePrice = cd.Medicine.Price,
                    SumPrice = cd.Total
                }).ToList()
            };

            // Save the new order to the database
            db.Orders.Add(newOrder);
            db.SaveChanges();

            // Clear the user's cart after placing the order
            db.CartDetails.RemoveRange(cart.CartDetails);
            db.Carts.Remove(cart);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Order placed successfully!"; 
            return RedirectToAction("OrderDetail", "Buy", new { id = newOrder.Id });

        }
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity, int price)
        {
            // Get the user's account ID from the JWT token
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                TempData["ErrorMessage"] = "User not authenticated.";
                return RedirectToAction("ProductDetail", new { id = productId }); // Redirect back if user is not authenticated
            }

            CreateCartIfNotExists(userId);

            // Get or create the user's cart
            var cart = db.Carts.SingleOrDefault(c => c.AccountId == userId);

            // Validate the quantity
            if (quantity <= 0)
                quantity = 1;

            // Check if the product is already in the cart
            var cartDetail = db.CartDetails.SingleOrDefault(cd => cd.CartId == cart.Id && cd.MedicineId == productId);

            if (cartDetail == null)
            {
                // Product is not in the cart, add new entry
                cartDetail = new CartDetail
                {
                    CartId = cart.Id, // Use the newly created or existing cart ID
                    MedicineId = productId,
                    Amount = quantity,
                    Total = quantity * price
                };
                db.CartDetails.Add(cartDetail);
            }
            else
            {
                // Product is already in the cart, update the quantity and sum
                cartDetail.Amount += quantity;
                cartDetail.Total = cartDetail.Amount * price;
                db.CartDetails.Update(cartDetail);
            }

            db.Carts.Update(cart); // Save the cart total change if modified
            db.SaveChanges(); // Save changes to the database

            TempData["SuccessMessage"] = "Product added to cart successfully!";
            return RedirectToAction("ShopDetail", "Shop", new { id = productId });

        }
        public async Task<IActionResult> OrderList()
        {
            if (!JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal))
            {
                return RedirectToAction("Login", "Dashboard");
            }

            // Get the user's account ID from the JWT token
            var userIdClaim = (principal as ClaimsPrincipal)?.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(); // Handle unauthorized access
            }

            // Retrieve the list of orders for the user
            var userOrders = await db.Orders
                .Where(o => o.AccountId == userId) // Filter by user ID
                .OrderByDescending(o => o.Date)    // Optional: Order by most recent
                .ToListAsync();

            return View(userOrders); // Pass the list to the view
        }


        // GET: Orders/Details/5
        public async Task<IActionResult> OrderDetail(int? id)
        {
            if (!JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal))
            {
                return RedirectToAction("Login", "Dashboard");
            }

            if (id == null || db.Orders == null)
            {
                return NotFound();
            }

            var userIdClaim = (principal as ClaimsPrincipal)?.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(); // Handle unauthorized access
            }

            var order = await db.Orders
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

    }
}
