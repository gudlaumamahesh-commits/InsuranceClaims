using System.ComponentModel.DataAnnotations;

namespace InsuranceClaims.Models.ViewModels
{
    public class AddPolicyViewModel
    {
        [Required(ErrorMessage = "Policy name is required.")]
        [StringLength(100, ErrorMessage = "Policy name must be under 100 characters.")]
        public string PolicyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Coverage amount is required.")]
        [Range(1000, 100000000, ErrorMessage = "Coverage must be between ₹1,000 and ₹10,00,00,000.")]
        public decimal CoverageAmount { get; set; }

        [StringLength(500, ErrorMessage = "Description must be under 500 characters.")]
        public string? Description { get; set; }
    }
}
