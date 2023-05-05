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

        private List<FileSystemWatcherDisposable> fsDisposables = new();
        private List<Watcher> watchers = new();

        public bool IsRefreshRequired { get; set; } = false;

        public WatcherManager(ManagerConfiguration managerConfiguration)
        {
            _managerConfiguration = managerConfiguration;
        }

        ~WatcherManager()
        {
            fsDisposables.ForEach(
                delegate (FileSystemWatcherDisposable value)
                {
                    value.Dispose();
                }
            );
        }

        private async Task RetrieveServiceSettings()
        {

        }

        private void InitFsDisposables()
        {
            fsDisposables = watchers.Select(
                delegate (Watcher watcher)
                {
                    return new FileSystemWatcherDisposable(watcher);
                }
             ).ToList();
            IsRefreshRequired = true;
        }

        private void StartFsDisposables()
        {
            fsDisposables.ForEach(
                delegate (FileSystemWatcherDisposable value)
                {
                    if (value.fsWatcher == null)
                    {
                        fsDisposables.Remove(value);
                        value.Dispose();
                        return;
                    }
                    value.fsWatcher.EnableRaisingEvents = true;
                }
            );
            IsRefreshRequired = false;
        }

        private void StopFsDisposables()
        {
            fsDisposables.ForEach(
                delegate (FileSystemWatcherDisposable value)
                {
                    value.Dispose();
                }
            );
            fsDisposables = new();
        }

        private void RefreshFsDisposables()
        {
            StopFsDisposables();
            InitFsDisposables();
        }

        public void RefreshWatchers(List<Watcher> newWatchers)
        {
            watchers = newWatchers;
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