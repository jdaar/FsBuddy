using Service;
using Serilog;
using Configuration;
using Serilog.Formatting.Compact;
using Serilog.Core;
using System.ComponentModel.Design;

namespace Service
{
    class FileSystemService
    {
        private string appDirectoryPath;
        private string logDirectoryPath;
        private IHostBuilder _hostBuilder;

        private Logger LoggerFactory()
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

        public FileSystemService()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);

            appDirectoryPath = Path.Join(path, "fsBuddy"); 
            logDirectoryPath = Path.Join(appDirectoryPath, "logs");

            Directory.CreateDirectory(appDirectoryPath);
            Directory.CreateDirectory(logDirectoryPath);

            Log.Logger = LoggerFactory();

            _hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddSerilog(Log.Logger);
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ManagerConfiguration>();

                    services.AddSingleton<WatcherManager>();
                    services.AddHostedService(provider => provider.GetService<WatcherManager>());

                    services.AddHostedService<PipeManager>();
                });
        }

        public async Task RunAsync()
        {
            var host = _hostBuilder.Build();
            ManagerConfiguration? _manager = host.Services.GetService<ManagerConfiguration>();

            if (_manager == null) { 
                Log.Fatal("Couldn't retrieve ManagerConfiguration service");
                return;
            };

            var serviceSettingsExists = await _manager.ServiceSettingsExists();

            if (!serviceSettingsExists)
            {
                Log.Information("Recreating service settings table");
                await _manager.CreateDefaultServiceSettings();
            }

            await host.RunAsync();
        }
    }

}
