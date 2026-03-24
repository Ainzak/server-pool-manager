using Microsoft.EntityFrameworkCore;
using KaspTestTask.Models;

namespace KaspTestTask.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Server>()
                .Property(s => s.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        }
    }
}
