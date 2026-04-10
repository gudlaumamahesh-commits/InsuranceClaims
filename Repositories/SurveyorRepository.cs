using InsuranceClaims.Data;
using InsuranceClaims.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceClaims.Repositories
{
    public class SurveyorRepository
    {
        private readonly AppDbContext _db;
        public SurveyorRepository(AppDbContext db) => _db = db;

        // All surveyors (active + inactive) for admin dashboard
        public async Task<List<Surveyor>> GetAllAsync()
            => await _db.Surveyors.Include(s => s.User).OrderBy(s => s.Name).ToListAsync();

        // Only active surveyors — used in AssignSurveyor dropdown
        public async Task<List<Surveyor>> GetActiveAsync()
            => await _db.Surveyors.Include(s => s.User).Where(s => s.IsActive).ToListAsync();

        public async Task<Surveyor?> GetByUserIdAsync(int userId)
            => await _db.Surveyors.FirstOrDefaultAsync(s => s.UserId == userId);

        public async Task<Surveyor?> GetByIdAsync(int id)
            => await _db.Surveyors.Include(s => s.User).FirstOrDefaultAsync(s => s.SurveyorId == id);

        // Toggle active/inactive instead of delete
        public async Task<(bool IsNowActive, string Message)> ToggleActiveAsync(int surveyorId)
        {
            var surveyor = await _db.Surveyors.FindAsync(surveyorId);
            if (surveyor == null) return (false, "Surveyor not found.");

            surveyor.IsActive = !surveyor.IsActive;
            if (surveyor.IsActive)
                surveyor.ReactivatedAt = DateTime.Now;
            else
                surveyor.DeactivatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return (surveyor.IsActive, surveyor.IsActive ? "Surveyor activated." : "Surveyor deactivated.");
        }

        public async Task AddAsync(Surveyor surveyor)
        {
            await _db.Surveyors.AddAsync(surveyor);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int surveyorId)
        {
            var surveyor = await _db.Surveyors.FindAsync(surveyorId);
            if (surveyor != null) { _db.Surveyors.Remove(surveyor); await _db.SaveChangesAsync(); }
        }
    }
}
