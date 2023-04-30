using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.Sqlite;
using System.ComponentModel;
using System.Diagnostics;

namespace Configuration
{
    public class ManagerConfiguration
    {
        private ConfigurationContext _context;
        private readonly ILogger<ManagerConfiguration> _logger;
        private readonly string appDirectoryPath;

        private List<Watcher> cachedWatchers = new List<Watcher>();

        public ManagerConfiguration(ILogger<ManagerConfiguration> logger) { 
            _logger = logger;

            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);

            appDirectoryPath = Path.Join(path, "fsBuddy");

            var dbPath = Path.Join(appDirectoryPath, "fsBuddy.config.db");

            var optionsBuilder = new DbContextOptionsBuilder<ConfigurationContext>();
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            
            _context = new ConfigurationContext(optionsBuilder.Options);

            Directory.CreateDirectory(appDirectoryPath);
            _context.Database.EnsureCreated();
        }

        ~ManagerConfiguration()
        {
            _context.Dispose();
        }

        public async void CreateWatcher(Watcher watcher)
        {
            if (_context.Watchers.Where(_watcher => _watcher.Name == watcher.Name).Count() > 0)
            {
                _logger.LogInformation("Watchers already exist");
                return;
            }

            await _context.Watchers.AddAsync(watcher);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Watcher>> GetWatchers()
        {
            return await _context.Watchers.ToListAsync();
        }

        public async Task<bool> WatchersChanged()
        {
            var watchers = await GetWatchers();
            var result = false;

            if (!Enumerable.SequenceEqual(watchers, cachedWatchers))
            {
                result = true;
            }
            cachedWatchers = watchers;
            return result;
        }
    }
}
