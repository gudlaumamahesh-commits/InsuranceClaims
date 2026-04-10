using System.ComponentModel.DataAnnotations;

namespace InsuranceClaims.Models.ViewModels
{
    public class CreateClaimViewModel
    {
        [Required(ErrorMessage = "Please select a policy.")]
        public int PolicyId { get; set; }

        [Required(ErrorMessage = "Claim type is required.")]
        public string ClaimType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Claim amount is required.")]
        [Range(100, 10000000, ErrorMessage = "Amount must be between 100 and 1,00,00,000.")]
        public decimal ClaimAmount { get; set; }
    }
}
