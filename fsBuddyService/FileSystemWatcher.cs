using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Configuration;
using Serilog;
using Serilog.Context;
using System.IO;
using ConnectionInterface;

namespace Service
{


    class FileSystemWatcherDisposable : IDisposable
    {
        private bool disposed = false;
        private Watcher _watcher;

        private static Dictionary<t_WatcherAction, Action<Watcher, FileSystemEventArgs>> WatcherActions = new();

        public FileSystemWatcher? fsWatcher;

        private void PopulateWatcherActions() {
            WatcherActions.Add(
                t_WatcherAction.MOVE, 
                delegate (Watcher watcher, FileSystemEventArgs fsEvent)
                {
                    Log.Information("From watcher action {@watcher} and event {@fsEvent}", watcher, fsEvent);

                    if (watcher.OutputPath == null)
                        return;
                    if (fsEvent.Name == null)
                        return;

                    try
                    {
                        File.Move(fsEvent.FullPath, Path.Combine(watcher.OutputPath, fsEvent.Name));
                    }
                    catch (IOException)
                    {
                        var splitFilename = fsEvent.Name.Split('.');

                        var fileExtension = splitFilename.Last();
                        var fileName = String.Join('.', splitFilename.Take(splitFilename.Length - 1));
                        fileName = $"{fileName} (new).{fileExtension}";

                        Log.Information("Attempting to execute MOVE action with filename {fileName}", fileName);

                        try
                        {
                            File.Move(fsEvent.FullPath, Path.Combine(watcher.OutputPath, fileName));
                        } catch (Exception error) 
                        {
                            Log.Error(error, "Couldn't execute MOVE action");
                        }
                    }
                    catch (Exception error) 
                    {
                        Log.Error(error, "Couldn't execute MOVE action");
                    }
                }
            );
        }

        public FileSystemWatcherDisposable(Watcher watcher)
        {
            if (WatcherActions.Count() == 0)
            {
                PopulateWatcherActions();
            }

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
                    fsWatcher.Renamed += Renamed;

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
                var action = WatcherActions.GetValueOrDefault(_watcher.Action);
                if (action != null)
                {
                    action.Invoke(_watcher, e);
                }
            }
        }

        private void Renamed(object sender, FileSystemEventArgs e)
        {
            using (LogContext.PushProperty("WatcherId", _watcher.Id))
            {
                Log.Information("File renamed: {FullPath}", e.FullPath);
                var action = WatcherActions.GetValueOrDefault(_watcher.Action);
                if (action != null)
                {
                    action.Invoke(_watcher, e);
                }
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
