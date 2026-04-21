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

        [Required(ErrorMessage = "Bank account number is required.")]
        [StringLength(20, MinimumLength = 9, ErrorMessage = "Account number must be between 9 and 20 digits.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Account number must contain only digits.")]
        public string BankAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "IFSC code is required.")]
        [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "Invalid IFSC code format (e.g., SBIN0001234).")]
        public string IFSCCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bank name is required.")]
        [StringLength(100, ErrorMessage = "Bank name cannot exceed 100 characters.")]
        public string BankName { get; set; } = string.Empty;
    }
}
