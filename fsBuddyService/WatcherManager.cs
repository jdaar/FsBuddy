using Serilog;
using Configuration;
using Serilog.Context;

namespace Service
{
    class FsWatcherDisposable : IDisposable
    {
        private bool disposed = false;
        private IDisposable loggerContext;

        public FileSystemWatcher? fsWatcher;

        public FsWatcherDisposable(Watcher watcher)
        {
            loggerContext = LogContext.PushProperty("WatcherName", watcher.Name);

            {
                if (watcher.InputPath == null)
                {
                    throw new ArgumentNullException("InputPath cannot be null");
                }

                try
                {
                    fsWatcher = new FileSystemWatcher(watcher.InputPath);
                } catch (ArgumentException)
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
        private void Created(object sender, FileSystemEventArgs e)
        {
            Log.Information("File created: {FullPath}", e.FullPath);
        }

        private void Deleted(object sender, FileSystemEventArgs e)
        {
            Log.Information("File deleted: {FullPath}", e.FullPath);
        }

        private void Renamed(object sender, FileSystemEventArgs e)
        {
            Log.Information("File renamed: {FullPath}", e.FullPath);
        }

        private void Changed(object sender, FileSystemEventArgs e)
        {
            Log.Information("File changed: {FullPath}", e.FullPath);
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
            loggerContext.Dispose();

            disposed = true;

            GC.SuppressFinalize(this);
        }
    }

    public class WatcherManager : BackgroundService
    {
        private readonly ManagerConfiguration _managerConfiguration;
        private List<FsWatcherDisposable> fsWatchers = new List<FsWatcherDisposable>();

        public WatcherManager(ManagerConfiguration managerConfiguration)
        {
            _managerConfiguration = managerConfiguration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var watchers = await _managerConfiguration.GetWatchers();
            while (!stoppingToken.IsCancellationRequested)
            {
                Log.Information("Reloading watchers from database");

                bool watchersChanged = await _managerConfiguration.WatchersChanged();
                if (watchersChanged) {
                    fsWatchers.ForEach(fsWatcher => fsWatcher.Dispose());
                    fsWatchers = new List<FsWatcherDisposable>();

                    foreach (var watcher in watchers)
                    {
                        try
                        {
                            var fsWatcherDisposable = new FsWatcherDisposable(watcher);

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
                await Task.Delay(30000);
            }
        }
    }
}