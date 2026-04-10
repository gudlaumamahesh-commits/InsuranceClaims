using InsuranceClaims.Models.Entities;
using InsuranceClaims.Models.ViewModels;

namespace InsuranceClaims.Services.Interfaces
{
    public interface IAdminService
    {
        Task<List<Officer>>  GetOfficersAsync();
        Task<List<Surveyor>> GetSurveyorsAsync();
        Task<(bool Success, string Message)> AddOfficerAsync(AddOfficerViewModel model);
        Task<(bool Success, string Message)> AddSurveyorAsync(AddSurveyorViewModel model);
        Task<(bool Success, string Message)> ToggleOfficerActiveAsync(int officerId);
        Task<(bool Success, string Message)> ToggleSurveyorActiveAsync(int surveyorId);
        Task DeleteOfficerAsync(int officerId);
        Task DeleteSurveyorAsync(int surveyorId);
    }
}
