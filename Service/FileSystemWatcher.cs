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

        private static Func<int, Task>? RegisterExecutionCallback;
        private static bool IsWatcherActionsInitialized { get; set; } = false;

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

                    var splitFileName = fsEvent.Name.Split('.');
                    var partFilter = $"{String.Join(".", splitFileName.Take(splitFileName.Length - 1))}*.part";

                    var fileExists = File.Exists(fsEvent.FullPath);

                    Log.Information("{InputPath} searching for file {partFilter}", watcher.InputPath, partFilter);

                    var fileIsDownloading = Directory.GetFiles(watcher.InputPath!, partFilter).Length > 0;

                    var fileOutputExists = File.Exists(Path.Combine(watcher.OutputPath, fsEvent.Name));

                    var fileIsEmpty = new FileInfo(fsEvent.FullPath).Length == 0;

                    Log.Information("The file {partFilter} exists: {fileIsDownloading}", partFilter, fileIsDownloading);

                    if (!fileExists)
                    {
                        Log.Information("File does not longer exist");
                        return;
                    }
                    if (fileIsDownloading)
                    {
                        Log.Information("File is not ready yet");
                        return;
                    }
                    if (fileIsEmpty)
                    {
                        Log.Information("File is empty");
                        return;
                    }
                    if (!fileOutputExists)
                    {
                        try
                        {
                            File.Move(fsEvent.FullPath, Path.Combine(watcher.OutputPath, fsEvent.Name));
                        }
                        catch (Exception error) 
                        {
                            Log.Error(error, "Couldn't execute MOVE action");
                        }
                    } else
                    {
                        var splitFilename = fsEvent.Name.Split('.');

                        var fileExtension = splitFilename.Last();
                        var fileName = String.Join('.', splitFilename.Take(splitFilename.Length - 1));

                        var fileIndex = Directory.GetFiles(watcher.OutputPath, $"{fileName}*.{fileExtension}").Length;

                        fileName = $"{fileName} ({fileIndex}).{fileExtension}";

                        Log.Information("Attempting to execute MOVE action with filename {fileName}", fileName);
                        try
                        {
                            File.Move(fsEvent.FullPath, Path.Combine(watcher.OutputPath, fileName));
                        } catch (Exception error) 
                        {
                            Log.Error(error, "Couldn't execute MOVE action");
                        }
                    }
                    RegisterExecutionCallback?.Invoke(watcher.Id);
                }
            );
            IsWatcherActionsInitialized = true;
        }

        public FileSystemWatcherDisposable(Watcher watcher, Func<int, Task> registerExecutionCallback)
        {
            if (!IsWatcherActionsInitialized)
            {
                PopulateWatcherActions();
            }

            RegisterExecutionCallback = registerExecutionCallback;

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
