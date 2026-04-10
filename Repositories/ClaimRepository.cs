using InsuranceClaims.Data;
using InsuranceClaims.Enums;
using InsuranceClaims.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceClaims.Repositories
{
    public class ClaimRepository
    {
        private readonly AppDbContext _db;
        public ClaimRepository(AppDbContext db) => _db = db;

        public async Task<List<Claim>> GetByCustomerAsync(int customerId)
        {
            try
            {
                return await _db.Claims
                    .Include(c => c.Policy)
                    .Where(c => c.CustomerId == customerId)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClaimRepository.GetByCustomerAsync] {ex.Message}");
                return new List<Claim>();
            }
        }

        public async Task<List<Claim>> GetAllAsync()
        {
            try
            {
                return await _db.Claims
                    .Include(c => c.Policy)
                    .Include(c => c.Customer)
                    .Include(c => c.FraudCheck)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClaimRepository.GetAllAsync] {ex.Message}");
                return new List<Claim>();
            }
        }

        public async Task<Claim?> GetByIdAsync(int id)
        {
            try
            {
                return await _db.Claims
                    .Include(c => c.Policy)
                    .Include(c => c.Customer)
                    .Include(c => c.Documents)
                    .Include(c => c.Assessments).ThenInclude(a => a.Surveyor)
                    .Include(c => c.FraudCheck)
                    .Include(c => c.SettlementLog)
                    .Include(c => c.TrackingHistory)
                    .FirstOrDefaultAsync(c => c.ClaimId == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClaimRepository.GetByIdAsync] {ex.Message}");
                return null;
            }
        }

        public async Task AddAsync(Claim claim)
        {
            try
            {
                await _db.Claims.AddAsync(claim);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClaimRepository.AddAsync] {ex.Message}");
                throw;
            }
        }

        public async Task UpdateAsync(Claim claim)
        {
            try
            {
                _db.Claims.Update(claim);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClaimRepository.UpdateAsync] {ex.Message}");
                throw;
            }
        }

        public async Task AddTrackingAsync(ClaimTracking tracking)
        {
            try
            {
                await _db.ClaimTrackings.AddAsync(tracking);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClaimRepository.AddTrackingAsync] {ex.Message}");
                throw;
            }
        }

        public async Task<List<Claim>> GetByStatusAsync(ClaimStatus status)
        {
            try
            {
                return await _db.Claims
                    .Include(c => c.Customer)
                    .Include(c => c.Policy)
                    .Where(c => c.ClaimStatus == status)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClaimRepository.GetByStatusAsync] {ex.Message}");
                return new List<Claim>();
            }
        }
    }
}
