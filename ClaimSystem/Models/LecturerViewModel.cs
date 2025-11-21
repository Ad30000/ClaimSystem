using System.ComponentModel.DataAnnotations;

namespace ClaimSystem.Models;

public class UserViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [Display(Name = "Full Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Department is required")]
    public string Department { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = null!;

    [Phone(ErrorMessage = "Invalid phone number")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    [Required]
    public string Role { get; set; } = null!;

    
    public string? Password { get; set; }
}

public class LecturerViewModel
    {
        public string Name { get; set; }
        public string Department { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
