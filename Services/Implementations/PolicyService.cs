using InsuranceClaims.Models.Entities;
using InsuranceClaims.Models.ViewModels;
using InsuranceClaims.Repositories;
using InsuranceClaims.Services.Interfaces;

namespace InsuranceClaims.Services.Implementations
{
    public class PolicyService : IPolicyService
    {
        private readonly PolicyRepository   _policyRepo;
        private readonly CustomerRepository _customerRepo;

        public PolicyService(PolicyRepository policyRepo, CustomerRepository customerRepo)
        {
            _policyRepo   = policyRepo;
            _customerRepo = customerRepo;
        }

        public async Task<List<Policy>> GetAllPoliciesAsync() => await _policyRepo.GetAllAsync();
        public async Task<Policy?> GetPolicyByIdAsync(int id) => await _policyRepo.GetByIdAsync(id);
        public async Task<PolicyPurchase?> GetPurchaseByIdAsync(int purchaseId) => await _policyRepo.GetPurchaseByIdAsync(purchaseId);

        public async Task<List<PolicyPurchase>> GetMyPoliciesAsync(int userId)
        {
            var customer = await _customerRepo.GetByUserIdAsync(userId);
            if (customer == null) return new List<PolicyPurchase>();
            return await _policyRepo.GetPurchasesByCustomerAsync(customer.CustomerId);
        }

        public async Task<(bool Success, string Message)> BuyPolicyAsync(int policyId, int userId)
        {
            var customer = await _customerRepo.GetByUserIdAsync(userId);
            if (customer == null) return (false, "Customer profile not found.");

            var policy = await _policyRepo.GetByIdAsync(policyId);
            if (policy == null) return (false, "Policy not found.");

            
            var existing = await _policyRepo.GetPurchasesByCustomerAsync(customer.CustomerId);
            bool alreadyOwns = existing.Any(p => p.PolicyId == policyId && p.EndDate >= DateTime.Now);
            if (alreadyOwns)
                return (false, $"You already have an active '{policy.PolicyName}' policy. You can renew it from My Policies. You may buy a different policy.");

            var purchase = new PolicyPurchase
            {
                PolicyId   = policyId,
                CustomerId = customer.CustomerId,
                StartDate  = DateTime.Now,
                EndDate    = DateTime.Now.AddYears(1)
            };
            await _policyRepo.AddPurchaseAsync(purchase);
            return (true, $"Policy '{policy.PolicyName}' purchased successfully! Valid till {purchase.EndDate:dd MMM yyyy}.");
        }

        public async Task<(bool Success, string Message)> RenewPolicyAsync(int purchaseId)
        {
            var purchase = await _policyRepo.GetPurchaseByIdAsync(purchaseId);
            if (purchase == null) return (false, "Purchase record not found.");
            var renewal = new PolicyRenewal
            {
                PurchaseId  = purchaseId,
                RenewalDate = DateTime.Now,
                NewEndDate  = purchase.EndDate.AddYears(1)
            };
            await _policyRepo.AddRenewalAsync(renewal);
            return (true, $"Policy '{purchase.Policy?.PolicyName}' renewed! New end date: {renewal.NewEndDate:dd MMM yyyy}.");
        }

        public async Task<(bool Success, string Message)> AddPolicyAsync(AddPolicyViewModel model)
        {
            var policy = new Policy { PolicyName = model.PolicyName.Trim(), CoverageAmount = model.CoverageAmount, Description = model.Description?.Trim() };
            await _policyRepo.AddPolicyAsync(policy);
            return (true, $"Policy '{policy.PolicyName}' added successfully.");
        }

        public async Task<(bool Success, string Message)> DeletePolicyAsync(int policyId)
        {
            var deleted = await _policyRepo.DeletePolicyAsync(policyId);
            return deleted ? (true, "Policy deleted.") : (false, "Cannot delete — customers have already purchased it, or policy not found.");
        }
    }
}
