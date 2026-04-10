using InsuranceClaims.Data;
using InsuranceClaims.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceClaims.Repositories
{
    public class CustomerRepository
    {
        private readonly AppDbContext _db;
        public CustomerRepository(AppDbContext db) => _db = db;

        public async Task<Customer?> GetByUserIdAsync(int userId)
            => await _db.Customers.FirstOrDefaultAsync(c => c.UserId == userId);

        public async Task<Customer?> GetByIdAsync(int id)
            => await _db.Customers.FindAsync(id);
        public async Task<List<Customer>> GetAllWithDetailsAsync()
            => await _db.Customers
                .Include(c => c.User)
                .Include(c => c.PolicyPurchases).ThenInclude(p => p.Policy)
                .Include(c => c.PolicyPurchases).ThenInclude(p => p.Renewals)
                .Include(c => c.Claims)
                .OrderBy(c => c.Name)
                .ToListAsync();

        public async Task AddAsync(Customer customer)
        {
            await _db.Customers.AddAsync(customer);
            await _db.SaveChangesAsync();
        }
    }
}
