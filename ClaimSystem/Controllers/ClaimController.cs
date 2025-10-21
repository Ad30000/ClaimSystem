using ClaimSystem.DB;
using ClaimSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMCS.Controllers
{
    public class ClaimController : Controller
    {
        private readonly ClaimDatabaseContext _context;

        public ClaimController(ClaimDatabaseContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var claims = _context.Claims
                .Include(c => c.Lecturer)
                .ThenInclude(l => l.User)
                .ToList();
            return View(claims);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Claim claim, IFormFile supportingDoc)
        {
            //if (ModelState.IsValid)
            {
                try
                {
                    // Generate claim number
                    claim.ClaimNumber = GenerateClaimNumber();
                    claim.DateSubmitted = DateTime.Now;
                    claim.LastUpdatedDate = DateTime.Now;
                    claim.Status = "Pending";

                    // For demo purposes, set a lecturer ID (you'll replace this with actual user logic)
                    claim.LecturerId = 1; // Temporary - replace with actual logged-in lecturer ID

                    // Calculate total amount
                    claim.TotalAmount = claim.HoursWorked * claim.AmountPerHour;

                    // Handle file upload
                    if (supportingDoc != null && supportingDoc.Length > 0)
                    {
                        var document = new SupportingDocument
                        {
                            FileName = supportingDoc.FileName,
                            ContentType = Path.GetExtension(supportingDoc.FileName),
                            FileSize = supportingDoc.Length,
                            FileUploadDate = DateTime.Now,
                            Description = "Supporting document for claim"
                        };

                        // For prototype, store file path (in production, store actual file)
                        document.FilePath = $"uploads/{document.FileName}";

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
            }

            return View(claim);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var claim = _context.Claims
                .Include(c => c.Lecturer)
                .ThenInclude(l => l.User)
                .Include(c => c.SupportingDocuments)
                .FirstOrDefault(c => c.ClaimId == id);

            if (claim == null)
            {
                return NotFound();
            }

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
