using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InsuranceClaims.Enums;

namespace InsuranceClaims.Models.Entities
{
    public class SettlementLog
    {
        [Key]
        public int SettlementId { get; set; }

        [ForeignKey("Claim")]
        public int ClaimId { get; set; }

        [Required]
        [Range(0, 10000000)]
        public decimal SettlementAmount { get; set; }

        [Required]
        public SettlementStatus SettlementStatus { get; set; }

        public string? Remarks { get; set; }

        public DateTime ProcessedAt { get; set; } = DateTime.Now;

        // Navigation
        public Claim? Claim { get; set; }
    }
}
