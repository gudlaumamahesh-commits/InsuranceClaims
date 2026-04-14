using InsuranceClaims.Enums;
using InsuranceClaims.Models.Entities;
using InsuranceClaims.Models.ViewModels;
using InsuranceClaims.Repositories;
using InsuranceClaims.Services.Interfaces;

namespace InsuranceClaims.Services.Implementations
{
    public class ClaimService : IClaimService
    {
        private readonly ClaimRepository    _claimRepo;
        private readonly CustomerRepository _customerRepo;
        private readonly PolicyRepository   _policyRepo;

        public ClaimService(ClaimRepository claimRepo, CustomerRepository customerRepo, PolicyRepository policyRepo)
        {
            _claimRepo    = claimRepo;
            _customerRepo = customerRepo;
            _policyRepo   = policyRepo;
        }

        public async Task<List<Claim>> GetMyClaimsAsync(int userId)
        {
            var customer = await _customerRepo.GetByUserIdAsync(userId);
            if (customer == null) return new List<Claim>();
            return await _claimRepo.GetByCustomerAsync(customer.CustomerId);
        }

        public async Task<List<Claim>> GetAllClaimsAsync() => await _claimRepo.GetAllAsync();

        public async Task<Claim?> GetClaimDetailsAsync(int claimId) => await _claimRepo.GetByIdAsync(claimId);

        public async Task<List<PolicyPurchase>> GetEligiblePoliciesForClaimAsync(int userId)
        {
            var customer = await _customerRepo.GetByUserIdAsync(userId);
            if (customer == null) return new List<PolicyPurchase>();

            var purchases = await _policyRepo.GetPurchasesByCustomerAsync(customer.CustomerId);
            var myClaims  = await _claimRepo.GetByCustomerAsync(customer.CustomerId);

            var alreadyClaimedPolicyIds = myClaims
                .Where(c => c.ClaimStatus == ClaimStatus.REGISTERED ||
                            c.ClaimStatus == ClaimStatus.UNDER_REVIEW)
                .Select(c => c.PolicyId)
                .ToHashSet();

            return purchases
                .Where(p => p.EndDate >= DateTime.Now && !alreadyClaimedPolicyIds.Contains(p.PolicyId))
                .ToList();
        }

        public async Task<(bool Success, string Message)> CreateClaimAsync(CreateClaimViewModel model, int userId)
        {
            var customer = await _customerRepo.GetByUserIdAsync(userId);
            if (customer == null) return (false, "Customer profile not found.");

            var purchases = await _policyRepo.GetPurchasesByCustomerAsync(customer.CustomerId);
            bool hasActive = purchases.Any(p => p.PolicyId == model.PolicyId && p.EndDate >= DateTime.Now);
            if (!hasActive) return (false, "You do not have an active purchase for this policy.");
			var policy = await _policyRepo.GetByIdAsync(model.PolicyId);

			if (policy != null && model.ClaimAmount > policy.CoverageAmount)
				return (false, $"Claim amount ₹{model.ClaimAmount:N0} cannot exceed the policy coverage of ₹{policy.CoverageAmount:N0}. Please enter a valid amount.");

			var myClaims = await _claimRepo.GetByCustomerAsync(customer.CustomerId);
            bool hasPending = myClaims.Any(c => c.PolicyId == model.PolicyId &&
                (c.ClaimStatus == ClaimStatus.REGISTERED || c.ClaimStatus == ClaimStatus.UNDER_REVIEW));
            if (hasPending) return (false, "You already have an active claim for this policy. Please wait for it to be resolved before filing another.");

            var claim = new Claim
            {
                PolicyId    = model.PolicyId,
                CustomerId  = customer.CustomerId,
                ClaimType   = model.ClaimType,
                ClaimAmount = model.ClaimAmount,
                ClaimStatus = ClaimStatus.REGISTERED,
                CreatedAt   = DateTime.Now
            };
            await _claimRepo.AddAsync(claim);

            await _claimRepo.AddTrackingAsync(new ClaimTracking
            {
                ClaimId   = claim.ClaimId,
                Status    = "REGISTERED",
                Remarks   = "Claim registered by customer.",
                UpdatedAt = DateTime.Now
            });

            return (true, "Claim submitted successfully. Please upload supporting documents.");
        }

        public async Task UpdateClaimStatusAsync(int claimId, ClaimStatus status, string remarks)
        {
            var claim = await _claimRepo.GetByIdAsync(claimId);
            if (claim == null) return;
            claim.ClaimStatus = status;
            await _claimRepo.UpdateAsync(claim);
            await _claimRepo.AddTrackingAsync(new ClaimTracking
            {
                ClaimId   = claimId,
                Status    = status.ToString(),
                Remarks   = remarks,
                UpdatedAt = DateTime.Now
            });
        }
    }
}
