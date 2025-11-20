using ClaimSystem.DB;
using ClaimSystem.Attributes; // Add this using directive
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClaimSystem.Controllers
{
    public class HrController : Controller
    {
        private readonly ClaimDatabaseContext _context;

        [AuthorizeRole("HR")]
        public HrController(ClaimDatabaseContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR") return RedirectToAction("Login", "Account");

            var stats = new
            {
                TotalClaims = _context.Claims.Count(),
                PendingClaims = _context.Claims.Count(c => c.Status == "Pending"),
                ApprovedClaims = _context.Claims.Count(c => c.Status == "Approved"),
                TotalLecturers = _context.Lecturers.Count()
            };

            return View(stats);
        }

        public IActionResult ManageLecturers()
        {
            var lecturers = _context.Lecturers
                .Include(l => l.User)
                .Include(l => l.Claims)
                .ToList();
            return View(lecturers);
        }

        public IActionResult GenerateReports()
        {
            var approvedClaims = _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.Status == "Approved")
                .ToList();

            return View(approvedClaims);
        }
    }
}
