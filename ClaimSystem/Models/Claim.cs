using System;
using System.Collections.Generic;

namespace ClaimSystem.Models;

public partial class Claim
{
    public int ClaimId { get; set; }

    public string ClaimNumber { get; set; } = null!;

    public DateTime LastUpdatedDate { get; set; } 

    public int LecturerId { get; set; }

    public string MonthClaim { get; set; } = null!;

    public decimal HoursWorked { get; set; }

    public decimal AmountPerHour { get; set; }

    public decimal? TotalAmount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime DateSubmitted { get; set; }

    public string? AdditionalNotes { get; set; }

    public virtual Lecturer Lecturer { get; set; } = null!;

    public virtual ICollection<SupportingDocument> SupportingDocuments { get; set; } = new List<SupportingDocument>();
}
