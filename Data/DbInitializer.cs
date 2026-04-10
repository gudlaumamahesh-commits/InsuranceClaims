using InsuranceClaims.Enums;
using InsuranceClaims.Helpers;
using InsuranceClaims.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceClaims.Data
{
    
    public static class DbInitializer
    {
        public static async Task SeedAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await db.Database.MigrateAsync();

            if (!await db.Users.AnyAsync(u => u.Role == UserRole.Admin))
            {
                PasswordHelper.CreatePasswordHash("Admin@123", out var hash, out var salt);
                var admin = new User
                {
                    Email        = "admin@insurance.com",
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    Role         = UserRole.Admin
                };
                await db.Users.AddAsync(admin);
                await db.SaveChangesAsync();
            }

            if (!await db.Policies.AnyAsync())
            {
                await db.Policies.AddRangeAsync(
                    new Policy { PolicyName = "Basic Health Cover",  CoverageAmount = 100000,  Description = "Basic health insurance covering hospitalization expenses." },
                    new Policy { PolicyName = "Motor Insurance",     CoverageAmount = 500000,  Description = "Comprehensive motor vehicle insurance for accidents and theft." },
                    new Policy { PolicyName = "Home Insurance",      CoverageAmount = 1000000, Description = "Home and property insurance against fire, flood, and disasters." },
                    new Policy { PolicyName = "Term Life Insurance", CoverageAmount = 2500000, Description = "Pure life cover for a fixed term at affordable premiums." }
                );
                await db.SaveChangesAsync();
            }
        }
    }
}
