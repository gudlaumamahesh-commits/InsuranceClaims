using Microsoft.EntityFrameworkCore;
using InsuranceClaims.Models.Entities;

namespace InsuranceClaims.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User>               Users               { get; set; }
        public DbSet<Customer>           Customers           { get; set; }
        public DbSet<Officer>            Officers            { get; set; }
        public DbSet<Surveyor>           Surveyors           { get; set; }
        public DbSet<Policy>             Policies            { get; set; }
        public DbSet<PolicyPurchase>     PolicyPurchases     { get; set; }
        public DbSet<PolicyRenewal>      PolicyRenewals      { get; set; }
        public DbSet<Claim>              Claims              { get; set; }
        public DbSet<ClaimDocument>      ClaimDocuments      { get; set; }
        public DbSet<Assessment>         Assessments         { get; set; }
        public DbSet<FraudCheck>         FraudChecks         { get; set; }
        public DbSet<SettlementLog>      SettlementLogs      { get; set; }
        public DbSet<ClaimTracking>      ClaimTrackings      { get; set; }
        public DbSet<SurveyorAssignment> SurveyorAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<User>().HasOne(u => u.Customer).WithOne(c => c.User)
                .HasForeignKey<Customer>(c => c.UserId);
            modelBuilder.Entity<User>().HasOne(u => u.Officer).WithOne(o => o.User)
                .HasForeignKey<Officer>(o => o.UserId);
            modelBuilder.Entity<User>().HasOne(u => u.Surveyor).WithOne(s => s.User)
                .HasForeignKey<Surveyor>(s => s.UserId);

            modelBuilder.Entity<Claim>().HasOne(c => c.FraudCheck).WithOne(f => f.Claim)
                .HasForeignKey<FraudCheck>(f => f.ClaimId);
            modelBuilder.Entity<Claim>().HasOne(c => c.SettlementLog).WithOne(s => s.Claim)
                .HasForeignKey<SettlementLog>(s => s.ClaimId);

            modelBuilder.Entity<SurveyorAssignment>()
                .HasOne(sa => sa.Claim).WithMany().HasForeignKey(sa => sa.ClaimId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SurveyorAssignment>()
                .HasOne(sa => sa.Surveyor).WithMany().HasForeignKey(sa => sa.SurveyorId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SurveyorAssignment>()
                .HasOne(sa => sa.Officer).WithMany().HasForeignKey(sa => sa.OfficerId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
