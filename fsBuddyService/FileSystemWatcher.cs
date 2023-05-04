using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Configuration;
using Serilog;
using Serilog.Context;
using ConnectionInterface;

namespace Service
{
    class FileSystemWatcherDisposable : IDisposable
    {
        private bool disposed = false;
        private Watcher _watcher;

        public System.IO.FileSystemWatcher? fsWatcher;

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
                        fsWatcher = new System.IO.FileSystemWatcher(watcher.InputPath);
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

}
