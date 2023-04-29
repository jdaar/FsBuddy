using Configuration;

namespace Service
{
    public class WatcherManager : BackgroundService
    {
        private readonly ILogger<WatcherManager> _logger;
        private readonly ManagerConfiguration _managerConfiguration;

        public WatcherManager(ILogger<WatcherManager> logger, ManagerConfiguration managerConfiguration)
        {
            _logger = logger;
            _managerConfiguration = managerConfiguration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}