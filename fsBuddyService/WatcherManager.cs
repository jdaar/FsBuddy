using Serilog;
using Configuration;
using Serilog.Context;
using ConnectionInterface;

namespace Service
{

    class WatcherManager : BackgroundService
    {
        private readonly ManagerConfiguration _managerConfiguration;
        private ThreadManager? _threadManager;

        private int threadNumber;

        public WatcherManager(ManagerConfiguration managerConfiguration)
        {
            _managerConfiguration = managerConfiguration;
        }
        private async Task RetrieveServiceSettings()
        {
            threadNumber = (await _managerConfiguration.GetServiceSetting(SettingType.THREAD_NUMBER))?.Value ?? 1;
            Log.Information("Setting ThreadNumber: {threadNumber}", threadNumber);

            _threadManager = new ThreadManager(threadNumber);
        }

        public void RefreshThreads(List<Watcher> watchers)
        {
            if (_threadManager == null) return;

            _threadManager.StopThreads();
            _threadManager.InitializeThreads(watchers);
            _threadManager.IsRefreshRequired = true;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RetrieveServiceSettings();

            var watchers = await _managerConfiguration.GetWatchers();
            RefreshThreads(watchers);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_threadManager == null) return;


                if (_threadManager.IsRefreshRequired)
                {
                    _threadManager.Start();
                }
            }
        }

    }
}