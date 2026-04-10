using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InsuranceClaims.Enums;

namespace InsuranceClaims.Models.Entities
{
    public class ClaimDocument
    {
        [Key]
        public int DocumentId { get; set; }

        [ForeignKey("Claim")]
        public int ClaimId { get; set; }

        [Required]
        public string DocumentName { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.PENDING;

        // Navigation
        public Claim? Claim { get; set; }
    }
}
