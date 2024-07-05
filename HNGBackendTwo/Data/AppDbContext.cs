using HNGBackendTwo.Models;
using Microsoft.EntityFrameworkCore;

namespace HNGBackendTwo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<OrganisationModel> Organisations { get; set; }
    }
}
