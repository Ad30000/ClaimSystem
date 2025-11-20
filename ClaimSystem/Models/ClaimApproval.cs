using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimSystem.Models
{
    public class ClaimApproval
    {
        [Key]
        public int ClaimApprovalId { get; set; }

        [Required]
        public int ClaimId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime ApprovalDate { get; set; }

        [StringLength(30)]
        public string Role { get; set; } // "Coordinator" or "Manager"

        [StringLength(20)]
        public string Action { get; set; }

        [StringLength(30)]
        public string Status { get; set; }

        [StringLength(500)]
        public string Comments { get; set; }


        // Navigation properties
        [ForeignKey("ClaimId")]
        public virtual Claim Claim { get; set; }

        [ForeignKey("UserId")]
        public virtual User Approver { get; set; }

    }
}
