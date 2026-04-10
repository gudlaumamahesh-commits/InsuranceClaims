using InsuranceClaims.Data;
using InsuranceClaims.Enums;
using InsuranceClaims.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceClaims.Repositories
{
    public class DocumentRepository
    {
        private readonly AppDbContext _db;
        public DocumentRepository(AppDbContext db) => _db = db;

        public async Task<List<ClaimDocument>> GetByClaimAsync(int claimId)
            => await _db.ClaimDocuments
                .Where(d => d.ClaimId == claimId)
                .ToListAsync();

        //All pending docs regardless of assignment 
        public async Task<List<ClaimDocument>> GetPendingAsync()
            => await _db.ClaimDocuments
                .Include(d => d.Claim).ThenInclude(c => c!.Customer)
                .Include(d => d.Claim).ThenInclude(c => c!.Policy)
                .Where(d => d.VerificationStatus == VerificationStatus.PENDING)
                .ToListAsync();

        // Pending docs ONLY for claims assigned to a specific surveyor
        public async Task<List<ClaimDocument>> GetPendingForSurveyorAsync(int surveyorId)
            => await _db.ClaimDocuments
                .Include(d => d.Claim).ThenInclude(c => c!.Customer)
                .Include(d => d.Claim).ThenInclude(c => c!.Policy)
                .Where(d => d.VerificationStatus == VerificationStatus.PENDING &&
                            _db.SurveyorAssignments.Any(sa =>
                                sa.ClaimId == d.ClaimId && sa.SurveyorId == surveyorId))
                .ToListAsync();

        public async Task<ClaimDocument?> GetByIdAsync(int id)
            => await _db.ClaimDocuments.FindAsync(id);

        public async Task AddAsync(ClaimDocument doc)
        {
            await _db.ClaimDocuments.AddAsync(doc);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(ClaimDocument doc)
        {
            _db.ClaimDocuments.Update(doc);
            await _db.SaveChangesAsync();
        }
    }
}
