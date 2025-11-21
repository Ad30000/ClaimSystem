using Microsoft.AspNetCore.Mvc;
using System.Security;

namespace ClaimSystem.Controllers
{
   
    public class BaseController : Controller
    {
        // Helper properties to access session data
        protected string CurrentUserRole => HttpContext.Session.GetString("UserRole") ?? string.Empty;
        protected int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;
        protected string CurrentUserEmail => HttpContext.Session.GetString("UserEmail") ?? string.Empty;

        /*Marco Code. (2024).*/
        protected void EnsureAuthenticated()
        {
            if (string.IsNullOrEmpty(CurrentUserRole))
            {
                throw new UnauthorizedAccessException("User not authenticated. Please log in.");
            }
        }

        /*Marco Code. (2024).*/
        // Helper method to ensure user has one of the specified roles
        protected void EnsureRole(params string[] allowedRoles)
        {
            EnsureAuthenticated();

            if (!allowedRoles.Contains(CurrentUserRole, StringComparer.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException($"Access denied. Required role(s): {string.Join(", ", allowedRoles)}. Your role: {CurrentUserRole}");
            }
        }

        /*Marco Code. (2024).*/
        // Helper method to check if user is in a specific role
        protected bool IsInRole(string role)
        {
            return CurrentUserRole.Equals(role, StringComparison.OrdinalIgnoreCase);
        }

        /*Marco Code. (2024).*/
        // Helper method to get user role for views
        protected string GetUserRole()
        {
            return CurrentUserRole;
        }
    }
}

/*Marco Code. (2024). Employees Management system. Available at: https://www.youtube.com/playlist?list=PLHeVx_crBwV8-CFDeGQpUMoAP-PCIZKai (Accessed: 15 November 2025).*/