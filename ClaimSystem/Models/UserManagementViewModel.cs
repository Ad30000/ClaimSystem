namespace ClaimSystem.Models
{
    public class UserManagementViewModel
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsActive { get; set; }
        public string Name { get; set; } = null!;
        public string Department { get; set; } = null!;
    }
}
