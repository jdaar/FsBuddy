using Service;
using Serilog;
using Configuration;
using Serilog.Formatting.Compact;
using Serilog.Core;

namespace Service
{
    public class Service
    {
        private string appDirectoryPath;
        private string logDirectoryPath;
        private IHost _host;

        public Logger LoggerFactory()
        {
            var logFilePath = Path.Join(logDirectoryPath, $"service_.log");

            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(
                    new CompactJsonFormatter(), 
                    logFilePath,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true
                )
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        public Service()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);

            appDirectoryPath = Path.Join(path, "fsBuddy"); 
            logDirectoryPath = Path.Join(appDirectoryPath, "logs");

            Directory.CreateDirectory(appDirectoryPath);
            Directory.CreateDirectory(logDirectoryPath);

            Log.Logger = LoggerFactory();

            _host = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddSerilog(Log.Logger);
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ManagerConfiguration>();
                    services.AddHostedService<WatcherManager>();
                })
                .Build();
        }

        public async Task StartAsync()
        {
            await _host.StartAsync();
        }
    }
}
