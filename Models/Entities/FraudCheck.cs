using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InsuranceClaims.Enums;

namespace InsuranceClaims.Models.Entities
{
    public class FraudCheck
    {
        [Key]
        public int FraudId { get; set; }

        [ForeignKey("Claim")]
        public int ClaimId { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Fraud score must be between 0 and 100.")]
        public int FraudScore { get; set; }

        [Required]
        public RiskFlag RiskFlag { get; set; }

        // Navigation
        public Claim? Claim { get; set; }
    }
}
