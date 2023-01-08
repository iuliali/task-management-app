using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Models;
using Task = TaskManagementApp.Models.Task;

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
        public DbSet<Project> Projects { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // composite primary key 
            modelBuilder.Entity<TeamMember>()
            .HasKey(tm => new {tm.Id,  tm.ApplicationUserId, tm.TeamId });
            modelBuilder.Entity<TeamMember>()
            .HasOne(tm => tm.ApplicationUser)
            .WithMany(tm => tm.TeamMembers)
            .HasForeignKey(tm => tm.ApplicationUserId);
            modelBuilder.Entity<TeamMember>()
            .HasOne(tm => tm.Team)
            .WithMany(tm => tm.TeamMembers)
            .HasForeignKey(tm => tm.TeamId);

            modelBuilder.Entity<Task>().Property(m => m.UserId).IsRequired(true);




            modelBuilder.Entity<Team>()
            .HasOne<Project>(t => t.Project)
            .WithOne(p => p.Team);
            
        }
    }
}