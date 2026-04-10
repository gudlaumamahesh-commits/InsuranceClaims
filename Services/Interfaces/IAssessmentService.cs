using InsuranceClaims.Models.Entities;
using InsuranceClaims.Models.ViewModels;

namespace InsuranceClaims.Services.Interfaces
{
    public interface IAssessmentService
    {
        Task<List<SurveyorAssignment>> GetAssignedClaimsForSurveyorAsync(int surveyorId);
        Task<(bool Success, string Message)> SubmitAssessmentAsync(AssessmentViewModel model, int userId);
    }
}
