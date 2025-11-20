using Microsoft.AspNetCore.Mvc;
using System.Security;

namespace ClaimSystem.Controllers
{
    public class BaseController : Controller
    {
        protected string CurrentUserRole => HttpContext.Session.GetString("UserRole") ?? string.Empty;
        protected int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;
        protected string CurrentUserEmail => HttpContext.Session.GetString("UserEmail") ?? string.Empty;

        protected void EnsureAuthenticated()
        {
            if (string.IsNullOrEmpty(CurrentUserRole))
            {
                throw new UnauthorizedAccessException("User not authenticated. Please log in.");
            }
        }

        protected void EnsureRole(params string[] allowedRoles)
        {
            EnsureAuthenticated();

            if (!allowedRoles.Contains(CurrentUserRole, StringComparer.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException($"Access denied. Required role(s): {string.Join(", ", allowedRoles)}. Your role: {CurrentUserRole}");
            }
        }

        // Helper method to check if user is in a specific role
        protected bool IsInRole(string role)
        {
            return CurrentUserRole.Equals(role, StringComparison.OrdinalIgnoreCase);
        }

        // Helper method to get user role for views
        protected string GetUserRole()
        {
            return CurrentUserRole;
        }
    }
}
