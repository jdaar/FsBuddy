using Serilog;
using Configuration;
using Serilog.Context;
using ConnectionInterface;
using System.Linq.Expressions;

namespace Service
{

    class WatcherManager : BackgroundService
    {
        private readonly ManagerConfiguration _managerConfiguration;

        private Dictionary<int, FileSystemWatcherDisposable> fsDisposables = new();
        private List<Watcher> watchers = new();

        public bool IsRefreshRequired { get; set; } = false;

        public WatcherManager(ManagerConfiguration managerConfiguration)
        {
            _managerConfiguration = managerConfiguration;
        }

        ~WatcherManager()
        {
            foreach (var keyValuePair in fsDisposables)
            {
                keyValuePair.Value.Dispose();
            }
        }

        private async Task RetrieveServiceSettings()
        {

        }

        private void InitFsDisposables()
        {
            fsDisposables = watchers.Select(
                delegate (Watcher watcher)
                {
                    return new KeyValuePair<int, FileSystemWatcherDisposable>(watcher.Id, new FileSystemWatcherDisposable(watcher, _managerConfiguration.RegisterExecution));
                }
             ).ToDictionary(x => x.Key, x => x.Value);
            IsRefreshRequired = true;
        }

        public bool IsFsDisposableRunning(int watcherId)
        {
            return fsDisposables.ContainsKey(watcherId);
        }

        public void StopFsDisposable(int watcherId)
        {
            fsDisposables.GetValueOrDefault(watcherId)?.Dispose();
            fsDisposables.Remove(watcherId);
        }
        public async Task StartFsDisposable(int watcherId)
        {
            if (fsDisposables.GetValueOrDefault(watcherId) != null)
            {
                return; 
            }

            var watcher = await _managerConfiguration.GetWatcher(watcherId);

            if (watcher == null)
            {
                return;
            }

            fsDisposables.Add(
                watcher.Id,
                new FileSystemWatcherDisposable(watcher, _managerConfiguration.RegisterExecution)
            );
        }

        private void StartFsDisposables()
        {
            foreach (var keyValuePair in fsDisposables)
            {
                if (keyValuePair.Value.fsWatcher == null)
                {
                    fsDisposables.Remove(keyValuePair.Key);
                    keyValuePair.Value.Dispose();
                    return;
                }
                keyValuePair.Value.fsWatcher.EnableRaisingEvents = true;
            }
            IsRefreshRequired = false;
        }

        private void StopFsDisposables()
        {
            foreach (var keyValuePair in fsDisposables)
            {
                keyValuePair.Value.Dispose();
            }
            fsDisposables = new();
        }

        private void RefreshFsDisposables()
        {
            StopFsDisposables();
            InitFsDisposables();
        }

        public void RefreshWatchers(List<Watcher> newWatchers)
        {
            watchers = newWatchers.Where(watcher => watcher.IsEnabled ?? false).ToList();
            RefreshFsDisposables();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RetrieveServiceSettings();

            watchers = await _managerConfiguration.GetWatchers();
            RefreshFsDisposables();
            
            while (!stoppingToken.IsCancellationRequested)
            {
                if (IsRefreshRequired)
                {
                    StartFsDisposables();
                }
            }
        }
    }
}