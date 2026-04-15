using System.ComponentModel.DataAnnotations;

namespace InsuranceClaims.Models.Entities
{
    public class Policy
    {
        [Key]
        public int PolicyId { get; set; }

        [Required]
        public string PolicyName { get; set; } = string.Empty;

        [Required]
        [Range(1000, 10000000, ErrorMessage = "Coverage amount must be between 1,000 and 1,00,00,000.")]
        public decimal CoverageAmount { get; set; }

        public string? Description { get; set; }
        public ICollection<PolicyPurchase> PolicyPurchases { get; set; } = new List<PolicyPurchase>();
    }
}
