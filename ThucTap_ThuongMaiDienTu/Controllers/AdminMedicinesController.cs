using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ThucTap_ThuongMaiDienTu.Models;
using ThucTap_ThuongMaiDienTu.ViewModels;

namespace ThucTap_ThuongMaiDienTu.Controllers
{
    public class AdminMedicinesController : Controller
    {
        private readonly HisContext _context;

        public AdminMedicinesController(HisContext context)
        {
            _context = context;
        }

        // GET: AdminMedicines
        public async Task<IActionResult> Index()
        {
            var hisContext = _context.Medicines.Include(m => m.Category);
            return View(await hisContext.ToListAsync());
        }

        // GET: AdminMedicines/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Medicines == null)
            {
                return NotFound();
            }

            var medicine = await _context.Medicines
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (medicine == null)
            {
                return NotFound();
            }

            return View(medicine);
        }

        // GET: AdminMedicines/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.MedicineCategories, "Id", "Name");
            return View();
        }

        // POST: AdminMedicines/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: AdminMedicines/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicineCreateVM model)
        {
            // Check if the category ID is valid
            var category = await _context.MedicineCategories.FindAsync(model.CategoryId);

            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "The selected category does not exist.");
                ViewData["CategoryId"] = new SelectList(_context.MedicineCategories, "Id", "Name", model.CategoryId);
                return View(model);
            }
            string relativePath = null;
            if (model.ImgFile != null && model.ImgFile.Length > 0)
            {
                // Define the relative path and physical path to save the image
                var uploadsFolder = Path.Combine("wwwroot", "img", "medicines");
                Directory.CreateDirectory(uploadsFolder); // Ensure the directory exists
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImgFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the image to the server
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImgFile.CopyToAsync(fileStream);
                }

                // Set the relative path to store in the database
                relativePath = Path.Combine("img", "medicines", uniqueFileName).Replace("\\", "/");
            }
            // Create the Medicine entity from the ViewModel
            var medicine = new Medicine
            {
                Name = model.Name,
                CategoryId = model.CategoryId,
                Quantity = model.Quantity,
                Price = model.Price,
                Unit = model.Unit,
                Description = model.Description,
                Uses = model.Uses,
                Img = relativePath,
                Sold = 0,
                Ingredient = model.Ingredient,
                Pack = model.Pack
            };

            if (ModelState.IsValid)
            {
                _context.Add(medicine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.MedicineCategories, "Id", "Name", model.CategoryId);
            return View(model);
        }


        // GET: AdminMedicines/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Medicines == null)
            {
                return NotFound();
            }

            var medicine = await _context.Medicines.FindAsync(id);
            if (medicine == null)
            {
                return NotFound();
            }

            // Map the Medicine to MedicineViewModel
            var model = new MedicineCreateVM
            {
                Id = medicine.Id,
                Name = medicine.Name,
                CategoryId = medicine.CategoryId,
                Quantity = medicine.Quantity,
                Price = medicine.Price,
                Unit = medicine.Unit,
                Description = medicine.Description,
                Uses = medicine.Uses,
                Img = medicine.Img,
                Sold = medicine.Sold,
                Ingredient = medicine.Ingredient,
                Pack = medicine.Pack
            };

            ViewData["CategoryId"] = new SelectList(_context.MedicineCategories, "Id", "Name", medicine.CategoryId);
            return View(model);
        }

        // POST: AdminMedicines/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedicine(int id, MedicineCreateVM model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            // Check if the category ID is valid
            var category = await _context.MedicineCategories.FindAsync(model.CategoryId);

            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "The selected category does not exist.");
                ViewData["CategoryId"] = new SelectList(_context.MedicineCategories, "Id", "Name", model.CategoryId);
                return View(model);
            }

            // Create the Medicine entity from the ViewModel
            var medicine = new Medicine
            {
                Id = model.Id,
                Name = model.Name,
                CategoryId = model.CategoryId,
                Quantity = model.Quantity,
                Price = model.Price,
                Unit = model.Unit,
                Description = model.Description,
                Uses = model.Uses,
                Img = model.Img,
                Sold = model.Sold,
                Ingredient = model.Ingredient,
                Pack = model.Pack
            };

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(medicine);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicineExists(medicine.Id))
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

            ViewData["CategoryId"] = new SelectList(_context.MedicineCategories, "Id", "Name", model.CategoryId);

            return RedirectToAction(nameof(Index));
        }
        // GET: AdminMedicines/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Medicines == null)
            {
                return NotFound();
            }

            var medicine = await _context.Medicines
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (medicine == null)
            {
                return NotFound();
            }

            return View(medicine);
        }

        // POST: AdminMedicines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Medicines == null)
            {
                return Problem("Entity set 'HisContext.Medicines'  is null.");
            }
            var medicine = await _context.Medicines.FindAsync(id);
            if (medicine != null)
            {
                _context.Medicines.Remove(medicine);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MedicineExists(int id)
        {
          return (_context.Medicines?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
