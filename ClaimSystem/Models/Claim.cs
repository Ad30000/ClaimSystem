using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimSystem.Models;

public partial class Claim
{
    [Key]
    public int ClaimId { get; set; }

    [Required]
    [StringLength(20)]
    public string ClaimNumber { get; set; } = null!;

    public DateTime LastUpdatedDate { get; set; }

    [Required]
    public int LecturerId { get; set; }

    [Required]
    [StringLength(20)]
    public string MonthClaim { get; set; } = null!;

    [Column(TypeName = "decimal(10,2")]
    public decimal HoursWorked { get; set; }

    [Column(TypeName = "decimal(10,2")]
    public decimal AmountPerHour { get; set; }

    [Column(TypeName = "decimal(21,4")]
    public decimal? TotalAmount { get; set; }

    [StringLength(30)]
    public string Status { get; set; } = "Pending";

    public DateTime DateSubmitted { get; set; }

    [StringLength(500)]
    public string? AdditionalNotes { get; set; }

    [ForeignKey("LecturerId")]
    public virtual Lecturer Lecturer { get; set; } = null!;

    public virtual ICollection<SupportingDocument> SupportingDocuments { get; set; } = new List<SupportingDocument>();

    public virtual ICollection<ClaimApproval> ClaimApprovals { get; set; } = new List<ClaimApproval>();
}
