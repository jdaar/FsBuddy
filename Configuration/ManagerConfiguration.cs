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
using ConnectionInterface;
using System.Runtime.CompilerServices;
using System.Formats.Asn1;

namespace Configuration
{
    public class ManagerConfiguration
    {
        private ConfigurationContext _context;
        private readonly string appDirectoryPath;

        public ManagerConfiguration() { 
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

        public async Task CreateWatcher(Watcher watcher)
        {
            if (_context.Watchers.Where(_watcher => _watcher.Name == watcher.Name).Count() > 0)
            {
                return;
            }
            if (!Directory.Exists(watcher.InputPath) || !Directory.Exists(watcher.OutputPath))
            {
                return;
            }

            await _context.Watchers.AddAsync(watcher);
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        public async Task<List<Watcher>> GetWatchers()
        {
            return await _context.Watchers.ToListAsync();
        }
        public async Task<Watcher> GetWatcher(int watcherId)
        {
            return await _context.Watchers.Where(watcher => watcher.Id == watcherId).FirstAsync();
        }

        public async Task UpdateWatcher(int watcherId, Watcher watcherData)
        {
            var watcher = await _context.Watchers.SingleOrDefaultAsync(watcher => watcher.Id == watcherId);
            if (watcher == null)
            {
                return;
            }

            watcher.Name = watcherData.Name;
            watcher.InputPath = watcherData.InputPath;
            watcher.OutputPath = watcherData.OutputPath;
            watcher.SearchPattern = watcherData.SearchPattern;
            watcher.IsEnabled = watcherData.IsEnabled;
            watcher.Action = watcherData.Action;

            await _context.SaveChangesAsync(CancellationToken.None);
        }

        public async Task DeleteWatcher(int watcherId)
        {
            var watcher = await _context.Watchers.SingleOrDefaultAsync(watcher => watcher.Id == watcherId);
            if (watcher == null)
            {
                return;
            }

            _context.Watchers.Remove(watcher);
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        public async Task RegisterExecution(int watcherId)
        {
            var now = DateTime.UtcNow;
            var watcher = await _context.Watchers.SingleOrDefaultAsync(watcher => watcher.Id == watcherId);
            if (watcher == null)
            {
                return;
            }
            watcher.ExecutedAt = now;
            watcher.ModifiedFiles = watcher.ModifiedFiles + 1;

            await _context.SaveChangesAsync(CancellationToken.None);
        }

        public async Task<ServiceSetting?> GetServiceSetting(t_Setting type)
        {
            return await _context.SystemSettings.Where(setting => setting.Type == type).FirstOrDefaultAsync();
        }

        public async Task<bool> ServiceSettingsExists()
        {
            return await _context.SystemSettings.CountAsync() != 0;
        }

        public async Task CreateDefaultServiceSettings()
        {
            var ThreadNumberSetting = new ServiceSetting { Type = t_Setting.THREAD_NUMBER , Value = 2 };

            await _context.SystemSettings.AddAsync(ThreadNumberSetting);
            await _context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
