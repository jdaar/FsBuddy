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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RetrieveServiceSettings();
            
            if (_threadManager == null) return;

            var watchers = await _managerConfiguration.GetWatchers();

            _threadManager.InitializeThreads(watchers);
            _threadManager.Start();
        }

    }
}