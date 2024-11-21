using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ThucTap_ThuongMaiDienTu.Models;

namespace ThucTap_ThuongMaiDienTu.Controllers
{
    public class AdminAccountsController : Controller
    {
        private readonly HisContext _context;

        public AdminAccountsController(HisContext context)
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
        // GET: AdminAccounts
        public async Task<IActionResult> Index()
        {
              return _context.Accounts != null ? 
                          View(await _context.Accounts.ToListAsync()) :
                          Problem("Entity set 'HisContext.Accounts'  is null.");
        }

        // GET: AdminAccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // GET: AdminAccounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdminAccounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Account account)
        {
            // Validate the user's credentials
            var existingUser = _context.Accounts
                .SingleOrDefault(u => u.Username == account.Username || u.Email == account.Email);

            if (existingUser != null)
            {
                // Add error message to ModelState
                ModelState.AddModelError(string.Empty, "Username or Email is already exists");
                return View();
            }
            if (!string.IsNullOrWhiteSpace(account.Username) && !Regex.IsMatch(account.Username, @"^\S+$"))
            {
                ModelState.AddModelError(nameof(account.Username), "Username cannot contain spaces. (controller noti)");
            }
            account.Password = DashboardController.HashPassword("123456"); // Example password
            account.Code = "0"; // Example password
            account.Active = true; // Example password
            ModelState.Remove("Password");
            ModelState.Remove("Code");
            if (ModelState.IsValid)
            {
                _context.Add(account);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(account);
        }

        // GET: AdminAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            return View(account);
        }

        // POST: AdminAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Account account)
        {
            if (id != account.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.Id))
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
            return View(account);
        }

        // GET: AdminAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: AdminAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Accounts == null)
            {
                return Problem("Entity set 'HisContext.Accounts'  is null.");
            }
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
          return (_context.Accounts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
