using Service;
using Serilog;
using Configuration;
using Serilog.Formatting.Compact;
using Serilog.Core;
using System.ComponentModel.Design;
using Microsoft.Extensions.Logging.EventLog;

namespace Service
{
    class FileSystemService
    {
        private string appDirectoryPath;
        private string logDirectoryPath;
        public IHostBuilder _hostBuilder;

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

            _hostBuilder = Host
                .CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddSerilog(Log.Logger);
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ManagerConfiguration>();

                    services.AddHostedService<PipeManager>();

                    services.AddSingleton<WatcherManager>();
                    services.AddHostedService(provider => provider.GetService<WatcherManager>());

                    if (OperatingSystem.IsWindows())
                    {
                        services.Configure<EventLogSettings>(config =>
                        {
                            config.LogName = "fsBuddy";
                            config.SourceName = "fsBuddy";
                        });
                    }
                })
                .UseWindowsService();
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
