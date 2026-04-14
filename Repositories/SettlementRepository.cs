using InsuranceClaims.Data;
using InsuranceClaims.Enums;
using InsuranceClaims.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceClaims.Repositories
{
    public class SettlementRepository
    {
        private readonly AppDbContext _db;
        public SettlementRepository(AppDbContext db) => _db = db;

        public async Task<List<Claim>> GetApprovedClaimsWithoutSettlementAsync()
        {
            try
            {
                return await _db.Claims
                    .Include(c => c.Customer)
                    .Include(c => c.FraudCheck)
                    .Include(c => c.Policy)
                    .Where(c => c.ClaimStatus == ClaimStatus.APPROVED
                             && !_db.SettlementLogs.Any(s => s.ClaimId == c.ClaimId))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SettlementRepository.GetApprovedClaimsWithoutSettlementAsync] {ex.Message}");
                return new List<Claim>();
            }
        }

        public async Task<List<Claim>> GetClaimsReadyForAdminActionAsync()
        {
            try
            {
                return await _db.Claims
                    .Include(c => c.Customer)
                    .Include(c => c.FraudCheck)
                    .Include(c => c.Policy)
                    .Include(c => c.SettlementLog)
                    .Where(c =>
                        (c.ClaimStatus == ClaimStatus.UNDER_REVIEW && _db.FraudChecks.Any(f => f.ClaimId == c.ClaimId)) ||
                        (c.ClaimStatus == ClaimStatus.APPROVED && !_db.SettlementLogs.Any(s => s.ClaimId == c.ClaimId))
                    )
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SettlementRepository.GetClaimsReadyForAdminActionAsync] {ex.Message}");
                return new List<Claim>();
            }
        }

        public async Task<SettlementLog?> GetByClaimIdAsync(int claimId)
        {
            try
            {
                return await _db.SettlementLogs.FirstOrDefaultAsync(s => s.ClaimId == claimId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SettlementRepository.GetByClaimIdAsync] {ex.Message}");
                return null;
            }
        }

        public async Task AddAsync(SettlementLog log)
        {
            try
            {
                await _db.SettlementLogs.AddAsync(log);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SettlementRepository.AddAsync] {ex.Message}");
                throw;
            }
        }
    }
}
