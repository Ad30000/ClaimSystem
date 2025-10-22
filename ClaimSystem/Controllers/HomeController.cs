//using System.Diagnostics;
//using ClaimSystem.DB;
//using ClaimSystem.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace ClaimSystem.Controllers
//{
//    public class HomeController : Controller
//    {
//        private readonly ILogger<HomeController> _logger;
//        private readonly ClaimDatabaseContext _context;

//        public HomeController(ILogger<HomeController> logger, ClaimDatabaseContext context)
//        {
//            _logger = logger;
//            _context = context;
//        }

//        public IActionResult Index()
//        {
//            return View();
//        }

//        public IActionResult Dashboard()
//        {
//            var userRole = TempData["UserRole"]?.ToString();
//            var userEmail = TempData["UserEmail"]?.ToString();

//            // Store these back in TempData for the view
//            TempData["UserRole"] = userRole;
//            TempData["UserEmail"] = userEmail;

//            if (userRole == "HR")
//                return RedirectToAction("HrDashboard");

//            // Get claims for the dashboard
//            var claims = _context.Claims
//                .Include(c => c.Lecturer)
//                .ThenInclude(l => l.User)
//                .ToList();

//            return View(claims);
//        }

//        public IActionResult HrDashboard()
//        {
//            ViewData["Title"] = "HR Dashboard";

//            var approvedClaims = _context.Claims
//                .Include(c => c.Lecturer)
//                .ThenInclude(l => l.User)
//                .Where(c => c.Status == "Approved")
//                .ToList();

//            return View(approvedClaims);
//        }

//        public IActionResult Privacy()
//        {
//            return View();
//        }

//        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//        public IActionResult Error()
//        {
//            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//        }
//    }

//}


using System.Diagnostics;
using ClaimSystem.DB;
using ClaimSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClaimSystem.Controllers
{
    public class HomeController : Controller
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

        public IActionResult Dashboard()
        {
            var userRole = TempData["UserRole"]?.ToString();
            var userEmail = TempData["UserEmail"]?.ToString();

            // Store these back in TempData for the view
            TempData["UserRole"] = userRole;
            TempData["UserEmail"] = userEmail;

            if (userRole == "HR")
                return RedirectToAction("HrDashboard");

            // Get actual claims from database
            var claims = _context.Claims
                .Include(c => c.Lecturer)
                .ThenInclude(l => l.User)
                .Include(c => c.SupportingDocuments)
                .OrderByDescending(c => c.DateSubmitted)
                .ToList();

            return View(claims);
        }

        public IActionResult HrDashboard()
        {
            ViewData["Title"] = "HR Dashboard";

            // Get actual approved claims from database
            var approvedClaims = _context.Claims
                .Include(c => c.Lecturer)
                .ThenInclude(l => l.User)
                .Where(c => c.Status == "Approved")
                .OrderByDescending(c => c.DateSubmitted)
                .ToList();

            return View(approvedClaims);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}