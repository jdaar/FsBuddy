using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ConnectionInterface;

namespace Configuration
{
    public class ConfigurationContextFactory : IDesignTimeDbContextFactory<ConfigurationContext>
    {
        public ConfigurationContext CreateDbContext(string[] args)
        {
            var _folder = Environment.SpecialFolder.LocalApplicationData;
            var _path = Environment.GetFolderPath(_folder);
            var _dbPath = System.IO.Path.Join(_path, "fsBuddy.config.db");

            var optionsBuilder = new DbContextOptionsBuilder<ConfigurationContext>();
            optionsBuilder.UseSqlite($"Data Source={_dbPath}");

            return new ConfigurationContext(optionsBuilder.Options);
        }
    }

    public class ConfigurationContext : DbContext
    {
        public DbSet<Watcher> Watchers { get; set; }
        public DbSet<ServiceSetting> SystemSettings { get; set; }

        public ConfigurationContext(DbContextOptions<ConfigurationContext> options) : base(options) {}

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