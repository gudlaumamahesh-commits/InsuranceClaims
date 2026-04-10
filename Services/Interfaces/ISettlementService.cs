using InsuranceClaims.Models.Entities;
using InsuranceClaims.Models.ViewModels;

namespace InsuranceClaims.Services.Interfaces
{
    public interface ISettlementService
    {
        Task<List<Claim>> GetClaimsForSettlementAsync();          // APPROVED, no settlement yet
        Task<List<Claim>> GetClaimsReadyForAdminAsync();          // Fraud-checked OR Approved (for Admin dashboard)
        Task<(bool Success, string Message)> ProcessSettlementAsync(SettlementViewModel model);
        Task<(bool Success, string Message)> ApproveClaimAsync(int claimId, IClaimService claimService);
        Task<(bool Success, string Message)> RejectClaimAsync(int claimId, string remarks, IClaimService claimService);
    }
}
