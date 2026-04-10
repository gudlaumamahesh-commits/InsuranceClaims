using InsuranceClaims.Data;
using InsuranceClaims.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceClaims.Repositories
{
    public class OfficerRepository
    {
        private readonly AppDbContext _db;
        public OfficerRepository(AppDbContext db) => _db = db;

        public async Task<List<Officer>> GetAllAsync()
            => await _db.Officers.Include(o => o.User).OrderBy(o => o.Name).ToListAsync();

        public async Task<List<Officer>> GetActiveAsync()
            => await _db.Officers.Include(o => o.User).Where(o => o.IsActive).ToListAsync();

        public async Task<Officer?> GetByUserIdAsync(int userId)
            => await _db.Officers.FirstOrDefaultAsync(o => o.UserId == userId);

        public async Task<Officer?> GetByIdAsync(int id)
            => await _db.Officers.Include(o => o.User).FirstOrDefaultAsync(o => o.OfficerId == id);

        public async Task AddAsync(Officer officer)
        {
            await _db.Officers.AddAsync(officer);
            await _db.SaveChangesAsync();
        }

        public async Task<(bool IsNowActive, string Message)> ToggleActiveAsync(int officerId)
        {
            var officer = await _db.Officers.FindAsync(officerId);
            if (officer == null) return (false, "Officer not found.");

            officer.IsActive = !officer.IsActive;
            if (officer.IsActive)
                officer.ReactivatedAt = DateTime.Now;
            else
                officer.DeactivatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return (officer.IsActive, officer.IsActive ? "Officer activated." : "Officer deactivated.");
        }

        public async Task DeleteAsync(int officerId)
        {
            var officer = await _db.Officers.FindAsync(officerId);
            if (officer != null) { _db.Officers.Remove(officer); await _db.SaveChangesAsync(); }
        }
    }
}
