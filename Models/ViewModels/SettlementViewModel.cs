using System.ComponentModel.DataAnnotations;

namespace InsuranceClaims.Models.ViewModels
{
    public class SettlementViewModel
    {
        [Required]
        public int ClaimId { get; set; }

        [Required(ErrorMessage = "Settlement amount is required.")]
        [Range(0, 10000000, ErrorMessage = "Amount must be between 0 and 1,00,00,000.")]
        public decimal SettlementAmount { get; set; }

        public string? Remarks { get; set; }
    }
}
