
//using ClaimSystem.DB;
//using ClaimSystem.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace ClaimSystem.Controllers
//{

//    public class ReviewController : Controller
//    {
//        private readonly ClaimDatabaseContext _context;

//        public ReviewController(ClaimDatabaseContext context)
//        {
//            _context = context;
//        }

//        public IActionResult Index()
//        {
//            var pendingClaims = _context.Claims
//                .Include(c => c.Lecturer)
//                .ThenInclude(l => l.User)
//                .Where(c => c.Status == "Pending")
//                .ToList();

//            return View(pendingClaims);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Approve(int id)
//        {
//            var claim = await _context.Claims.FindAsync(id);
//            if (claim != null)
//            {
//                // Use "Coordinator Approved" instead of "Approved"
//                claim.Status = "Coordinator Approved";
//                claim.LastUpdatedDate = DateTime.Now;
//                await _context.SaveChangesAsync();

//                TempData["SuccessMessage"] = "Claim approved successfully!";
//            }

//            return RedirectToAction("Index");
//        }

//        //// Two-step approval process
//        //public async Task<IActionResult> Approve(int id, string comments)
//        //{
//        //    var claim = await _context.Claims.FindAsync(id);
//        //    var userRole = HttpContext.Session.GetString("UserRole");

//        //    if (userRole == "Coordinator")
//        //    {
//        //        claim.Status = "Coordinator Approved";
//        //        // Create approval record
//        //        var approval = new ClaimApproval
//        //        {
//        //            ClaimId = id,
//        //            UserId = HttpContext.Session.GetInt32("UserId").Value,
//        //            Role = "Coordinator",
//        //            Action = "Approve",
//        //            Status = "Coordinator Approved",
//        //            Comments = comments,
//        //            ApprovalDate = DateTime.Now
//        //        };
//        //        _context.ClaimApprovals.Add(approval);
//        //    }
//        //    else if (userRole == "Manager")
//        //    {
//        //        claim.Status = "Approved";
//        //        // Similar approval record for manager
//        //    }

//        //    await _context.SaveChangesAsync();
//        //    return RedirectToAction("Index");
//        //}

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Reject(int id)
//        {
//            var claim = await _context.Claims.FindAsync(id);
//            if (claim != null)
//            {
//                claim.Status = "Rejected";
//                claim.LastUpdatedDate = DateTime.Now;
//                await _context.SaveChangesAsync();

//                TempData["SuccessMessage"] = "Claim rejected!";
//            }

//            return RedirectToAction("Index");
//        }
//    }
//}

using ClaimSystem.DB;
using ClaimSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace ClaimSystem.Controllers
{
    public class ReviewController : BaseController
    {
        private readonly ClaimDatabaseContext _context;

        public ReviewController(ClaimDatabaseContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            EnsureRole("Coordinator", "Manager");

            // Check if user is logged in and has appropriate role
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || (userRole != "Coordinator" && userRole != "Manager"))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserRole = userRole;

            try
            {
                // Get claims based on user role
                var claimsQuery = _context.Claims
                    .Include(c => c.Lecturer)
                    .ThenInclude(l => l.User)
                    .Include(c => c.SupportingDocuments)
                    .AsQueryable();

                if (userRole == "Coordinator")
                {
                    // Coordinators see only pending claims
                    claimsQuery = claimsQuery.Where(c => c.Status == "Pending");
                }
                else if (userRole == "Manager")
                {
                    // Managers see claims that have been coordinator approved but not manager approved
                    claimsQuery = claimsQuery.Where(c => c.Status == "Coordinator Approved");
                }

                var claims = claimsQuery.ToList();
                return View(claims);
            }
            catch (Exception ex)
            {
                // Log the error and show a user-friendly message
                TempData["ErrorMessage"] = "Error loading claims. Please try again.";
                // In development, you might want to see the actual error:
                // TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return View(new List<Claim>());
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            EnsureRole("Coordinator", "Manager");

            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (string.IsNullOrEmpty(userRole) || !userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var claim = await _context.Claims.FindAsync(id);
                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                    return RedirectToAction("Index");
                }

                // Update claim status based on role
                if (userRole == "Coordinator")
                {
                    claim.Status = "Coordinator Approved";
                    TempData["SuccessMessage"] = "Claim approved by Coordinator! Waiting for Manager approval.";
                }
                else if (userRole == "Manager")
                {
                    claim.Status = "Fully Approved";
                    TempData["SuccessMessage"] = "Claim fully approved!";
                }

                claim.LastUpdatedDate = DateTime.Now;

                // Create approval record
                var approval = new ClaimApproval
                {
                    ClaimId = id,
                    UserId = userId.Value,
                    ApprovalDate = DateTime.Now,
                    Role = userRole,
                    Action = "Approve",
                    Status = claim.Status,
                    Comments = $"Approved by {userRole}"
                };

                _context.ClaimApprovals.Add(approval);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error approving claim: {ex.Message}";
            }

            return RedirectToAction("Index");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? comments = null)
        {
            EnsureRole("Coordinator", "Manager");

            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (string.IsNullOrEmpty(userRole) || !userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var claim = await _context.Claims
                .Include(c => c.ClaimApprovals)
                .FirstOrDefaultAsync(c => c.ClaimId == id);

            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim not found.";
                return RedirectToAction("Index");
            }

            try
            {
                // Create rejection record
                var approval = new ClaimApproval
                {
                    ClaimId = id,
                    UserId = userId.Value,
                    ApprovalDate = DateTime.Now,
                    Role = userRole,
                    Action = "Reject",
                    Status = "Rejected",
                    Comments = comments ?? $"Rejected by {userRole}"
                };

                _context.ClaimApprovals.Add(approval);

                // Update claim status
                claim.Status = "Rejected";
                claim.LastUpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Claim rejected successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error rejecting claim: {ex.Message}";
            }

            return RedirectToAction("Index");
        }


        public IActionResult ApprovalHistory(int id)
        {
            EnsureRole("Coordinator", "Manager");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var claim = _context.Claims
                    .Include(c => c.Lecturer)
                    .ThenInclude(l => l.User)
                    .Include(c => c.ClaimApprovals)
                    .ThenInclude(ca => ca.Approver)
                    .FirstOrDefault(c => c.ClaimId == id);

                if (claim == null)
                {
                    return NotFound();
                }

                ViewBag.UserRole = userRole;
                return View(claim);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading approval history.";
                return RedirectToAction("Index");
            }
        }
    }
}