using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceClaims.Models.Entities
{
    public class Assessment
    {
        [Key]
        public int AssessmentId { get; set; }

        [ForeignKey("Claim")]
        public int ClaimId { get; set; }

        [ForeignKey("Surveyor")]
        public int SurveyorId { get; set; }

        [Required]
        [Range(0, 10000000)]
        public decimal AssessedAmount { get; set; }

        public string? AssessmentRemarks { get; set; }

        public DateTime AssessedAt { get; set; } = DateTime.Now;

        // Navigation
        public Claim? Claim { get; set; }
        public Surveyor? Surveyor { get; set; }
    }
}
