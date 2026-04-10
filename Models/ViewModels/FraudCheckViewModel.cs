using System.ComponentModel.DataAnnotations;
using InsuranceClaims.Enums;

namespace InsuranceClaims.Models.ViewModels
{
    public class FraudCheckViewModel
    {
        [Required]
        public int ClaimId { get; set; }

        [Required(ErrorMessage = "Fraud score is required.")]
        [Range(0, 100, ErrorMessage = "Score must be between 0 and 100.")]
        public int FraudScore { get; set; }

        [Required(ErrorMessage = "Please select a risk flag.")]
        public RiskFlag RiskFlag { get; set; }
    }
}
