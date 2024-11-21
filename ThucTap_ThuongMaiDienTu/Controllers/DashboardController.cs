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
    public class DashboardController : Controller
    {
        private readonly HisContext db;
        private readonly ILogger<HomeController> _logger;

        public DashboardController(HisContext context, ILogger<HomeController> logger)
        {
            db = context;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            // Check if the user is already authenticated via JWT
            if (JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal))
            {
                return RedirectToAction("Index"); // Redirect to Index if already logged in
            }

            return View(); // Show login view for unauthenticated users
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(Login login)
        {
            // Validate the user's credentials
            var user = db.Accounts.SingleOrDefault(u => u.Username == login.Username);

            if (user == null || !VerifyPassword(login.Password, user.Password))
            {
                // Invalid credentials
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(); // Return the same view with error message
            }

            if (!user.Active)
            {
                ModelState.AddModelError(string.Empty, "Your account is not activated. Please check your email for the activation code.");
                TempData["Username"] = user.Username; // Store username temporarily
                return RedirectToAction("Activate"); // Redirect to the activation page
            }

            // Generate JWT token and store it in a cookie
            var token = JwtTokenHelper.GenerateToken(user);
            Response.Cookies.Append("jwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddHours(1) // Set token expiry
            });

            // Store user info in session
            HttpContext.Session.SetString("UserAccount", Newtonsoft.Json.JsonConvert.SerializeObject(user));

            // Optionally, create a cart if the user doesn't have one

            return user.AccountType == "admin" ? RedirectToAction("Index", "AdminHome") : RedirectToAction("Index");
        }

        // Helper method to create a cart if it doesn't exist

        public IActionResult Logup()
        {
            if (JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal))
            {
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpPost]
        public IActionResult Logup(Logup logup)
        {
            if (logup.Password != logup.CFPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return View();
            }
            // Validate the user's credentials
            var existingUser = db.Accounts
                .SingleOrDefault(u => u.Username == logup.Username || u.Email == logup.Email);

            if (existingUser != null)
            {
                // Add error message to ModelState
                ModelState.AddModelError(string.Empty, "Username or Email is already exists");
                return View();
            }
            var Code = GenerateRandomCode();
            // Create a new user
            var newUser = new Account
            {
                Username = logup.Username,
                Password = HashPassword(logup.Password), // Store hashed password
                Name = logup.Name,
                Address = logup.Address,
                Email = logup.Email,
                AccountType = "customer", // Set a default role
                Active = false,
                Code = Code,
            };
            db.Accounts.Add(newUser);
            db.SaveChanges();
            // Send activation email
            var subject = "Account Activation";
            var body = $"<p>Welcome, {logup.Name}!</p>" +
                       $"<p>Your activation code is: <strong>{Code}</strong></p>" +
                       "<p>Please enter this code to activate your account.</p>";
            EmailHelper.SendEmail(logup.Email, subject, body);

            // Redirect to the activation view
            TempData["Username"] = newUser.Username; // Pass username to the view
            return RedirectToAction("Activate");
        }
        [HttpGet]
        public IActionResult Activate()
        {
            if (JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal))
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Activate(string Username, string Code)
        {
            var user = db.Accounts.SingleOrDefault(u => u.Username == Username && u.Code == Code);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid activation code.");
                return View();
            }

            user.Active = true;  // Activate the user
            db.SaveChanges();

            return RedirectToAction("Login");
        }

        // Password hashing method
        public static string HashPassword(string password)
        {
            // Retrieve the global salt from appsettings.json
            var salt = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build()
                            .GetSection("Security")["GlobalSalt"];

            // Hash the password with the global salt
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password + salt,
                salt: Encoding.UTF8.GetBytes(salt), // Optional, adds more entropy
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            // Retrieve the global salt from appsettings.json
            var salt = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build()
                            .GetSection("Security")["GlobalSalt"];

            // Hash the entered password with the global salt
            var enteredHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: enteredPassword + salt,
                salt: Encoding.UTF8.GetBytes(salt),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));

            // Compare the two hashes
            return storedHash == enteredHash;
        }
        private string GenerateRandomCode(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                                        .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public IActionResult Logout()
        {
            // Clear the JWT token from the cookie
            Response.Cookies.Delete("jwtToken");

            // Clear session data if needed
            HttpContext.Session.Remove("UserAccount");

            // Optionally, redirect to the login page or homepage
            return RedirectToAction("Login");
        }
        public IActionResult Dashboard()
        {
            if (!JwtTokenHelper.TryAuthenticateUser(Request, HttpContext, out var principal))
            {
                return RedirectToAction("Login");
            }


            return View();
        }

    }
}
