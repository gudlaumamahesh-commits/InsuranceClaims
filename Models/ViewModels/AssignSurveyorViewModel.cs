using System.ComponentModel.DataAnnotations;

namespace InsuranceClaims.Models.ViewModels
{
    public class AssignSurveyorViewModel
    {
        [Required]
        public int ClaimId { get; set; }

        [Required(ErrorMessage = "Please select a surveyor.")]
        public int SurveyorId { get; set; }

        public string? Notes { get; set; }
    }
}
