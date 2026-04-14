using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceClaims.Models.Entities
{
    //Officer assigns a Surveyor to a Claim for assessment.
    public class SurveyorAssignment
    {
        [Key]
        public int AssignmentId { get; set; }

        [ForeignKey("Claim")]
        public int ClaimId { get; set; }

        [ForeignKey("Surveyor")]
        public int SurveyorId { get; set; }

        [ForeignKey("Officer")]
        public int OfficerId { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.Now;

        public string? Notes { get; set; }

        // Navigation
        public Claim?    Claim    { get; set; }
        public Surveyor? Surveyor { get; set; }
        public Officer?  Officer  { get; set; }
    }
}
