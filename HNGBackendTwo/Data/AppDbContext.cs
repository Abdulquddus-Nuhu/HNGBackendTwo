using HNGBackendTwo.Models;
using Microsoft.EntityFrameworkCore;

namespace HNGBackendTwo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Organisation> Organisations { get; set; }
        public DbSet<OrganisationUser> OrganisationUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrganisationUser>()
                .HasKey(ou => new { ou.OrganisationId, ou.UserId });

            modelBuilder.Entity<OrganisationUser>()
                .HasOne(ou => ou.Organisation)
                .WithMany(o => o.OrganisationUsers)
                .HasForeignKey(ou => ou.OrganisationId);

            modelBuilder.Entity<OrganisationUser>()
                .HasOne(ou => ou.User)
                .WithMany(u => u.OrganisationUsers)
                .HasForeignKey(ou => ou.UserId);

        }
    }
}
