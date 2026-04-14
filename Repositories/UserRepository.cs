using InsuranceClaims.Data;
using InsuranceClaims.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceClaims.Repositories
{
    public class UserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) => _db = db;

        public async Task<User?> GetByEmailAsync(string email)
            => await _db.Users
                .Include(u => u.Officer)
                .Include(u => u.Surveyor)
                .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant().Trim());

        public async Task AddAsync(User user)
        {
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
        }
    }
}
