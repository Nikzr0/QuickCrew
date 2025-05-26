using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuickCrew.Data.Entities;

namespace QuickCrew.Data
{
    public class QuickCrewContext : IdentityDbContext<User>
    {
        public QuickCrewContext(DbContextOptions<QuickCrewContext> options) : base(options)
        {
        }

        public DbSet<Application> Applications { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            this.ChangeDefaultIdentityTableNames(modelBuilder);

            modelBuilder.Entity<JobPosting>()
                .HasOne(jp => jp.Owner)
                .WithMany(u => u.JobPostings)
                .HasForeignKey(jp => jp.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .Property(r => r.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasOne(r => r.Reviewer)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.ReviewerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.JobPosting)
                    .WithMany(jp => jp.Reviews)
                    .HasForeignKey(r => r.JobPostingId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Application>()
                .HasOne(a => a.JobPosting)
                .WithMany()
                .HasForeignKey(a => a.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Application>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ChangeDefaultIdentityTableNames(ModelBuilder builder)
        {
            builder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
            });
            builder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable("Roles");
            });
            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("UserRoles");
            });
            builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("UserClaims");
            });
            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("UserLogins");
            });
            builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("RoleClaims");
            });
            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("UserTokens");
            });
        }
    }
}