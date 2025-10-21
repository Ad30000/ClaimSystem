using System;
using System.Collections.Generic;

namespace ClaimSystem.Models;

public partial class Lecturer
{
    public int LecturerId { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Department { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();

    public virtual User User { get; set; } = null!;
}
