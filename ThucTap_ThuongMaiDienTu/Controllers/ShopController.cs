        using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThucTap_ThuongMaiDienTu.Models;
using ThucTap_ThuongMaiDienTu.ViewModels;

namespace ThucTap_ThuongMaiDienTu.Controllers
{
    public class ShopController : Controller
    {
        private readonly HisContext db;
        public ShopController(HisContext context) => db = context;
        public IActionResult Index(int? category)
        {
            if (!JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal))
            {
                return RedirectToAction("Login", "Dashboard");
            }
            var Medicines = db.Medicines.AsQueryable();
            if (category.HasValue)
            {
                Medicines = Medicines.Where(m => m.CategoryId == category.Value);
            }
            var Result = Medicines.Select(m => new MedicineVM
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price ?? 0,
                Pack = m.Pack,
                Img = m.Img,
                Category = m.Category.Name,
                CategoryID = m.CategoryId
            });
            return View(Result);
        }
        public IActionResult Search(String? query)
        {
            if (!JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal))
            {
                return RedirectToAction("Login", "Dashboard");
            }
            var Medicines = db.Medicines.AsQueryable();
            if (query != null)
            {
                Medicines = Medicines.Where(m => m.Name.Contains(query));
            }
            var Result = Medicines.Select(m => new MedicineVM
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price ?? 0,
                Pack = m.Pack,
                Img = m.Img,
                Category = m.Category.Name
            });
            return View(Result);
        }
        public IActionResult ShopDetail(int? id)
        {
            if (!JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal))
            {
                return RedirectToAction("Login", "Dashboard");
            }
            var data = db.Medicines
                .Include(m => m.Category)
                .Include(m => m.Ratings)
                    .ThenInclude(r => r.Account) // Include Account while loading Ratings
                .SingleOrDefault(m => m.Id == id);

            if (data == null)
            {
                TempData["Message"] = $"This medicine {id} is not found";
                return Redirect("/404");
            }

            var ratingsList = data.Ratings.Select(r => new RatingVM
            {
                Account = r.Account.Name,
                Stars = r.Stars,
                Comment = r.Comment,
            }).ToList();

            var result = new MedicineVM
            {
                Id = data.Id,
                Name = data.Name,
                Description = data.Description,
                Uses = data.Uses,
                Price = data.Price ?? 0,
                Unit = data.Unit,
                Ingredient = data.Ingredient,
                Pack = data.Pack,
                Img = data.Img,
                Category = data.Category.Name,
                CategoryID = data.Category.Id,
                Ratings = ratingsList, // Pass the list of ratings
                AverageRating = ratingsList.Any() ? ratingsList.Average(r => r.Stars) : 0, // Calculate average rating
                RatingsCount = ratingsList.Count // Count of ratings
            };

            return View(result);
        }

    }
}
