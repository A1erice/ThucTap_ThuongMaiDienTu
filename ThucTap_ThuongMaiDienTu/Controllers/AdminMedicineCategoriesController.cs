using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ThucTap_ThuongMaiDienTu.Models;

namespace ThucTap_ThuongMaiDienTu.Controllers
{
    public class AdminMedicineCategoriesController : Controller
    {
        private readonly HisContext _context;

        public AdminMedicineCategoriesController(HisContext context)
        {
            _context = context;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Check if the user is authenticated via JWT
            if (!JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal))
            {
                // Redirect to Login if not authenticated
                context.Result = RedirectToAction("Login", "Dashboard");
            }
            if (!User.IsInRole("admin"))
            {
                // Redirect to Login if not authenticated
                context.Result = RedirectToAction("Index", "Home");
            }
            base.OnActionExecuting(context);
        }

        // GET: AdminMedicineCategories
        public async Task<IActionResult> Index()
        {
              return _context.MedicineCategories != null ? 
                          View(await _context.MedicineCategories.ToListAsync()) :
                          Problem("Entity set 'HisContext.MedicineCategories'  is null.");
        }

        // GET: AdminMedicineCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.MedicineCategories == null)
            {
                return NotFound();
            }

            var medicineCategory = await _context.MedicineCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (medicineCategory == null)
            {
                return NotFound();
            }

            return View(medicineCategory);
        }

        // GET: AdminMedicineCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdminMedicineCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Img,Description")] MedicineCategory medicineCategory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(medicineCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(medicineCategory);
        }

        // GET: AdminMedicineCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MedicineCategories == null)
            {
                return NotFound();
            }

            var medicineCategory = await _context.MedicineCategories.FindAsync(id);
            if (medicineCategory == null)
            {
                return NotFound();
            }
            return View(medicineCategory);
        }

        // POST: AdminMedicineCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Img,Description")] MedicineCategory medicineCategory)
        {
            if (id != medicineCategory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(medicineCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicineCategoryExists(medicineCategory.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(medicineCategory);
        }

        // GET: AdminMedicineCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MedicineCategories == null)
            {
                return NotFound();
            }

            var medicineCategory = await _context.MedicineCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (medicineCategory == null)
            {
                return NotFound();
            }

            return View(medicineCategory);
        }

        // POST: AdminMedicineCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MedicineCategories == null)
            {
                return Problem("Entity set 'HisContext.MedicineCategories'  is null.");
            }
            var medicineCategory = await _context.MedicineCategories.FindAsync(id);
            if (medicineCategory != null)
            {
                _context.MedicineCategories.Remove(medicineCategory);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MedicineCategoryExists(int id)
        {
          return (_context.MedicineCategories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
