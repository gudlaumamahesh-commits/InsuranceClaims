using InsuranceClaims.Data;
using InsuranceClaims.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceClaims.Repositories
{
    public class SurveyorAssignmentRepository
    {
        private readonly AppDbContext _db;
        public SurveyorAssignmentRepository(AppDbContext db) => _db = db;

        public async Task<SurveyorAssignment?> GetByClaimIdAsync(int claimId)
            => await _db.SurveyorAssignments
                .Include(sa => sa.Surveyor)
                .Include(sa => sa.Officer)
                .FirstOrDefaultAsync(sa => sa.ClaimId == claimId);

        public async Task<List<SurveyorAssignment>> GetAllWithDetailsAsync()
            => await _db.SurveyorAssignments
                .Include(sa => sa.Claim).ThenInclude(c => c!.Customer)
                .Include(sa => sa.Claim).ThenInclude(c => c!.Policy)
                .Include(sa => sa.Surveyor).ThenInclude(s => s!.User)
                .Include(sa => sa.Officer).ThenInclude(o => o!.User)
                .OrderByDescending(sa => sa.AssignedAt)
                .ToListAsync();

        public async Task AddAsync(SurveyorAssignment assignment)
        {
            await _db.SurveyorAssignments.AddAsync(assignment);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ExistsForClaimAsync(int claimId)
            => await _db.SurveyorAssignments.AnyAsync(sa => sa.ClaimId == claimId);
    }
}
