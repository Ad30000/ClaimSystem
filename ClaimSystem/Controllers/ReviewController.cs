using ClaimSystem.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClaimSystem.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ClaimDatabaseContext _context;

        public ReviewController(ClaimDatabaseContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var pendingClaims = _context.Claims
                .Include(c => c.Lecturer)
                .ThenInclude(l => l.User)
                .Where(c => c.Status == "Pending")
                .ToList();

            return View(pendingClaims);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim != null)
            {
                claim.Status = "Approved";
                claim.LastUpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Claim approved successfully!";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim != null)
            {
                claim.Status = "Rejected";
                claim.LastUpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Claim rejected!";
            }

            return RedirectToAction("Index");
        }
    }
}
