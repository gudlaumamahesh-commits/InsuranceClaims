using InsuranceClaims.Enums;
using InsuranceClaims.Models.Entities;
using InsuranceClaims.Models.ViewModels;

namespace InsuranceClaims.Services.Interfaces
{
    public interface IClaimService
    {
        Task<List<Claim>> GetMyClaimsAsync(int userId);
        Task<List<Claim>> GetAllClaimsAsync();
        Task<Claim?> GetClaimDetailsAsync(int claimId);
        Task<List<PolicyPurchase>> GetEligiblePoliciesForClaimAsync(int userId);
        Task<(bool Success, string Message)> CreateClaimAsync(CreateClaimViewModel model, int userId);
        Task UpdateClaimStatusAsync(int claimId, ClaimStatus status, string remarks);
    }
}
