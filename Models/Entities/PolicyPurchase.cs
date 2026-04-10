using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceClaims.Models.Entities
{
    public class PolicyPurchase
    {
        [Key]
        public int PurchaseId { get; set; }

        [ForeignKey("Policy")]
        public int PolicyId { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // Navigation
        public Policy? Policy { get; set; }
        public Customer? Customer { get; set; }
        public ICollection<PolicyRenewal> Renewals { get; set; } = new List<PolicyRenewal>();
    }
}
