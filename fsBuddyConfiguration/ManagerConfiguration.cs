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

            await _context.Watchers.AddAsync(watcher);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Watcher>> GetWatchers()
        {
            return await _context.Watchers.ToListAsync();
        }
        public async Task<ServiceSetting?> GetServiceSetting(SettingType type)
        {
            return await _context.SystemSettings.Where(setting => setting.Type == type).FirstOrDefaultAsync();
        }

        public async Task<bool> ServiceSettingsExists()
        {
            return await _context.SystemSettings.CountAsync() != 0;
        }

        public async Task CreateDefaultServiceSettings()
        {
            var ThreadNumberSetting = new ServiceSetting { Type = SettingType.THREAD_NUMBER , Value = 2 };
            await _context.SystemSettings.AddAsync(ThreadNumberSetting);
            await _context.SaveChangesAsync();
        }
    }
}
