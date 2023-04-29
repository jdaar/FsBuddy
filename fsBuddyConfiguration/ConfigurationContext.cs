using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Configuration
{
    public class ConfigurationContext : DbContext
    {
        public ConfigurationContext(DbContextOptions<ConfigurationContext> options): base(options) { }

        public DbSet<Watcher> Watchers { get; set; }

        public override int SaveChanges()
        {
            var now = DateTime.UtcNow;
            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
            {
                entry.Property("CreatedAt").CurrentValue = now;
            }
            return base.SaveChanges();
        }
    }
}