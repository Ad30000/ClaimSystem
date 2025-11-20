using ClaimSystem.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClaimSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ClaimDatabaseContext _context;

        public AccountController(ClaimDatabaseContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        /*Adnan, Y. (2025)*/
        [HttpPost]
        public IActionResult Login(string email, string password, string role)
        {
            if (string.IsNullOrEmpty(role))
            {
                ModelState.AddModelError("role", "Please select a role");
                return View();
            }

            // Validate user against database
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Role == role);

            if (user != null && VerifyPassword(password, user.PasswordHash))
            {
                // Store ACTUAL user info in session
                HttpContext.Session.SetString("UserRole", user.Role);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetInt32("UserId", user.UserId); // Actual user ID from database

                // Redirect based on role
                return user.Role.ToLower() switch
                {
                    "lecturer" => RedirectToAction("Dashboard", "Home"),
                    "coordinator" => RedirectToAction("Index", "Review"),
                    "manager" => RedirectToAction("Index", "Review"),
                    "hr" => RedirectToAction("Dashboard", "Hr"),
                    _ => RedirectToAction("Index", "Home")
                };
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View();
        }

        // Simple password verification 
        private bool VerifyPassword(string enteredPassword, string storedPasswordHash)
        {
            // For now - simple comparison (in real app, use proper hashing)
            return enteredPassword == storedPasswordHash;

            // Later, implement proper password hashing:
            // return BCrypt.Net.BCrypt.Verify(enteredPassword, storedPasswordHash);
        }

        // Logout action

        public IActionResult Logout()
        {
            // Clear all session data
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}

/* Adnan, Y. (2025). Session Management – Roles [PDF lecture slides]. CS405: Advanced Web Development. Emeris Durban North. Available at: https://advtechonline.sharepoint.com/:b:/r/sites/BCA1_VCKNDN/Shared%20Documents/PROG6212%20Group%201/Session%20Management_Guide.pdf?csf=1&web=1&e=g1uN5a (Accessed: 17 November 2025). */