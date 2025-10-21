using Microsoft.AspNetCore.Mvc;

namespace ClaimSystem.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {

            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password, string role)
        {
            // For prototype purposes, we'll simulate successful login
            // In Part 2, this will connect to actual authentication

            if (string.IsNullOrEmpty(role))
            {
                ModelState.AddModelError("role", "Please select a role");
                return View();
            }

            // Store user role in session/temp data for demonstration
            TempData["UserRole"] = role;
            TempData["UserEmail"] = email;

            // Redirect based on role
            return role.ToLower() switch
            {
                "lecturer" => RedirectToAction("Dashboard", "Home"),
                "coordinator" => RedirectToAction("Index", "Review"),
                "manager" => RedirectToAction("Index", "Review"),
                "hr" => RedirectToAction("HrDashboard", "Home"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        public IActionResult Logout()
        {
            TempData.Remove("UserRole");
            TempData.Remove("UserEmail");
            return RedirectToAction("Login");
        }
    }
}

