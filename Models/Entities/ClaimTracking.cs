using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceClaims.Models.Entities
{
    public class ClaimTracking
    {
        [Key]
        public int TrackingId { get; set; }

        [ForeignKey("Claim")]
        public int ClaimId { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        public string? Remarks { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        public Claim? Claim { get; set; }
    }
}
