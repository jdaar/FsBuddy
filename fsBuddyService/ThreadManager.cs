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
    class ThreadManager
    {
        private List<Thread> threads = new List<Thread>();
        private List<FileSystemWatcherDisposable> fsWatchers = new List<FileSystemWatcherDisposable>();
        private Thread mainThread = Thread.CurrentThread;
        private readonly int ThreadNumber = 1;

        public ThreadManager(int threadNumber)
        {
            ThreadNumber = threadNumber;
        }

        public void Start()
        {
            foreach (var thread in threads)
            {
                thread.Start();
            }
        }
        public void StopThreads()
        {
            fsWatchers.ForEach(fsWatcher => fsWatcher.Dispose());
            threads.ForEach(thread => thread.Interrupt());
            threads = new List<Thread>();
            fsWatchers = new List<FileSystemWatcherDisposable>();
        }

        public void InitializeThreads(List<Watcher> watchers)
        {
            if (threads.Count != 0)
            {
                StopThreads();
            }

            if (watchers.Count == 0)
            {
                Log.Information("No watchers to initialize");
                return;
            }

            var threadWatcherCount = watchers.Count() / ThreadNumber;

            Log.Information("Creating threads with {threadWatcherCount} watchers", threadWatcherCount);

            foreach (var watcherChunk in watchers.Chunk(threadWatcherCount))
            {
                try
                {
                    Thread thread = ThreadFactory(watcherChunk.ToArray());
                    threads.Add(thread);
                } catch (ArgumentNullException error)
                {
                    Log.Error(error.Message);
                } catch (ArgumentException error)
                {
                    Log.Error(error.Message);
                } 
            }

        }

        public Thread ThreadFactory(Watcher[] watchers)
        {
            Thread thread = new Thread(() =>
            {
                lock (fsWatchers)
                {
                    using (LogContext.PushProperty("ThreadId", Thread.CurrentThread.ManagedThreadId))
                    {
                        Log.Information("Thread {ManagedThreadId} started", Thread.CurrentThread.ManagedThreadId);
                        foreach (var watcher in watchers)
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
                        }
                    }
                }
            });

            return thread;
        }
    }
}
