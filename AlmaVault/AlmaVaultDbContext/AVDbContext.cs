using AlmaVault.Models.Domains;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AlmaVault.Data
{
    public class AVDbContext : IdentityDbContext<ApplicationUser,ApplicationRole, Guid>
    {
        public DbSet<ContributionCampaign> ContributionCampaigns { get; set; } = null!;
        public DbSet<AlumniContribution> AlumniContributions { get; set; } = null!;
        public AVDbContext(DbContextOptions<AVDbContext> options) : base(options) { }
        // Master Records & Identity
        public DbSet<HistoricalStudent> HistoricalStudents => Set<HistoricalStudent>();
        public DbSet<MentorshipRequestsDM> MentorshipRequests => Set<MentorshipRequestsDM>();
        public DbSet<MentorshipTasks> MentorshipTask { get; set; }
        public DbSet<MentorshipFeedback> MentorshipFeedbacks { get; set; }
        public DbSet<MentorshipNotes> MentorshipNotes => Set<MentorshipNotes>();
        // Modules
        public DbSet<ContributionLedger> ContributionLedgers => Set<ContributionLedger>();
        public DbSet<JobPosting> JobPostings => Set<JobPosting>();
        public DbSet<MentorshipSession> MentorshipSessions => Set<MentorshipSession>();
        public DbSet<AlumniDirectoryDM> AlumniDirectoryDMs => Set<AlumniDirectoryDM>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Configure default Identity tables
            base.OnModelCreating(modelBuilder);

            // 2. HistoricalStudent Entity Configuration
            modelBuilder.Entity<HistoricalStudent>()
                .HasKey(h => h.StudentIdNumber);

            modelBuilder.Entity<AlumniContribution>()
                .HasOne(c => c.Campaign)
                .WithMany(m => m.Contributions)
                .HasForeignKey(c => c.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
            // 3. ApplicationUser Configuration (RollNumber declared without FK restriction)
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.RollNumber)
                      .IsRequired(false);
            });

            // Configure MentorshipRequest Dual Foreign Keys
            modelBuilder.Entity<MentorshipRequestsDM>()
                .HasOne(m => m.Mentee)
                .WithMany()
                .HasForeignKey(m => m.MenteeId)
                .OnDelete(DeleteBehavior.Restrict); // Prevents circular cascading deletes

            modelBuilder.Entity<MentorshipRequestsDM>()
                .HasOne(m => m.Mentor)
                .WithMany()
                .HasForeignKey(m => m.MentorId)
                .OnDelete(DeleteBehavior.Restrict);
            // 4. ContributionLedger Configuration (Guid PK & Precision)
            modelBuilder.Entity<ContributionLedger>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Amount)
                      .HasPrecision(18, 2);

                entity.HasOne(c => c.User)
                      .WithMany(u => u.Contributions)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 5. JobPosting Configuration (Guid PK)
            modelBuilder.Entity<JobPosting>(entity =>
            {
                entity.HasKey(j => j.Id);

                entity.HasOne(j => j.PostedByUser)
                      .WithMany(u => u.JobPostings)
                      .HasForeignKey(j => j.PostedByUserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 6. MentorshipSession Configuration (Guid PK)
            modelBuilder.Entity<MentorshipSession>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.HasOne(m => m.Mentor)
                      .WithMany(u => u.MentorshipSessions)
                      .HasForeignKey(m => m.MentorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}