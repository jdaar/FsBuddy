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
using Microsoft.Extensions.DependencyInjection;

namespace Configuration
{
    public class ManagerConfiguration: IDisposable
    {
        private readonly IServiceScope _scope;
        private readonly ConfigurationContext _context;

        public ManagerConfiguration(IServiceProvider provider) {
            _scope = provider.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<ConfigurationContext>();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task CreateWatcher(Watcher watcher)
        {
            if (_context.Watchers.Where(_watcher => _watcher.Name == watcher.Name).Any())
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
            watcher.ModifiedFiles++;

            await _context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
