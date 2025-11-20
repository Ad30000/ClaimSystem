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

        
        // HR Dashboard 
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
                        PasswordHash = model.Password, 
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
                    
                }
            }

            
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

                // Update lecturer details
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

        // Export to Excel 
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

            // Generate simple CSV 
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

        // Activate/Deactivate lecturer accounts
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

        [HttpGet]
        public IActionResult AddUser()
        {
            EnsureRole("HR");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(UserViewModel model)
        {
            EnsureRole("HR");

            if (ModelState.IsValid)
            {
                try
                {
                    // Validate role
                    var validRoles = new[] { "Lecturer", "Coordinator", "Manager", "HR" };
                    if (!validRoles.Contains(model.Role))
                    {
                        ModelState.AddModelError("Role", "Invalid role selected.");
                        return View(model);
                    }

                    // Check if email already exists
                    if (_context.Users.Any(u => u.Email == model.Email))
                    {
                        ModelState.AddModelError("Email", "Email address is already registered.");
                        return View(model);
                    }

                    // Create user account
                    var user = new User
                    {
                        Username = model.Email,
                        Email = model.Email,
                        PasswordHash = model.Password, // In production, hash this properly
                        Role = model.Role,
                        IsActive = true,
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Create profile based on role
                    switch (model.Role.ToLower())
                    {
                        case "lecturer":
                            var lecturer = new Lecturer
                            {
                                UserId = user.UserId,
                                Name = model.Name,
                                Department = model.Department,
                                PhoneNumber = model.PhoneNumber,
                                Address = model.Address
                            };
                            _context.Lecturers.Add(lecturer);
                            break;

                        case "coordinator":
                        case "manager":
                            // For now, coordinators and managers are stored as lecturers with different roles
                            // You might want to create separate tables for these roles later
                            var staff = new Lecturer
                            {
                                UserId = user.UserId,
                                Name = model.Name,
                                Department = model.Department,
                                PhoneNumber = model.PhoneNumber,
                                Address = model.Address
                            };
                            _context.Lecturers.Add(staff);
                            break;

                        case "hr":
                            var hr = new HR
                            {
                                UserId = user.UserId,
                                Name = model.Name,
                                Department = model.Department,
                                PhoneNumber = model.PhoneNumber,
                                Address = model.Address,
                                DefaultHourlyRate = 0 // Set default rate
                            };
                            _context.HR.Add(hr);
                            break;
                    }

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"{model.Role} added successfully!";
                    return RedirectToAction("ManageUsers");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error adding user: {ex.Message}");
                }
            }

            return View(model);
        }

        // Manage all users (Lecturers, Coordinators, Managers, HR)
        // Manage all users (Lecturers, Coordinators, Managers, HR)
        public IActionResult ManageUsers()
        {
            EnsureRole("HR");

            var users = _context.Users
                .Include(u => u.Lecturer)
                .Include(u => u.HR)
                .Where(u => u.Role != "System") // Exclude system accounts if any
                .Select(u => new UserManagementViewModel
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    Name = u.Lecturer != null ? u.Lecturer.Name :
                           u.HR != null ? u.HR.Name : "N/A",
                    Department = u.Lecturer != null ? u.Lecturer.Department :
                                u.HR != null ? u.HR.Department : "N/A",
                    // Add these for action links
                    LecturerId = u.Lecturer != null ? u.Lecturer.LecturerId : (int?)null,
                    HRId = u.HR != null ? u.HR.HRId : (int?)null
                })
                .ToList();

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            EnsureRole("HR");

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user != null)
                {
                    user.IsActive = !user.IsActive;
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"User {(user.IsActive ? "activated" : "deactivated")} successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "User not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating user status: {ex.Message}";
            }

            return RedirectToAction("ManageUsers");
        }

        // Edit User
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            EnsureRole("HR");

            var user = _context.Users
                .Include(u => u.Lecturer)
                .Include(u => u.HR)
                .FirstOrDefault(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserViewModel
            {
                Id = user.UserId, // Set the Id property
                Name = user.Lecturer?.Name ?? user.HR?.Name ?? "",
                Department = user.Lecturer?.Department ?? user.HR?.Department ?? "",
                PhoneNumber = user.Lecturer?.PhoneNumber ?? user.HR?.PhoneNumber,
                Address = user.Lecturer?.Address ?? user.HR?.Address,
                Email = user.Email,
                Role = user.Role
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserViewModel model)
        {
            EnsureRole("HR");

            Console.WriteLine($"Model State IsValid: {ModelState.IsValid}");
            Console.WriteLine($"Model ID: {model?.Id}");
            Console.WriteLine($"Model Name: {model?.Name}");
            Console.WriteLine($"Model Role: {model?.Role}");

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Model Error: {error.ErrorMessage}");
                }

                ViewBag.UserId = model?.Id;
                return View(model);
            }

            try
            {
                var user = await _context.Users
                    .Include(u => u.Lecturer)
                    .Include(u => u.HR)
                    .FirstOrDefaultAsync(u => u.UserId == model.Id);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("ManageUsers");
                }

                // Update user email
                user.Email = model.Email;
                user.Username = model.Email;

                // Update profile based on role
                switch (user.Role.ToLower())
                {
                    case "lecturer":
                    case "coordinator":
                    case "manager":
                        if (user.Lecturer != null)
                        {
                            user.Lecturer.Name = model.Name;
                            user.Lecturer.Department = model.Department;
                            user.Lecturer.PhoneNumber = model.PhoneNumber;
                            user.Lecturer.Address = model.Address;
                        }
                        break;

                    case "hr":
                        if (user.HR != null)
                        {
                            user.HR.Name = model.Name;
                            user.HR.Department = model.Department;
                            user.HR.PhoneNumber = model.PhoneNumber;
                            user.HR.Address = model.Address;
                        }
                        break;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction("ManageUsers");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                ModelState.AddModelError("", $"Error updating user: {ex.Message}");
                ViewBag.UserId = model?.Id;
                return View(model);
            }
        }

        // User Details
        public IActionResult UserDetails(int id)
        {
            EnsureRole("HR");

            var user = _context.Users
                .Include(u => u.Lecturer)
                .Include(u => u.HR)
                .Include(u => u.Lecturer.Claims)
                .FirstOrDefault(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


    }
}