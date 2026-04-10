using System.ComponentModel.DataAnnotations;

namespace InsuranceClaims.Models.ViewModels
{
    public class AssessmentViewModel
    {
        [Required]
        public int ClaimId { get; set; }

        [Required(ErrorMessage = "Assessed amount is required.")]
        [Range(0, 10000000, ErrorMessage = "Amount must be between 0 and 1,00,00,000.")]
        public decimal AssessedAmount { get; set; }

        public string? AssessmentRemarks { get; set; }
    }
}
