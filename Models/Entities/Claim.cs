using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InsuranceClaims.Enums;

namespace InsuranceClaims.Models.Entities
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        [ForeignKey("Policy")]
        public int PolicyId { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [Required]
        public string ClaimType { get; set; } = string.Empty;

        [Required]
        [Range(100, 10000000, ErrorMessage = "Claim amount must be between 100 and 1,00,00,000.")]
        public decimal ClaimAmount { get; set; }

        [Required]
        public ClaimStatus ClaimStatus { get; set; } = ClaimStatus.REGISTERED;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public Policy? Policy { get; set; }
        public Customer? Customer { get; set; }
        public ICollection<ClaimDocument> Documents { get; set; } = new List<ClaimDocument>();
        public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
        public FraudCheck? FraudCheck { get; set; }
        public SettlementLog? SettlementLog { get; set; }
        public ICollection<ClaimTracking> TrackingHistory { get; set; } = new List<ClaimTracking>();
    }
}
