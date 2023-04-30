using Serilog;
using Configuration;
using Serilog.Context;

namespace Service
{
    class FileSystemWatcherDisposable : IDisposable
    {
        private bool disposed = false;
        private Watcher _watcher;

        public FileSystemWatcher? fsWatcher;

        public FileSystemWatcherDisposable(Watcher watcher)
        {
            _watcher = watcher;
            using (LogContext.PushProperty("WatcherId", _watcher.Id))
            {
                {
                    if (watcher.InputPath == null)
                    {
                        throw new ArgumentNullException("InputPath cannot be null");
                    }

                    try
                    {
                        fsWatcher = new FileSystemWatcher(watcher.InputPath);
                    }
                    catch (ArgumentException)
                    {
                        fsWatcher = null;
                        throw new ArgumentException($"Path for FsWatcher is not valid: {watcher.InputPath}");
                    }

                    fsWatcher.NotifyFilter = NotifyFilters.Attributes
                                         | NotifyFilters.CreationTime
                                         | NotifyFilters.DirectoryName
                                         | NotifyFilters.FileName
                                         | NotifyFilters.LastAccess
                                         | NotifyFilters.LastWrite
                                         | NotifyFilters.Security
                                         | NotifyFilters.Size;

                    fsWatcher.Created += Created;
                    fsWatcher.Deleted += Deleted;
                    fsWatcher.Renamed += Renamed;
                    fsWatcher.Changed += Changed;

                    fsWatcher.Filter = watcher.SearchPattern ?? "*.*";
                    fsWatcher.IncludeSubdirectories = true;

                    Log.Information("Instantiated FsWatcher for: {InputPath}", watcher.InputPath);
                }
            }
        }
        private void Created(object sender, FileSystemEventArgs e)
        {
            using (LogContext.PushProperty("WatcherId", _watcher.Id))
            {
                Log.Information("File created: {FullPath}", e.FullPath);
            }
        }

        private void Deleted(object sender, FileSystemEventArgs e)
        {
            using (LogContext.PushProperty("WatcherId", _watcher.Id))
            {
                Log.Information("File deleted: {FullPath}", e.FullPath);
            }
        }

        private void Renamed(object sender, FileSystemEventArgs e)
        {
            using (LogContext.PushProperty("WatcherId", _watcher.Id))
            {
                Log.Information("File renamed: {FullPath}", e.FullPath);
            }
        }

        private void Changed(object sender, FileSystemEventArgs e)
        {
            using (LogContext.PushProperty("WatcherId", _watcher.Id))
            {
                Log.Information("File changed: {FullPath}", e.FullPath);
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;
            if (fsWatcher == null)
            {
                disposed = true;
                return;
            }

            fsWatcher.Dispose();

            disposed = true;

            GC.SuppressFinalize(this);
        }
    }

    class WatcherManager : BackgroundService
    {
        private readonly ManagerConfiguration _managerConfiguration;
        private List<FileSystemWatcherDisposable> fsWatchers = new List<FileSystemWatcherDisposable>();

        public WatcherManager(ManagerConfiguration managerConfiguration)
        {
            _managerConfiguration = managerConfiguration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var watchers = await _managerConfiguration.GetWatchers();

            foreach (var watcher in watchers)
            {
                try
                {
                    var fsWatcherDisposable = new FileSystemWatcherDisposable(watcher);

                    fsWatchers.Add(fsWatcherDisposable);

                    if (fsWatcherDisposable.fsWatcher != null)
                    {
                        fsWatcherDisposable.fsWatcher.EnableRaisingEvents = true;
                    } else
                    {
                        fsWatcherDisposable.Dispose();
                        fsWatchers.Remove(fsWatcherDisposable);
                    }
                } catch (ArgumentNullException error)
                {
                    Log.Error("Instantiated FsWatcher for: {InputPath}", watcher.InputPath);
                    Log.Error(error.Message);
                } catch (ArgumentException error)
                {
                    Log.Error("Instantiated FsWatcher for: {InputPath}", watcher.InputPath);
                    Log.Error(error.Message);
                } 
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            fsWatchers.ForEach(fsWatcher => fsWatcher.Dispose());
            fsWatchers = new List<FileSystemWatcherDisposable>();
            await base.StopAsync(stoppingToken);
        }
    }
}