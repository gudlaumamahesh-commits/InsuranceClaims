using InsuranceClaims.Data;
using InsuranceClaims.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceClaims.Repositories
{
    public class AssessmentRepository
    {
        private readonly AppDbContext _db;
        public AssessmentRepository(AppDbContext db) => _db = db;

        public async Task<List<SurveyorAssignment>> GetAssignedClaimsForSurveyorAsync(int surveyorId)
            => await _db.SurveyorAssignments
                .Include(sa => sa.Claim).ThenInclude(c => c!.Customer)
                .Include(sa => sa.Claim).ThenInclude(c => c!.Policy)
                .Include(sa => sa.Claim).ThenInclude(c => c!.Documents)
                .Include(sa => sa.Claim).ThenInclude(c => c!.Assessments) 
                .Where(sa => sa.SurveyorId == surveyorId)
                .ToListAsync();

        public async Task AddAsync(Assessment assessment)
        {
            await _db.Assessments.AddAsync(assessment);
            await _db.SaveChangesAsync();
        }
    }
}
