using ClaimSystem.DB;
using ClaimSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;

namespace ClaimSystem.Controllers
{
    public class HrController : BaseController
    {
        private readonly ClaimDatabaseContext _context;

        public HrController(ClaimDatabaseContext context)
        {
            _context = context;
        }

        
        // HR Dashboard with enhanced features
        public IActionResult Dashboard()
        {
            EnsureRole("HR");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR") return RedirectToAction("Login", "Account");

            // Get statistics
            var stats = new
            {
                TotalClaims = _context.Claims.Count(),
                PendingClaims = _context.Claims.Count(c => c.Status == "Pending"),
                ApprovedClaims = _context.Claims.Count(c => c.Status == "Approved"),
                TotalLecturers = _context.Lecturers.Count(),
                CoordinatorApproved = _context.Claims.Count(c => c.Status == "Coordinator Approved"),
                RejectedClaims = _context.Claims.Count(c => c.Status == "Rejected")
            };

            // Get recent claims (last 10 claims)
            var recentClaims = _context.Claims
                .Include(c => c.Lecturer)
                .ThenInclude(l => l.User)
                .OrderByDescending(c => c.DateSubmitted)
                .Take(10)
                .ToList();

            // Create a view model to pass both stats and recent claims
            var viewModel = new DashboardViewModel
            {
                Statistics = stats,
                RecentClaims = recentClaims
            };

            return View(viewModel);
        }

        // Manage Lecturers with CRUD operations
        public IActionResult ManageLecturers()
        {
            EnsureRole("HR");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR") return RedirectToAction("Login", "Account");

            var lecturers = _context.Lecturers
                .Include(l => l.User)
                .Include(l => l.Claims)
                .ToList();
            return View(lecturers);
        }

        // Add new lecturer
        [HttpGet]
        public IActionResult AddLecturer()
        {
            EnsureRole("HR");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR") return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLecturer(LecturerViewModel model)
        {
            EnsureRole("HR");

            if (ModelState.IsValid)
            {
                try
                {
                    // Create user account first
                    var user = new User
                    {
                        Username = model.Email,
                        Email = model.Email,
                        PasswordHash = model.Password, // Hash the password
                        Role = "Lecturer",
                        IsActive = true,
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Create lecturer profile
                    var lecturer = new Lecturer
                    {
                        UserId = user.UserId,
                        Name = model.Name,
                        Department = model.Department,
                        PhoneNumber = model.PhoneNumber,
                        Address = model.Address
                    };

                    _context.Lecturers.Add(lecturer);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Lecturer added successfully!";
                    return RedirectToAction("ManageLecturers");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error adding lecturer: {ex.Message}");
                    // Log the exception here
                }
            }

            // If we got this far, something failed; redisplay form
            return View(model);
        }

        // Edit lecturer
        [HttpGet]
        public IActionResult EditLecturer(int id)
        {
            EnsureRole("HR");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR") return RedirectToAction("Login", "Account");

            var lecturer = _context.Lecturers
                .Include(l => l.User)
                .FirstOrDefault(l => l.LecturerId == id);

            if (lecturer == null)
            {
                return NotFound();
            }

            return View(lecturer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLecturer(int LecturerId, string Name, string Department, string PhoneNumber, string Address, int UserId)
        {
            EnsureRole("HR");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR") return RedirectToAction("Login", "Account");

            try
            {
                var existingLecturer = await _context.Lecturers
                    .Include(l => l.User)
                    .FirstOrDefaultAsync(l => l.LecturerId == LecturerId);

                if (existingLecturer == null)
                {
                    TempData["ErrorMessage"] = "Lecturer not found.";
                    return RedirectToAction("ManageLecturers");
                }

                // Update properties
                existingLecturer.Name = Name;
                existingLecturer.Department = Department;
                existingLecturer.PhoneNumber = PhoneNumber;
                existingLecturer.Address = Address;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Lecturer updated successfully!";
                return RedirectToAction("ManageLecturers");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating lecturer: {ex.Message}";
                return RedirectToAction("EditLecturer", new { id = LecturerId });
            }
        }

        private bool LecturerExists(int id)
        {
            return _context.Lecturers.Any(e => e.LecturerId == id);
        }

        // Generate Reports with filtering
        [HttpGet]
        public IActionResult GenerateReports(string statusFilter, string lecturerFilter, DateTime? startDate, DateTime? endDate)
        {
            EnsureRole("HR");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR") return RedirectToAction("Login", "Account");

            var claimsQuery = _context.Claims
                .Include(c => c.Lecturer)
                .ThenInclude(l => l.User)
                .Include(c => c.SupportingDocuments)
                .Include(c => c.ClaimApprovals)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(statusFilter))
            {
                claimsQuery = claimsQuery.Where(c => c.Status == statusFilter);
            }

            if (!string.IsNullOrEmpty(lecturerFilter))
            {
                claimsQuery = claimsQuery.Where(c => c.Lecturer.Name.Contains(lecturerFilter));
            }

            if (startDate.HasValue)
            {
                claimsQuery = claimsQuery.Where(c => c.DateSubmitted >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                claimsQuery = claimsQuery.Where(c => c.DateSubmitted <= endDate.Value);
            }

            var claims = claimsQuery.OrderByDescending(c => c.DateSubmitted).ToList();

            // Pass filter values to maintain state
            ViewBag.StatusFilter = statusFilter;
            ViewBag.LecturerFilter = lecturerFilter;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(claims);
        }

        // Export to Excel (Basic implementation)
        public IActionResult ExportToExcel(string statusFilter, string lecturerFilter, DateTime? startDate, DateTime? endDate)
        {
            EnsureRole("HR");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR") return RedirectToAction("Login", "Account");

            // Filter claims as in GenerateReports
            var claimsQuery = _context.Claims
                .Include(c => c.Lecturer)
                .ThenInclude(l => l.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter))
            {
                claimsQuery = claimsQuery.Where(c => c.Status == statusFilter);
            }

            if (!string.IsNullOrEmpty(lecturerFilter))
            {
                claimsQuery = claimsQuery.Where(c => c.Lecturer.Name.Contains(lecturerFilter));
            }

            if (startDate.HasValue)
            {
                claimsQuery = claimsQuery.Where(c => c.DateSubmitted >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                claimsQuery = claimsQuery.Where(c => c.DateSubmitted <= endDate.Value);
            }

            var claims = claimsQuery.OrderByDescending(c => c.DateSubmitted).ToList();

            // Generate simple CSV (for Excel)
            var csv = "Claim Number, Lecturer, Date Submitted, Month, Hours Worked, Hourly Rate, Total Amount, Status\n";
            foreach (var claim in claims)
            {
                csv += $"\"{claim.ClaimNumber}\",\"{claim.Lecturer.Name}\",\"{claim.DateSubmitted:yyyy-MM-dd}\",\"{claim.MonthClaim}\",{claim.HoursWorked},{claim.AmountPerHour},{claim.TotalAmount},\"{claim.Status}\"\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"ClaimsReport_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }

        // Set default hourly rate for lecturers
        [HttpGet]
        public IActionResult SetHourlyRate()
        {
            EnsureRole("HR");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR") return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SetHourlyRate(decimal hourlyRate)
        {
            EnsureRole("HR");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR") return RedirectToAction("Login", "Account");

            try
            {
                // Update HR default rate
                var hrUser = await _context.HR.FirstOrDefaultAsync();
                if (hrUser != null)
                {
                    hrUser.DefaultHourlyRate = hourlyRate;
                    _context.HR.Update(hrUser);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"Default hourly rate set to R{hourlyRate:F2}";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error setting hourly rate: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLecturerStatus(int id)
        {
            EnsureRole("HR");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR") return RedirectToAction("Login", "Account");

            try
            {
                var lecturer = await _context.Lecturers
                    .Include(l => l.User)
                    .FirstOrDefaultAsync(l => l.LecturerId == id);

                if (lecturer?.User != null)
                {
                    lecturer.User.IsActive = !lecturer.User.IsActive;
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Lecturer {(lecturer.User.IsActive ? "activated" : "deactivated")} successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Lecturer not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating lecturer status: {ex.Message}";
            }

            return RedirectToAction("ManageLecturers");
        }

        // View lecturer details
        public IActionResult LecturerDetails(int id)
        {
            EnsureRole("HR");

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR") return RedirectToAction("Login", "Account");

            var lecturer = _context.Lecturers
                .Include(l => l.User)
                .Include(l => l.Claims)
                .ThenInclude(c => c.ClaimApprovals)
                .FirstOrDefault(l => l.LecturerId == id);

            if (lecturer == null)
            {
                return NotFound();
            }

            return View(lecturer);
        }

        //private string HashPassword(string password)
        //{
            

        //    // Implement proper password hashing here
        //    // Consider using BCrypt.Net, ASP.NET Core Identity, or similar
        //    using var sha256 = System.Security.Cryptography.SHA256.Create();
        //    var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        //    var hash = sha256.ComputeHash(bytes);
        //    return Convert.ToBase64String(hash);
        //}
    }
}