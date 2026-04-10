using InsuranceClaims.Data;
using InsuranceClaims.Enums;
using InsuranceClaims.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceClaims.Repositories
{
    public class FraudRepository
    {
        private readonly AppDbContext _db;
        public FraudRepository(AppDbContext db) => _db = db;

        /// Claims eligible for fraud check:
        /// - Has documents, all VERIFIED
        /// - Has at least one assessment
        /// - No fraud check done yet
        public async Task<List<Claim>> GetClaimsReadyForFraudCheckAsync()
            => await _db.Claims
                .Include(c => c.Customer)
                .Include(c => c.Policy)
                .Include(c => c.Documents)
                .Include(c => c.Assessments)
                .Where(c =>
                    c.ClaimStatus == ClaimStatus.UNDER_REVIEW &&
                    c.Documents.Any() &&
                    c.Documents.All(d => d.VerificationStatus == VerificationStatus.VERIFIED) &&
                    c.Assessments.Any() &&
                    !_db.FraudChecks.Any(f => f.ClaimId == c.ClaimId))
                .ToListAsync();

        public async Task<FraudCheck?> GetByClaimIdAsync(int claimId)
            => await _db.FraudChecks.FirstOrDefaultAsync(f => f.ClaimId == claimId);

        public async Task AddAsync(FraudCheck fc)
        {
            await _db.FraudChecks.AddAsync(fc);
            await _db.SaveChangesAsync();
        }
    }
}
