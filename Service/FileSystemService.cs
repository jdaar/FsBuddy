using Service;
using Serilog;
using Configuration;
using Serilog.Formatting.Compact;
using Serilog.Core;
using System.ComponentModel.Design;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace Service
{
    class FileSystemService
    {
        private readonly string appDirectoryPath;
        private readonly string logDirectoryPath;
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
                    services.AddDbContext<ConfigurationContext>(
                        delegate(DbContextOptionsBuilder optionsBuilder) { 
                            var _folder = Environment.SpecialFolder.LocalApplicationData;
                            var _path = Environment.GetFolderPath(_folder);
                            var _dbPath = Path.Join(_path, "fsBuddy.config.db");

                            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = _dbPath };
                            var connectionString = connectionStringBuilder.ToString();
                            var connection = new SqliteConnection(connectionString);

                            Log.Information("Using {connectionString}", connectionString);

                            optionsBuilder.UseSqlite(connection);
                        },
                        ServiceLifetime.Scoped
                    );

                    services.AddTransient<ManagerConfiguration>();

                    services.AddSingleton<WatcherManager>();

                    services.AddHostedService<PipeManager>();
                    services.AddHostedService(provider => provider.GetService<WatcherManager>()!);

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

            using var scope = host.Services.CreateScope();
            using var context = scope.ServiceProvider.GetService<ConfigurationContext>();

            if (context == null)
            {
                Log.Fatal("Couldn't retrieve ConfigurationContext service");
                return;
            }

            await context.Database.EnsureCreatedAsync();

            await host.RunAsync();
        }
    }

}
