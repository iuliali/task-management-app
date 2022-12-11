using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Models;

namespace TaskManagementApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // composite primary key 
            modelBuilder.Entity<TeamMember>()
            .HasKey(tm => new { tm.Id, tm.ApplicationUserId, tm.TeamId });
            modelBuilder.Entity<TeamMember>()
            .HasOne(tm => tm.ApplicationUser)
            .WithMany(tm => tm.TeamMembers)
            .HasForeignKey(tm => tm.ApplicationUserId);
            modelBuilder.Entity<TeamMember>()
            .HasOne(tm => tm.Team)
            .WithMany(tm => tm.TeamMembers)
            .HasForeignKey(tm => tm.TeamId);
        }
    }
}