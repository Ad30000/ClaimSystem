using ClaimSystem.DB;
using ClaimSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClaimSystem.Controllers
{
    public class ClaimController : BaseController
    {
        private readonly ClaimDatabaseContext _context;

        public ClaimController(ClaimDatabaseContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            EnsureAuthenticated();

            // Check if user is logged in
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Login", "Account");
            }

            var claims = _context.Claims
                .Include(c => c.Lecturer)
                .ThenInclude(l => l.User)
                .ToList();
            return View(claims);
        }

        [HttpGet]
        public IActionResult Create()
        {
            EnsureRole("Lecturer");

            // Check if user is logged in
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Login", "Account");
            }

            // Get HR-set hourly rate and pass it to the view
            var hrDefaultRate = _context.HR
                .OrderBy(h => h.HRId)
                .Select(h => h.DefaultHourlyRate)
                .FirstOrDefault();

            ViewData["HourlyRate"] = hrDefaultRate.ToString("F2");
            ViewBag.HourlyRate = hrDefaultRate.ToString("F2"); // Alternative approach

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Claim claim, IFormFile supportingDoc)
        {
            EnsureRole("Lecturer");

            // Check if user is logged in
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Get HR-set hourly rate instead of user input
                var hrDefaultRate = _context.HR
                    .OrderBy(h => h.HRId)
                    .Select(h => h.DefaultHourlyRate)
                    .FirstOrDefault();

                claim.AmountPerHour = hrDefaultRate > 0 ? hrDefaultRate : claim.AmountPerHour;

                // Generate claim number
                claim.ClaimNumber = GenerateClaimNumber();
                claim.DateSubmitted = DateTime.Now;
                claim.LastUpdatedDate = DateTime.Now;
                claim.Status = "Pending";

                // FIXED: Get the actual lecturer ID from the logged-in user
                var userId = HttpContext.Session.GetInt32("UserId");
                var lecturer = _context.Lecturers.FirstOrDefault(l => l.UserId == userId);

                if (lecturer == null)
                {
                    ModelState.AddModelError("", "Lecturer profile not found. Please contact administrator.");
                    return View(claim);
                }

                claim.LecturerId = lecturer.LecturerId;

                // Calculate total amount
                claim.TotalAmount = claim.HoursWorked * claim.AmountPerHour;

                // Handle file upload
                if (supportingDoc != null && supportingDoc.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await supportingDoc.CopyToAsync(memoryStream);

                    var document = new SupportingDocument
                    {
                        FileName = supportingDoc.FileName,
                        ContentType = supportingDoc.ContentType,
                        FileContent = memoryStream.ToArray(),
                        FileSize = supportingDoc.Length,
                        FileUploadDate = DateTime.Now,
                        Description = "Supporting document for claim",
                        FilePath = $"uploads/{supportingDoc.FileName}"
                    };

                    claim.SupportingDocuments = new List<SupportingDocument> { document };
                }
                else
                {
                    var document = new SupportingDocument
                    {
                        FileName = "No file uploaded",
                        ContentType = "text/plain",
                        FileContent = new byte[0],
                        FileSize = 0,
                        FileUploadDate = DateTime.Now,
                        Description = "No supporting document provided",
                        FilePath = "none"
                    };
                    claim.SupportingDocuments = new List<SupportingDocument> { document };
                }

                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Claim submitted successfully!";
                return RedirectToAction("Details", new { id = claim.ClaimId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error submitting claim: {ex.Message}");
            }

            return View(claim);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            EnsureAuthenticated();

            // Check if user is logged in
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Login", "Account");
            }

            var claim = _context.Claims
                .Include(c => c.Lecturer)
                .ThenInclude(l => l.User)
                .Include(c => c.SupportingDocuments)
                .FirstOrDefault(c => c.ClaimId == id);

            if (claim == null)
            {
                return NotFound();
            }

            // FIXED: Pass user role to view for conditional buttons - using ViewBag instead of TempData
            ViewBag.UserRole = userRole;

            return View(claim);
        }

        private string GenerateClaimNumber()
        {
            var lastClaim = _context.Claims.OrderByDescending(c => c.ClaimId).FirstOrDefault();
            var nextNumber = lastClaim == null ? 1 : int.Parse(lastClaim.ClaimNumber.Replace("CLM-", "")) + 1;
            return $"CLM-{nextNumber:D3}";
        }

    }
}