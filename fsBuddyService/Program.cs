using Service;
using Serilog;
using Configuration;
using Serilog.Formatting.Compact;

// App folder creation

var folder = Environment.SpecialFolder.LocalApplicationData;
var path = Environment.GetFolderPath(folder);

var appDirectoryPath = Path.Join(path, "fsBuddy"); 
var logDirectoryPath = Path.Join(appDirectoryPath, "logs");

Directory.CreateDirectory(appDirectoryPath);
Directory.CreateDirectory(logDirectoryPath);

// Logs

var logFilePath = Path.Join(logDirectoryPath, $"service_.log");

Log.Logger = new LoggerConfiguration()
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


IHost host = Host.CreateDefaultBuilder(args)
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

await host.RunAsync();
