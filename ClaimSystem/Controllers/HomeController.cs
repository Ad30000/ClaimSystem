using System.Diagnostics;
using ClaimSystem.DB;
using ClaimSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClaimSystem.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ClaimDatabaseContext _context;

        public HomeController(ILogger<HomeController> logger, ClaimDatabaseContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: Home/Dashboard
        public IActionResult Dashboard()
        {
            EnsureAuthenticated();

            // Get user info from session
            var userRole = HttpContext.Session.GetString("UserRole");
            var userEmail = HttpContext.Session.GetString("UserEmail");

            // Check if user is logged in
            if (string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Login", "Account");
            }

            // Store in ViewBag for the view
            ViewBag.UserRole = userRole;
            ViewBag.UserEmail = userEmail;

            if (userRole == "HR")
                return RedirectToAction("Dashboard", "Hr");

            // Get actual claims from database
            var claims = _context.Claims
                .Include(c => c.Lecturer)
                .ThenInclude(l => l.User)
                .Include(c => c.SupportingDocuments)
                .OrderByDescending(c => c.DateSubmitted)
                .ToList();

            return View(claims);
        }

        // GET: Home/HrDashboard
        public IActionResult HrDashboard()
        {
            EnsureRole("HR");

            // Check if user is HR
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "HR")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["Title"] = "HR Dashboard";
            ViewBag.UserRole = userRole;
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");

            var approvedClaims = _context.Claims
                .Include(c => c.Lecturer)
                .ThenInclude(l => l.User)
                .Where(c => c.Status == "Approved")
                .OrderByDescending(c => c.DateSubmitted)
                .ToList();

            return View(approvedClaims);
        }

        // GET: Home/AccessDenied
        public IActionResult AccessDenied(string message)
        {
            ViewBag.AccessDeniedMessage = message ?? "You do not have permission to access this page.";
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");
            ViewBag.AttemptedUrl = HttpContext.Request.Query["returnUrl"];

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Error handling
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}