using InsuranceClaims.Models.Entities;
using InsuranceClaims.Models.ViewModels;

namespace InsuranceClaims.Services.Interfaces
{
    public interface IPolicyService
    {
        Task<List<Policy>> GetAllPoliciesAsync();
        Task<Policy?> GetPolicyByIdAsync(int id);
        Task<List<PolicyPurchase>> GetMyPoliciesAsync(int userId);
        Task<PolicyPurchase?> GetPurchaseByIdAsync(int purchaseId);
        Task<(bool Success, string Message)> BuyPolicyAsync(int policyId, int userId);
        Task<(bool Success, string Message)> RenewPolicyAsync(int purchaseId);
        Task<(bool Success, string Message)> AddPolicyAsync(AddPolicyViewModel model);
        Task<(bool Success, string Message)> DeletePolicyAsync(int policyId);
    }
}
