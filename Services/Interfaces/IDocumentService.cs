using InsuranceClaims.Enums;
using InsuranceClaims.Models.Entities;

namespace InsuranceClaims.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<List<ClaimDocument>> GetPendingDocumentsAsync();
        Task<List<ClaimDocument>> GetPendingDocumentsForSurveyorAsync(int surveyorId);
        Task<List<ClaimDocument>> GetDocumentsByClaimAsync(int claimId);
        Task<(bool Success, string Message)> UploadDocumentAsync(int claimId, IFormFile file, string documentName);
        Task<(bool Success, string Message)> VerifyDocumentAsync(int documentId, VerificationStatus status);
    }
}
