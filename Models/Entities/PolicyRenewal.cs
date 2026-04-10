using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceClaims.Models.Entities
{
    public class PolicyRenewal
    {
        [Key]
        public int RenewalId { get; set; }

        [ForeignKey("PolicyPurchase")]
        public int PurchaseId { get; set; }

        [Required]
        public DateTime RenewalDate { get; set; }

        [Required]
        public DateTime NewEndDate { get; set; }

        // Navigation
        public PolicyPurchase? PolicyPurchase { get; set; }
    }
}
