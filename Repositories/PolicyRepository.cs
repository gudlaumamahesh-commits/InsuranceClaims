using InsuranceClaims.Data;
using InsuranceClaims.Enums;
using InsuranceClaims.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceClaims.Repositories
{
    public class PolicyRepository
    {
        private readonly AppDbContext _db;
        public PolicyRepository(AppDbContext db) => _db = db;

        public async Task<List<Policy>> GetAllAsync()
            => await _db.Policies.OrderBy(p => p.PolicyName).ToListAsync();

        public async Task<Policy?> GetByIdAsync(int id)
            => await _db.Policies.FindAsync(id);

        public async Task<List<PolicyPurchase>> GetPurchasesByCustomerAsync(int customerId)
            => await _db.PolicyPurchases
                .Include(p => p.Policy)
                .Include(p => p.Renewals)
                .Where(p => p.CustomerId == customerId)
                .ToListAsync();

        public async Task<List<Claim>> GetClaimsByCustomerAndPolicyAsync(int customerId, int policyId)
            => await _db.Claims
                .Include(c => c.SettlementLog)
                .Where(c => c.CustomerId == customerId && c.PolicyId == policyId)
                .ToListAsync();

        public async Task<PolicyPurchase?> GetPurchaseByIdAsync(int purchaseId)
            => await _db.PolicyPurchases
                .Include(p => p.Policy)
                .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId);

        public async Task AddPurchaseAsync(PolicyPurchase purchase)
        {
            await _db.PolicyPurchases.AddAsync(purchase);
            await _db.SaveChangesAsync();
        }

        public async Task AddRenewalAsync(PolicyRenewal renewal)
        {
            var purchase = await _db.PolicyPurchases.FindAsync(renewal.PurchaseId);
            if (purchase != null)
            {
                purchase.EndDate = renewal.NewEndDate;
                await _db.PolicyRenewals.AddAsync(renewal);
                await _db.SaveChangesAsync();
            }
        }

        public async Task AddPolicyAsync(Policy policy)
        {
            await _db.Policies.AddAsync(policy);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> DeletePolicyAsync(int policyId)
        {
            var policy = await _db.Policies.FindAsync(policyId);
            if (policy == null) return false;
            bool hasPurchases = await _db.PolicyPurchases.AnyAsync(p => p.PolicyId == policyId);
            if (hasPurchases) return false;
            _db.Policies.Remove(policy);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
