using System;
using System.Collections.Generic;

namespace ClaimSystem.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual Lecturer? Lecturer { get; set; }

    public virtual HR? HR { get; set; }
}
