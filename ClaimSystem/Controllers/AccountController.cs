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

        //[HttpPost]
        //public IActionResult Login(string email, string password, string role)
        //{
        //    if (string.IsNullOrEmpty(role))
        //    {
        //        ModelState.AddModelError("role", "Please select a role");
        //        return View();
        //    }

        //    // For prototype purposes - in real application, validate against database
        //    if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        //    {
        //        // Store user info in session
        //        HttpContext.Session.SetString("UserRole", role);
        //        HttpContext.Session.SetString("UserEmail", email);
        //        HttpContext.Session.SetInt32("UserId", 1); // Replace with actual user ID from database

        //        // Redirect based on role
        //        return role.ToLower() switch
        //        {
        //            "lecturer" => RedirectToAction("Dashboard", "Home"),
        //            "coordinator" => RedirectToAction("Index", "Review"),
        //            "manager" => RedirectToAction("Index", "Review"),
        //            "hr" => RedirectToAction("Dashboard", "Hr"),
        //            _ => RedirectToAction("Index", "Home")
        //        };
        //    }

        //    ModelState.AddModelError("", "Invalid login attempt.");
        //    return View();
        //}

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

        // Add this helper method for password verification
        private bool VerifyPassword(string enteredPassword, string storedPasswordHash)
        {
            // For now - simple comparison (in real app, use proper hashing)
            return enteredPassword == storedPasswordHash;

            // Later, implement proper password hashing:
            // return BCrypt.Net.BCrypt.Verify(enteredPassword, storedPasswordHash);
        }

        public IActionResult Logout()
        {
            // Clear all session data
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}